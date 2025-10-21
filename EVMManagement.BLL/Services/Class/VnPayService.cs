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
    public class VnPayService : IVnPayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly VnPaySettings _vnPaySettings;

        public VnPayService(IUnitOfWork unitOfWork, IOptions<VnPaySettings> vnPaySettings)
        {
            _unitOfWork = unitOfWork;
            _vnPaySettings = vnPaySettings.Value;
        }

        public async Task<VnPayPaymentResponse> CreatePaymentUrlAsync(VnPayPaymentRequest request, string ipAddress)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                throw new Exception("Order not found");
            }

            var createDate = DateTime.UtcNow;
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var createDateVn = TimeZoneInfo.ConvertTimeFromUtc(createDate, vnTimeZone);
            var expireDateVn = createDateVn.AddMinutes(15);
            var transactionCode = $"ORD{request.OrderId.ToString("N")[..8]}{createDateVn:yyyyMMddHHmmss}";

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", _vnPaySettings.Version);
            vnpay.AddRequestData("vnp_Command", _vnPaySettings.Command);
            vnpay.AddRequestData("vnp_TmnCode", _vnPaySettings.TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", createDateVn.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _vnPaySettings.CurrCode);
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", request.Locale ?? _vnPaySettings.Locale);
            vnpay.AddRequestData("vnp_OrderInfo", request.OrderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _vnPaySettings.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", transactionCode);
            vnpay.AddRequestData("vnp_ExpireDate", expireDateVn.ToString("yyyyMMddHHmmss"));

            if (!string.IsNullOrEmpty(request.BankCode))
            {
                vnpay.AddRequestData("vnp_BankCode", request.BankCode);
            }

            var paymentUrl = vnpay.CreateRequestUrl(_vnPaySettings.PaymentUrl, _vnPaySettings.HashSecret);

            var transaction = new Transaction
            {
                Amount = request.Amount,
                Currency = "VND",
                Status = TransactionStatus.PENDING,
                TransactionTime = createDate,
                PaymentGateway = "VNPAY",
                VnpayTransactionCode = transactionCode,
                TransactionInfo = request.OrderInfo
            };

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

            return new VnPayPaymentResponse
            {
                PaymentUrl = paymentUrl,
                TransactionCode = transactionCode,
                OrderId = request.OrderId,
                Amount = request.Amount,
                OrderInfo = request.OrderInfo,
                CreatedDate = createDate
            };
        }

        public async Task<VnPayCallbackResponse> ProcessCallbackAsync(Dictionary<string, string> vnpayData)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in vnpayData)
            {
                if (!string.IsNullOrEmpty(value) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            var vnpayTxnRef = vnpay.GetResponseData("vnp_TxnRef");
            var vnpayTransactionNo = vnpay.GetResponseData("vnp_TransactionNo");
            var vnpayResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnpaySecureHash = vnpayData.ContainsKey("vnp_SecureHash") ? vnpayData["vnp_SecureHash"] : "";
            var vnpayAmount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
            var vnpayBankCode = vnpay.GetResponseData("vnp_BankCode");
            var vnpayCardType = vnpay.GetResponseData("vnp_CardType");
            var vnpayOrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            var vnpayPayDate = vnpay.GetResponseData("vnp_PayDate");

            var checkSignature = vnpay.ValidateSignature(vnpaySecureHash, _vnPaySettings.HashSecret);
            if (!checkSignature)
            {
                return new VnPayCallbackResponse
                {
                    Success = false,
                    Message = "Invalid signature",
                    ResponseCode = "97"
                };
            }

            var transaction = _unitOfWork.Transactions.GetQueryable()
                .FirstOrDefault(t => t.VnpayTransactionCode == vnpayTxnRef);

            if (transaction == null)
            {
                return new VnPayCallbackResponse
                {
                    Success = false,
                    Message = "Transaction not found",
                    ResponseCode = "01",
                    TransactionCode = vnpayTxnRef
                };
            }

            transaction.VnpayTransactionNo = vnpayTransactionNo;
            transaction.BankCode = vnpayBankCode;
            transaction.CardType = vnpayCardType;
            transaction.ResponseCode = vnpayResponseCode;
            transaction.SecureHash = vnpaySecureHash;
            transaction.ModifiedDate = DateTime.UtcNow;

            DateTime payDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(vnpayPayDate) && vnpayPayDate.Length >= 14)
            {
                DateTime.TryParseExact(vnpayPayDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out payDate);
            }

            if (vnpayResponseCode == "00")
            {
                transaction.Status = TransactionStatus.SUCCESS;
                transaction.TransactionTime = payDate;

                if (transaction.DepositId.HasValue)
                {
                    var deposit = await _unitOfWork.Deposits.GetByIdAsync(transaction.DepositId.Value);
                    if (deposit != null)
                    {
                        deposit.Status = DepositStatus.PAID;
                        deposit.ModifiedDate = DateTime.UtcNow;
                        _unitOfWork.Deposits.Update(deposit);
                    }
                }

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
                            order.Status = OrderStatus.CONFIRMED;
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

            return new VnPayCallbackResponse
            {
                Success = vnpayResponseCode == "00",
                ResponseCode = vnpayResponseCode,
                TransactionCode = vnpayTxnRef,
                VnpayTransactionNo = vnpayTransactionNo,
                Amount = vnpayAmount,
                BankCode = vnpayBankCode,
                CardType = vnpayCardType,
                OrderInfo = vnpayOrderInfo,
                PayDate = payDate,
                OrderId = orderId,
                TransactionId = transaction.Id,
                Message = vnpayResponseCode == "00" ? "Payment successful" : GetResponseMessage(vnpayResponseCode)
            };
        }

        public VnPayCallbackResponse ProcessReturnUrl(Dictionary<string, string> vnpayData)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in vnpayData)
            {
                if (!string.IsNullOrEmpty(value) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            var vnpayTxnRef = vnpay.GetResponseData("vnp_TxnRef");
            var vnpayTransactionNo = vnpay.GetResponseData("vnp_TransactionNo");
            var vnpayResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnpaySecureHash = vnpayData.ContainsKey("vnp_SecureHash") ? vnpayData["vnp_SecureHash"] : "";
            var vnpayAmount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
            var vnpayBankCode = vnpay.GetResponseData("vnp_BankCode");
            var vnpayCardType = vnpay.GetResponseData("vnp_CardType");
            var vnpayOrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            var vnpayPayDate = vnpay.GetResponseData("vnp_PayDate");

            var checkSignature = vnpay.ValidateSignature(vnpaySecureHash, _vnPaySettings.HashSecret);

            DateTime payDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(vnpayPayDate) && vnpayPayDate.Length >= 14)
            {
                DateTime.TryParseExact(vnpayPayDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out payDate);
            }

            var transaction = _unitOfWork.Transactions.GetQueryable()
                .FirstOrDefault(t => t.VnpayTransactionCode == vnpayTxnRef);

            Guid? orderId = null;
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

            return new VnPayCallbackResponse
            {
                Success = checkSignature && vnpayResponseCode == "00",
                ResponseCode = vnpayResponseCode,
                TransactionCode = vnpayTxnRef,
                VnpayTransactionNo = vnpayTransactionNo,
                Amount = vnpayAmount,
                BankCode = vnpayBankCode,
                CardType = vnpayCardType,
                OrderInfo = vnpayOrderInfo,
                PayDate = payDate,
                OrderId = orderId,
                TransactionId = transaction?.Id,
                Message = !checkSignature ? "Invalid signature" : (vnpayResponseCode == "00" ? "Payment successful" : GetResponseMessage(vnpayResponseCode))
            };
        }

        private string GetResponseMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)",
                "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng",
                "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán",
                "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa",
                "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP)",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày",
                "75" => "Ngân hàng thanh toán đang bảo trì",
                "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định",
                "99" => "Các lỗi khác",
                _ => "Lỗi không xác định"
            };
        }
    }
}
