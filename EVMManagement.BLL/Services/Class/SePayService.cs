using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Payment;
using EVMManagement.BLL.DTOs.Response.Payment;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Options;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.Extensions.Options;

namespace EVMManagement.BLL.Services.Class
{
    public class SePayService : ISePayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SePaySettings _sePaySettings;

        public SePayService(IUnitOfWork unitOfWork, IOptions<SePaySettings> sePaySettings)
        {
            _unitOfWork = unitOfWork;
            _sePaySettings = sePaySettings.Value;
        }

        public async Task<PaymentResponse> CreatePaymentUrlAsync(PaymentRequest request, string ipAddress)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                throw new Exception("Order not found");
            }

            // Validate amount
            if (request.Amount <= 0)
            {
                throw new Exception($"Invalid payment amount: {request.Amount}. Amount must be greater than 0.");
            }

            // SEPay minimum amount is 10,000 VND
            const decimal SEPAY_MIN_AMOUNT = 10000;
            if (request.Amount < SEPAY_MIN_AMOUNT)
            {
                throw new Exception($"Payment amount ({request.Amount:N0} VND) is less than SEPay minimum ({SEPAY_MIN_AMOUNT:N0} VND).");
            }

            var createDate = DateTime.UtcNow;
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var createDateVn = TimeZoneInfo.ConvertTimeFromUtc(createDate, vnTimeZone);
            
            // Transaction code format: {PREFIX}{timestamp}
            var transactionCode = $"{_sePaySettings.TransactionPrefix}{createDateVn:yyyyMMddHHmmss}";

            var sepay = new SePayLibrary();
            
            // Tạo nội dung chuyển khoản
            var transactionContent = sepay.CreateTransactionContent(
                _sePaySettings.TransactionPrefix,
                transactionCode,
                request.OrderInfo
            );

            // Tạo QR code URL cho khách hàng quét mã
            var qrCodeUrl = sepay.CreateQRCodeUrl(
                _sePaySettings.QRApiUrl,
                _sePaySettings.AccountNumber,
                _sePaySettings.AccountName,
                _sePaySettings.BankCode,
                request.Amount,
                transactionContent
            );

            // Create transaction record
            var transaction = new Transaction
            {
                Amount = request.Amount,
                Currency = "VND",
                Status = TransactionStatus.PENDING,
                TransactionTime = createDate,
                PaymentGateway = "SEPAY",
                VnpayTransactionCode = transactionCode,
                TransactionInfo = transactionContent
            };

            // Handle deposit or invoice
            if (request.IsDeposit)
            {
                var deposit = new Deposit
                {
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    Method = PaymentMethod.PAYMENT_GATEWAY,
                    Status = DepositStatus.PENDING
                };
                await _unitOfWork.Deposits.AddAsync(deposit);
                await _unitOfWork.SaveChangesAsync();
                transaction.DepositId = deposit.Id;
            }
            else
            {
                var invoice = order.Invoice;
                if (invoice == null)
                {
                    invoice = new Invoice
                    {
                        OrderId = request.OrderId,
                        InvoiceCode = $"INV{request.OrderId.ToString("N")[..8]}{DateTime.UtcNow:yyyyMMddHHmmss}",
                        TotalAmount = request.Amount,
                        Status = InvoiceStatus.DRAFT
                    };
                    await _unitOfWork.Invoices.AddAsync(invoice);
                    await _unitOfWork.SaveChangesAsync();
                }
                transaction.InvoiceId = invoice.Id;
            }

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return new PaymentResponse
            {
                PaymentUrl = qrCodeUrl, // Trả về QR code URL thay vì payment URL
                TransactionCode = transactionCode,
                OrderId = request.OrderId,
                Amount = request.Amount,
                OrderInfo = transactionContent,
                CreatedDate = createDate,
                PaymentGateway = "SEPAY"
            };
        }

        public async Task<PaymentCallbackResponse> ProcessCallbackAsync(Dictionary<string, string> callbackData)
        {
            // Log raw callback data for debugging
            Console.WriteLine("=== SEPay Callback Received ===");
            foreach (var kvp in callbackData)
            {
                Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
            }

            var sepay = new SePayLibrary();
            sepay.ParseResponseData(callbackData);

            // Validate webhook signature nếu có WebhookSecret
            if (!string.IsNullOrEmpty(_sePaySettings.WebhookSecret))
            {
                var signature = sepay.GetResponseData("signature");
                var payload = sepay.GetResponseData("payload");
                
                if (!sepay.ValidateWebhookSignature(payload, signature, _sePaySettings.WebhookSecret))
                {
                    Console.WriteLine("ERROR: Invalid webhook signature");
                    return new PaymentCallbackResponse
                    {
                        Success = false,
                        Message = "Invalid webhook signature",
                        ResponseCode = "97",
                        PaymentGateway = "SEPAY"
                    };
                }
            }

            // Parse webhook data từ SEPay
            // SEPay real webhook fields: content, id, transferAmount, transactionDate, gateway, referenceCode
            // Old mock fields: transaction_content, transaction_id, amount_in, transaction_date
            var transactionCode = sepay.GetResponseData("content") 
                                ?? sepay.GetResponseData("transaction_content");
            var gatewayTransactionNo = sepay.GetResponseData("id") 
                                     ?? sepay.GetResponseData("referenceCode")
                                     ?? sepay.GetResponseData("transaction_id");
            var amountStr = sepay.GetResponseData("transferAmount") 
                          ?? sepay.GetResponseData("amount_in") 
                          ?? sepay.GetResponseData("amount");
            var amount = !string.IsNullOrEmpty(amountStr) ? decimal.Parse(amountStr) : 0;
            var bankCode = sepay.GetResponseData("gateway") ?? _sePaySettings.BankCode;
            var description = sepay.GetResponseData("description");
            var payDateStr = sepay.GetResponseData("transactionDate") 
                           ?? sepay.GetResponseData("transaction_date") 
                           ?? sepay.GetResponseData("when");
            
            Console.WriteLine($"Parsed - Transaction Content: '{transactionCode}'");
            Console.WriteLine($"Parsed - Gateway Transaction No: '{gatewayTransactionNo}'");
            Console.WriteLine($"Parsed - Amount: {amount}");
            Console.WriteLine($"Parsed - Bank: '{bankCode}'");
            
            DateTime payDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(payDateStr))
            {
                DateTime.TryParse(payDateStr, out payDate);
            }

            // Extract transaction code from content (format: PREFIX YYYYMMDDHHMMSS Order Info)
            var extractedTransactionCode = ExtractTransactionCode(transactionCode, _sePaySettings.TransactionPrefix);
            
            Console.WriteLine($"Extracted Transaction Code: '{extractedTransactionCode}'");
            Console.WriteLine($"Transaction Prefix used: '{_sePaySettings.TransactionPrefix}'");
            
            if (string.IsNullOrEmpty(extractedTransactionCode))
            {
                Console.WriteLine("ERROR: Failed to extract transaction code from content");
                return new PaymentCallbackResponse
                {
                    Success = false,
                    Message = $"Invalid transaction content format. Content received: '{transactionCode}', Expected prefix: '{_sePaySettings.TransactionPrefix}'",
                    ResponseCode = "02",
                    TransactionCode = transactionCode,
                    PaymentGateway = "SEPAY"
                };
            }

            // Find transaction
            Console.WriteLine($"Searching for transaction with VnpayTransactionCode = '{extractedTransactionCode}'");
            var transaction = _unitOfWork.Transactions.GetQueryable()
                .FirstOrDefault(t => t.VnpayTransactionCode == extractedTransactionCode);

            if (transaction == null)
            {
                Console.WriteLine("ERROR: Transaction not found in database");
                Console.WriteLine("Trying to list all transactions with similar codes...");
                
                var similarTransactions = _unitOfWork.Transactions.GetQueryable()
                    .Where(t => t.VnpayTransactionCode.Contains(_sePaySettings.TransactionPrefix))
                    .Select(t => new { t.Id, t.VnpayTransactionCode, t.Status })
                    .Take(10)
                    .ToList();
                    
                Console.WriteLine($"Found {similarTransactions.Count} transactions with prefix '{_sePaySettings.TransactionPrefix}':");
                foreach (var t in similarTransactions)
                {
                    Console.WriteLine($"  - Id: {t.Id}, Code: '{t.VnpayTransactionCode}', Status: {t.Status}");
                }
                
                return new PaymentCallbackResponse
                {
                    Success = false,
                    Message = "Transaction not found",
                    ResponseCode = "01",
                    TransactionCode = extractedTransactionCode,
                    PaymentGateway = "SEPAY"
                };
            }

            // Check if transaction already processed
            if (transaction.Status == TransactionStatus.SUCCESS)
            {
                return new PaymentCallbackResponse
                {
                    Success = true,
                    Message = "Transaction already processed",
                    ResponseCode = "00",
                    TransactionCode = extractedTransactionCode,
                    GatewayTransactionNo = transaction.VnpayTransactionNo ?? string.Empty,
                    Amount = transaction.Amount,
                    OrderId = GetOrderId(transaction),
                    TransactionId = transaction.Id,
                    PaymentGateway = "SEPAY"
                };
            }

            // Validate amount
            if (Math.Abs(amount - transaction.Amount) > 1)
            {
                return new PaymentCallbackResponse
                {
                    Success = false,
                    Message = $"Amount mismatch. Expected: {transaction.Amount}, Received: {amount}",
                    ResponseCode = "04",
                    TransactionCode = extractedTransactionCode,
                    Amount = amount,
                    PaymentGateway = "SEPAY"
                };
            }

            // Update transaction - Payment từ SEPay luôn là thành công khi nhận được webhook
            transaction.VnpayTransactionNo = gatewayTransactionNo;
            transaction.BankCode = bankCode;
            transaction.ResponseCode = "success";
            transaction.Status = TransactionStatus.SUCCESS;
            transaction.TransactionTime = payDate;
            transaction.ModifiedDate = DateTime.UtcNow;

            // Update deposit status
            if (transaction.DepositId.HasValue)
            {
                var deposit = await _unitOfWork.Deposits.GetByIdAsync(transaction.DepositId.Value);
                if (deposit != null)
                {
                    deposit.Status = DepositStatus.PAID;
                    deposit.ModifiedDate = DateTime.UtcNow;
                    _unitOfWork.Deposits.Update(deposit);

                    // Update Order status to IN_PROGRESS after successful deposit
                    var order = await _unitOfWork.Orders.GetByIdAsync(deposit.OrderId);
                    if (order != null && order.Status == OrderStatus.AWAITING_DEPOSIT)
                    {
                        order.Status = OrderStatus.IN_PROGRESS;
                        order.ModifiedDate = DateTime.UtcNow;
                        _unitOfWork.Orders.Update(order);
                    }
                }
            }

            // Update invoice status
            if (transaction.InvoiceId.HasValue)
            {
                var invoice = await _unitOfWork.Invoices.GetByIdAsync(transaction.InvoiceId.Value);
                if (invoice != null)
                {
                    invoice.Status = InvoiceStatus.PAID;
                    invoice.ModifiedDate = DateTime.UtcNow;
                    _unitOfWork.Invoices.Update(invoice);

                    var order = await _unitOfWork.Orders.GetByIdAsync(invoice.OrderId);
                    if (order != null)
                    {
                        order.Status = OrderStatus.PAY_SUCCESS;
                        order.ModifiedDate = DateTime.UtcNow;
                        _unitOfWork.Orders.Update(order);
                    }
                }
            }

            _unitOfWork.Transactions.Update(transaction);
            await _unitOfWork.SaveChangesAsync();

            return new PaymentCallbackResponse
            {
                Success = true,
                ResponseCode = "00",
                TransactionCode = extractedTransactionCode,
                GatewayTransactionNo = gatewayTransactionNo,
                Amount = amount,
                BankCode = bankCode,
                OrderInfo = description,
                PayDate = payDate,
                OrderId = GetOrderId(transaction),
                TransactionId = transaction.Id,
                PaymentGateway = "SEPAY",
                Message = "Payment successful"
            };
        }

        private string ExtractTransactionCode(string content, string prefix)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine("ERROR: Content is null or empty");
                return string.Empty;
            }

            var trimmedContent = content.Trim();
            Console.WriteLine($"Trying to extract from content: '{trimmedContent}'");
            Console.WriteLine($"Expected prefix: '{prefix}'");
            
            // Method 1: No spaces - simple format "EVM20251113221731"
            if (!trimmedContent.Contains(' '))
            {
                Console.WriteLine("Format: No spaces detected");
                if (trimmedContent.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"✓ Matched! Returning: '{trimmedContent}'");
                    return trimmedContent;
                }
                Console.WriteLine($"✗ Prefix mismatch. Content starts with: '{trimmedContent.Substring(0, Math.Min(prefix.Length, trimmedContent.Length))}'");
            }
            
            // Method 2: With spaces - old format "EVMGT EVMGT20251113221731 Info"
            Console.WriteLine("Format: Spaces detected, trying to parse");
            var parts = content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine($"Split into {parts.Length} parts");
            
            if (parts.Length >= 2)
            {
                Console.WriteLine($"Part[0]: '{parts[0]}', Part[1]: '{parts[1]}'");
                
                if (parts[0].Equals(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"✓ Prefix matched! Returning: '{parts[1]}'");
                    return parts[1];
                }
                
                // Fallback: Check if parts[1] starts with prefix
                if (parts[1].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"✓ Part[1] starts with prefix! Returning: '{parts[1]}'");
                    return parts[1];
                }
            }
            
            // Method 3: Fallback - try to find any substring starting with prefix + 14 digits
            Console.WriteLine("Trying regex fallback to find transaction code...");
            var pattern = $@"{prefix}\d{{14}}";
            var match = System.Text.RegularExpressions.Regex.Match(trimmedContent, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                Console.WriteLine($"✓ Regex match found! Returning: '{match.Value}'");
                return match.Value;
            }
            
            Console.WriteLine("✗ All extraction methods failed");
            return string.Empty;
        }

        private Guid? GetOrderId(Transaction transaction)
        {
            if (transaction.InvoiceId.HasValue)
            {
                var invoice = _unitOfWork.Invoices.GetQueryable()
                    .FirstOrDefault(i => i.Id == transaction.InvoiceId.Value);
                return invoice?.OrderId;
            }
            
            if (transaction.DepositId.HasValue)
            {
                var deposit = _unitOfWork.Deposits.GetQueryable()
                    .FirstOrDefault(d => d.Id == transaction.DepositId.Value);
                return deposit?.OrderId;
            }
            
            return null;
        }

        public async Task<PaymentCallbackResponse> ProcessReturnUrlAsync(Dictionary<string, string> returnData)
        {
            // SEPay return URL chỉ dùng để check status, payment đã được xử lý qua webhook
            var transactionCode = returnData.ContainsKey("transaction_code") 
                ? returnData["transaction_code"] 
                : string.Empty;

            if (string.IsNullOrEmpty(transactionCode))
            {
                return new PaymentCallbackResponse
                {
                    Success = false,
                    Message = "Missing transaction code",
                    ResponseCode = "99",
                    PaymentGateway = "SEPAY"
                };
            }

            var transaction = _unitOfWork.Transactions.GetQueryable()
                .FirstOrDefault(t => t.VnpayTransactionCode == transactionCode);

            if (transaction == null)
            {
                return new PaymentCallbackResponse
                {
                    Success = false,
                    Message = "Transaction not found",
                    ResponseCode = "01",
                    TransactionCode = transactionCode,
                    PaymentGateway = "SEPAY"
                };
            }

            var isSuccess = transaction.Status == TransactionStatus.SUCCESS;

            return new PaymentCallbackResponse
            {
                Success = isSuccess,
                ResponseCode = isSuccess ? "00" : transaction.ResponseCode ?? "99",
                TransactionCode = transactionCode,
                GatewayTransactionNo = transaction.VnpayTransactionNo ?? string.Empty,
                Amount = transaction.Amount,
                BankCode = transaction.BankCode ?? string.Empty,
                OrderInfo = transaction.TransactionInfo ?? string.Empty,
                PayDate = transaction.TransactionTime,
                OrderId = GetOrderId(transaction),
                TransactionId = transaction.Id,
                PaymentGateway = "SEPAY",
                Message = isSuccess ? "Payment successful" : "Payment pending or failed"
            };
        }

        public async Task<PaymentCallbackResponse> CheckTransactionStatusAsync(string transactionCode)
        {
            if (string.IsNullOrEmpty(transactionCode))
            {
                throw new Exception("Transaction code is required");
            }

            var transaction = _unitOfWork.Transactions.GetQueryable()
                .FirstOrDefault(t => t.VnpayTransactionCode == transactionCode);

            if (transaction == null)
            {
                throw new Exception($"Transaction not found: {transactionCode}");
            }

            var isSuccess = transaction.Status == TransactionStatus.SUCCESS;

            return new PaymentCallbackResponse
            {
                Success = isSuccess,
                ResponseCode = isSuccess ? "00" : (transaction.Status == TransactionStatus.PENDING ? "01" : "99"),
                TransactionCode = transactionCode,
                GatewayTransactionNo = transaction.VnpayTransactionNo ?? string.Empty,
                Amount = transaction.Amount,
                BankCode = transaction.BankCode ?? string.Empty,
                OrderInfo = transaction.TransactionInfo ?? string.Empty,
                PayDate = transaction.TransactionTime,
                OrderId = GetOrderId(transaction),
                TransactionId = transaction.Id,
                PaymentGateway = "SEPAY",
                Message = GetStatusMessage(transaction.Status)
            };
        }

        private string GetStatusMessage(TransactionStatus status)
        {
            return status switch
            {
                TransactionStatus.SUCCESS => "Thanh toán thành công",
                TransactionStatus.PENDING => "Đang chờ thanh toán",
                TransactionStatus.FAILED => "Thanh toán thất bại",
                _ => "Trạng thái không xác định"
            };
        }

        private string GetResponseMessage(string? responseCode)
        {
            if (string.IsNullOrEmpty(responseCode))
                return "Lỗi không xác định";

            return responseCode.ToLower() switch
            {
                "success" or "00" => "Giao dịch thành công",
                "pending" => "Giao dịch đang xử lý",
                "failed" => "Giao dịch thất bại",
                "canceled" => "Giao dịch đã bị hủy",
                "expired" => "Giao dịch đã hết hạn",
                "insufficient_balance" => "Số dư không đủ",
                "invalid_card" => "Thẻ không hợp lệ",
                "bank_error" => "Lỗi từ ngân hàng",
                _ => "Lỗi không xác định"
            };
        }
    }
}
