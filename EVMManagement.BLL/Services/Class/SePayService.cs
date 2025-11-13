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
            
            // Transaction code format: ORD{orderId_8chars}{timestamp}
            var transactionCode = $"ORD{request.OrderId.ToString("N")[..8]}{createDateVn:yyyyMMddHHmmss}";

            var sepay = new SePayLibrary();
            
            // Add required parameters for SEPay
            sepay.AddRequestData("merchant_code", _sePaySettings.MerchantCode);
            sepay.AddRequestData("order_id", transactionCode);
            sepay.AddRequestData("amount", ((long)request.Amount).ToString());
            sepay.AddRequestData("currency", _sePaySettings.Currency);
            sepay.AddRequestData("order_info", request.OrderInfo);
            sepay.AddRequestData("return_url", _sePaySettings.ReturnUrl);
            sepay.AddRequestData("callback_url", _sePaySettings.CallbackUrl);
            sepay.AddRequestData("locale", request.Locale ?? _sePaySettings.Locale);
            sepay.AddRequestData("created_at", createDateVn.ToString("yyyy-MM-dd HH:mm:ss"));
            sepay.AddRequestData("version", _sePaySettings.Version);

            // Optional bank code
            if (!string.IsNullOrEmpty(request.BankCode))
            {
                sepay.AddRequestData("bank_code", request.BankCode);
            }

            var paymentUrl = sepay.CreateRequestUrl(
                _sePaySettings.PaymentUrl, 
                _sePaySettings.SecretKey,
                _sePaySettings.SignatureType
            );

            // Create transaction record
            var transaction = new Transaction
            {
                Amount = request.Amount,
                Currency = "VND",
                Status = TransactionStatus.PENDING,
                TransactionTime = createDate,
                PaymentGateway = "SEPAY",
                VnpayTransactionCode = transactionCode, // Reuse this field for SEPay transaction code
                TransactionInfo = request.OrderInfo
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
                PaymentUrl = paymentUrl,
                TransactionCode = transactionCode,
                OrderId = request.OrderId,
                Amount = request.Amount,
                OrderInfo = request.OrderInfo,
                CreatedDate = createDate,
                PaymentGateway = "SEPAY"
            };
        }

        public async Task<PaymentCallbackResponse> ProcessCallbackAsync(Dictionary<string, string> callbackData)
        {
            var sepay = new SePayLibrary();
            sepay.ParseResponseData(callbackData);

            var orderIdFromGateway = sepay.GetResponseData("order_id");
            var gatewayTransactionNo = sepay.GetResponseData("transaction_id");
            var responseCode = sepay.GetResponseData("status");
            var signature = callbackData.ContainsKey("signature") ? callbackData["signature"] : "";
            var amount = decimal.Parse(sepay.GetResponseData("amount"));
            var bankCode = sepay.GetResponseData("bank_code");
            var cardType = sepay.GetResponseData("card_type");
            var orderInfo = sepay.GetResponseData("order_info");
            var payDateStr = sepay.GetResponseData("paid_at");

            // Validate signature
            var isValidSignature = sepay.ValidateSignature(signature, _sePaySettings.SecretKey, _sePaySettings.SignatureType);
            if (!isValidSignature)
            {
                return new PaymentCallbackResponse
                {
                    Success = false,
                    Message = "Invalid signature",
                    ResponseCode = "97",
                    PaymentGateway = "SEPAY"
                };
            }

            // Find transaction
            var transaction = _unitOfWork.Transactions.GetQueryable()
                .FirstOrDefault(t => t.VnpayTransactionCode == orderIdFromGateway);

            if (transaction == null)
            {
                return new PaymentCallbackResponse
                {
                    Success = false,
                    Message = "Transaction not found",
                    ResponseCode = "01",
                    TransactionCode = orderIdFromGateway,
                    PaymentGateway = "SEPAY"
                };
            }

            // Update transaction
            transaction.VnpayTransactionNo = gatewayTransactionNo; // Reuse field for SEPay transaction no
            transaction.BankCode = bankCode;
            transaction.CardType = cardType;
            transaction.ResponseCode = responseCode;
            transaction.SecureHash = signature;
            transaction.ModifiedDate = DateTime.UtcNow;

            DateTime payDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(payDateStr))
            {
                DateTime.TryParse(payDateStr, out payDate);
            }

            // Check if payment is successful (SEPay uses "success" or "00" for success)
            bool isSuccess = responseCode?.ToLower() == "success" || responseCode == "00";

            if (isSuccess)
            {
                transaction.Status = TransactionStatus.SUCCESS;
                transaction.TransactionTime = payDate;

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
            }
            else
            {
                transaction.Status = TransactionStatus.FAILED;
            }

            _unitOfWork.Transactions.Update(transaction);
            await _unitOfWork.SaveChangesAsync();

            // Get order ID
            Guid? orderId = null;
            if (transaction.InvoiceId.HasValue)
            {
                var invoice = await _unitOfWork.Invoices.GetByIdAsync(transaction.InvoiceId.Value);
                orderId = invoice?.OrderId;
            }
            else if (transaction.DepositId.HasValue)
            {
                var deposit = await _unitOfWork.Deposits.GetByIdAsync(transaction.DepositId.Value);
                orderId = deposit?.OrderId;
            }

            return new PaymentCallbackResponse
            {
                Success = isSuccess,
                ResponseCode = responseCode ?? "99",
                TransactionCode = orderIdFromGateway,
                GatewayTransactionNo = gatewayTransactionNo,
                Amount = amount,
                BankCode = bankCode,
                CardType = cardType,
                OrderInfo = orderInfo,
                PayDate = payDate,
                OrderId = orderId,
                TransactionId = transaction.Id,
                PaymentGateway = "SEPAY",
                Message = isSuccess ? "Payment successful" : GetResponseMessage(responseCode)
            };
        }

        public async Task<PaymentCallbackResponse> ProcessReturnUrlAsync(Dictionary<string, string> returnData)
        {
            var sepay = new SePayLibrary();
            sepay.ParseResponseData(returnData);

            var orderIdFromGateway = sepay.GetResponseData("order_id");
            var gatewayTransactionNo = sepay.GetResponseData("transaction_id");
            var responseCode = sepay.GetResponseData("status");
            var signature = returnData.ContainsKey("signature") ? returnData["signature"] : "";
            var amount = decimal.Parse(sepay.GetResponseData("amount"));
            var bankCode = sepay.GetResponseData("bank_code");
            var cardType = sepay.GetResponseData("card_type");
            var orderInfo = sepay.GetResponseData("order_info");
            var payDateStr = sepay.GetResponseData("paid_at");

            // Validate signature
            var isValidSignature = sepay.ValidateSignature(signature, _sePaySettings.SecretKey, _sePaySettings.SignatureType);

            DateTime payDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(payDateStr))
            {
                DateTime.TryParse(payDateStr, out payDate);
            }

            var transaction = _unitOfWork.Transactions.GetQueryable()
                .FirstOrDefault(t => t.VnpayTransactionCode == orderIdFromGateway);

            Guid? orderId = null;
            bool isSuccess = responseCode?.ToLower() == "success" || responseCode == "00";

            // Update transaction if payment is successful and signature is valid
            if (isValidSignature && isSuccess && transaction != null)
            {
                transaction.VnpayTransactionNo = gatewayTransactionNo;
                transaction.BankCode = bankCode;
                transaction.CardType = cardType;
                transaction.ResponseCode = responseCode;
                transaction.SecureHash = signature;
                transaction.Status = TransactionStatus.SUCCESS;
                transaction.TransactionTime = payDate;
                transaction.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.Transactions.Update(transaction);

                if (transaction.DepositId.HasValue)
                {
                    var deposit = await _unitOfWork.Deposits.GetByIdAsync(transaction.DepositId.Value);
                    if (deposit != null && deposit.Status != DepositStatus.PAID)
                    {
                        deposit.Status = DepositStatus.PAID;
                        deposit.ModifiedDate = DateTime.UtcNow;
                        _unitOfWork.Deposits.Update(deposit);

                        var order = await _unitOfWork.Orders.GetByIdAsync(deposit.OrderId);
                        if (order != null && order.Status == OrderStatus.AWAITING_DEPOSIT)
                        {
                            order.Status = OrderStatus.IN_PROGRESS;
                            order.ModifiedDate = DateTime.UtcNow;
                            _unitOfWork.Orders.Update(order);
                        }

                        orderId = deposit.OrderId;
                    }
                }
                else if (transaction.InvoiceId.HasValue)
                {
                    var invoice = await _unitOfWork.Invoices.GetByIdAsync(transaction.InvoiceId.Value);
                    if (invoice != null && invoice.Status != InvoiceStatus.PAID)
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

                        orderId = invoice.OrderId;
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                // Just get orderId for response
                if (transaction?.InvoiceId.HasValue == true)
                {
                    var invoice = _unitOfWork.Invoices.GetQueryable().FirstOrDefault(i => i.Id == transaction.InvoiceId.Value);
                    orderId = invoice?.OrderId;
                }
                else if (transaction?.DepositId.HasValue == true)
                {
                    var deposit = _unitOfWork.Deposits.GetQueryable().FirstOrDefault(d => d.Id == transaction.DepositId.Value);
                    orderId = deposit?.OrderId;
                }
            }

            return new PaymentCallbackResponse
            {
                Success = isValidSignature && isSuccess,
                ResponseCode = responseCode ?? "99",
                TransactionCode = orderIdFromGateway,
                GatewayTransactionNo = gatewayTransactionNo,
                Amount = amount,
                BankCode = bankCode,
                CardType = cardType,
                OrderInfo = orderInfo,
                PayDate = payDate,
                OrderId = orderId,
                TransactionId = transaction?.Id,
                PaymentGateway = "SEPAY",
                Message = !isValidSignature ? "Invalid signature" : (isSuccess ? "Payment successful" : GetResponseMessage(responseCode))
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
