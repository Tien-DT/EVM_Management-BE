using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.Order;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Order;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<Order> CreateOrderAsync(OrderCreateDto dto)
        {
            var order = new Order
            {
                Code = dto.Code,
                QuotationId = dto.QuotationId,
                CustomerId = dto.CustomerId,
                DealerId = dto.DealerId,
                CreatedByUserId = dto.CreatedByUserId,
                Status = dto.Status,
                TotalAmount = dto.TotalAmount,
                DiscountAmount = dto.DiscountAmount,
                FinalAmount = dto.FinalAmount,
                ExpectedDeliveryAt = DateTimeHelper.ToUtc(dto.ExpectedDeliveryAt),
                OrderType = dto.OrderType,
                IsFinanced = dto.IsFinanced
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return order;
        }

        public async Task<OrderWithDetailsResponse> CreateOrderWithDetailsAsync(OrderWithDetailsCreateDto dto)
        {
            var order = _mapper.Map<Order>(dto);
            order.ExpectedDeliveryAt = DateTimeHelper.ToUtc(dto.ExpectedDeliveryAt);

            var orderDetails = _mapper.Map<List<OrderDetail>>(dto.OrderDetails);
            foreach (var detail in orderDetails)
            {
                detail.OrderId = order.Id;
            }

            var baseAmount = orderDetails.Sum(d => d.UnitPrice * d.Quantity);

            var totalAmount = dto.TotalAmount ?? baseAmount;
            if (totalAmount < 0)
            {
                totalAmount = 0;
            }

            var discountAmount = dto.DiscountAmount ?? 0;
            if (discountAmount < 0)
            {
                discountAmount = 0;
            }

            decimal finalAmount;
            if (dto.FinalAmount.HasValue)
            {
                finalAmount = dto.FinalAmount.Value;
                if (finalAmount < 0)
                {
                    finalAmount = 0;
                }

                if (!dto.DiscountAmount.HasValue)
                {
                    discountAmount = totalAmount - finalAmount;
                }
            }
            else
            {
                finalAmount = totalAmount - discountAmount;
            }

            if (discountAmount > totalAmount)
            {
                discountAmount = totalAmount;
            }

            if (finalAmount < 0)
            {
                finalAmount = 0;
            }

            order.TotalAmount = totalAmount;
            order.DiscountAmount = discountAmount;
            order.FinalAmount = finalAmount;

            var vehicleIds = orderDetails
                .Where(d => d.VehicleId.HasValue)
                .Select(d => d.VehicleId.Value)
                .Distinct()
                .ToList();

            List<Vehicle>? vehicles = null;
            if (vehicleIds.Any())
            {
                vehicles = await _unitOfWork.Vehicles.GetQueryable()
                    .Where(v => vehicleIds.Contains(v.Id))
                    .ToListAsync();
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Orders.AddAsync(order);
                if (orderDetails.Count > 0)
                {
                    await _unitOfWork.OrderDetails.AddRangeAsync(orderDetails);
                }

                if (vehicles != null && vehicles.Any())
                {
                    foreach (var vehicle in vehicles)
                    {
                        vehicle.Status = VehicleStatus.RESERVED;
                    }

                    _unitOfWork.Vehicles.UpdateRange(vehicles);
                }

                if (order.IsFinanced && dto.InstallmentDuration.HasValue)
                {
                    var installmentPlan = new InstallmentPlan
                    {
                        OrderId = order.Id,
                        Provider = dto.InstallmentProvider ?? "Default Provider",
                        PrincipalAmount = order.FinalAmount ?? 0,
                        InterestRate = dto.InterestRate ?? 0,
                        NumberOfInstallments = dto.InstallmentDuration.Value,
                        Status = InstallmentPlanStatus.ACTIVE,
                        StartDate = DateTime.UtcNow
                    };

                    await _unitOfWork.InstallmentPlans.AddAsync(installmentPlan);
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return await _unitOfWork.Orders.GetQueryable()
                .Where(o => o.Id == order.Id)
                .ProjectTo<OrderWithDetailsResponse>(_mapper.ConfigurationProvider)
                .FirstAsync();
        }

        public async Task<PagedResult<OrderResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var totalCount = await _unitOfWork.Orders.CountAsync();

            var orders = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.Quotation)
                .Include(o => o.Customer)
                .Include(o => o.Dealer)
                .Include(o => o.CreatedByUser)
                .Include(o => o.Deposits)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.VehicleVariant)
                        .ThenInclude(vv => vv.VehicleModel)
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = _mapper.Map<List<OrderResponse>>(orders);

            return PagedResult<OrderResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<OrderResponse>> GetByFilterAsync(OrderFilterDto filter)
        {
            IQueryable<Order> query = _unitOfWork.Orders.GetQueryable()
                .Include(o => o.Quotation)
                .Include(o => o.Customer)
                .Include(o => o.Dealer)
                .Include(o => o.CreatedByUser)
                .Include(o => o.Deposits);

            if (!string.IsNullOrWhiteSpace(filter.Code))
            {
                var code = filter.Code.ToLower();
                query = query.Where(o => o.Code.ToLower().Contains(code));
            }

            if (filter.QuotationId.HasValue)
            {
                query = query.Where(o => o.QuotationId == filter.QuotationId.Value);
            }

            if (filter.CustomerId.HasValue)
            {
                query = query.Where(o => o.CustomerId == filter.CustomerId.Value);
            }

            if (filter.DealerId.HasValue)
            {
                query = query.Where(o => o.DealerId == filter.DealerId.Value);
            }

            if (filter.CreatedByUserId.HasValue)
            {
                query = query.Where(o => o.CreatedByUserId == filter.CreatedByUserId.Value);
            }

            if (filter.OrderType.HasValue)
            {
                query = query.Where(o => o.OrderType == filter.OrderType.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(o => o.Status == filter.Status.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ProjectTo<OrderResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<OrderResponse>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<OrderResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Orders.GetByIdAsync(id);
            if (entity == null) return null;

            return _mapper.Map<OrderResponse>(entity);
        }

        public async Task<OrderWithDetailsResponse?> GetByIdWithDetailsAsync(Guid id)
        {
            var order = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.Quotation)
                .Include(o => o.Customer)
                .Include(o => o.Dealer)
                .Include(o => o.CreatedByUser)
                .Include(o => o.Deposits)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Vehicle)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.VehicleVariant)
                        .ThenInclude(vv => vv.VehicleModel)
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();

            if (order == null) return null;

            return _mapper.Map<OrderWithDetailsResponse>(order);
        }

        public async Task<OrderResponse?> UpdateAsync(Guid id, OrderUpdateDto dto)
        {
            var entity = await _unitOfWork.Orders.GetByIdAsync(id);
            if (entity == null) return null;

            // Track old status to check if order is being canceled
            var oldStatus = entity.Status;

            if (dto.Code != null) entity.Code = dto.Code;
            if (dto.QuotationId.HasValue) entity.QuotationId = dto.QuotationId;
            if (dto.CustomerId.HasValue) entity.CustomerId = dto.CustomerId;
            if (dto.DealerId.HasValue) entity.DealerId = dto.DealerId;
            if (dto.Status.HasValue) entity.Status = dto.Status.Value;
            if (dto.TotalAmount.HasValue) entity.TotalAmount = dto.TotalAmount;
            if (dto.DiscountAmount.HasValue) entity.DiscountAmount = dto.DiscountAmount;
            if (dto.FinalAmount.HasValue) entity.FinalAmount = dto.FinalAmount;
            if (dto.ExpectedDeliveryAt.HasValue) entity.ExpectedDeliveryAt = DateTimeHelper.ToUtc(dto.ExpectedDeliveryAt);
            if (dto.OrderType.HasValue) entity.OrderType = dto.OrderType.Value;
            if (dto.IsFinanced.HasValue) entity.IsFinanced = dto.IsFinanced.Value;

            // If status is changing from QUOTATION_RECEIVED to AWAITING_DEPOSIT, add quotation total to order
            if (oldStatus == OrderStatus.QUOTATION_RECEIVED &&
                dto.Status.HasValue &&
                dto.Status.Value == OrderStatus.AWAITING_DEPOSIT &&
                entity.QuotationId.HasValue)
            {
                var quotation = await _unitOfWork.Quotations.GetByIdAsync(entity.QuotationId.Value);
                if (quotation != null && quotation.Total.HasValue)
                {
                    // Add quotation total to order amounts
                    entity.TotalAmount = (entity.TotalAmount ?? 0) + quotation.Total.Value;
                    entity.FinalAmount = (entity.FinalAmount ?? 0) + quotation.Total.Value;
                }
            }

            entity.ModifiedDate = DateTime.UtcNow;

            // If order is being canceled, return vehicles to warehouse (IN_STOCK status)
            if (oldStatus != OrderStatus.CANCELED && entity.Status == OrderStatus.CANCELED)
            {
                // Get all vehicles from this order's order details
                var orderDetails = await _unitOfWork.OrderDetails.GetQueryable()
                    .Where(od => od.OrderId == id && od.VehicleId.HasValue)
                    .ToListAsync();

                if (orderDetails.Any())
                {
                    var vehicleIds = orderDetails
                        .Select(od => od.VehicleId.Value)
                        .Distinct()
                        .ToList();

                    var vehicles = await _unitOfWork.Vehicles.GetQueryable()
                        .Where(v => vehicleIds.Contains(v.Id))
                        .ToListAsync();

                    if (vehicles.Any())
                    {
                        foreach (var vehicle in vehicles)
                        {
                            // Return vehicle to warehouse - set status back to IN_STOCK
                            vehicle.Status = VehicleStatus.IN_STOCK;
                        }

                        _unitOfWork.Vehicles.UpdateRange(vehicles);
                    }
                }
            }

            _unitOfWork.Orders.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<OrderResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.Orders.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Orders.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Orders.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.Orders.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<OrderResponse> CancelOrderAsync(Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Vehicle)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đơn hàng với mã {orderId}");
            }

            if (order.Status == OrderStatus.CANCELED)
            {
                return _mapper.Map<OrderResponse>(order);
            }

            var vehicles = order.OrderDetails
                .Where(od => od.Vehicle != null && od.Vehicle.Status == VehicleStatus.SOLD)
                .Select(od => od.Vehicle!)
                .GroupBy(v => v.Id)
                .Select(g => g.First())
                .ToList();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (vehicles.Count > 0)
                {
                    foreach (var vehicle in vehicles)
                    {
                        vehicle.Status = VehicleStatus.IN_STOCK;
                        vehicle.ModifiedDate = DateTime.UtcNow;
                    }

                    _unitOfWork.Vehicles.UpdateRange(vehicles);
                }

                order.Status = OrderStatus.CANCELED;
                order.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return await _unitOfWork.Orders.GetQueryable()
                .Where(o => o.Id == orderId)
                .ProjectTo<OrderResponse>(_mapper.ConfigurationProvider)
                .FirstAsync();
        }

        public async Task<OrderResponse> CompleteOrderAsync(Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
            }

            if (order.Status == OrderStatus.COMPLETED)
            {
                return await _unitOfWork.Orders.GetQueryable()
                    .Where(o => o.Id == orderId)
                    .ProjectTo<OrderResponse>(_mapper.ConfigurationProvider)
                    .FirstAsync();
            }

            if (!order.DealerId.HasValue)
            {
                throw new InvalidOperationException("Đơn hàng chưa được gán cho đại lý nên không thể hoàn tất.");
            }

            var vehicleIds = order.OrderDetails
                .Where(od => od.VehicleId.HasValue)
                .Select(od => od.VehicleId!.Value)
                .Distinct()
                .ToList();

            if (!vehicleIds.Any())
            {
                throw new InvalidOperationException("Đơn hàng chưa có xe để hoàn tất.");
            }

            var vehicles = await _unitOfWork.Vehicles.GetQueryable()
                .Where(v => vehicleIds.Contains(v.Id) && !v.IsDeleted)
                .ToListAsync();

            if (vehicles.Count != vehicleIds.Count)
            {
                throw new InvalidOperationException("Không thể xác nhận thông tin xe cho đơn hàng.");
            }

            var warehouse = await _unitOfWork.Warehouses.GetQueryable()
                .Where(w => w.DealerId == order.DealerId && !w.IsDeleted)
                .OrderBy(w => w.CreatedDate)
                .FirstOrDefaultAsync();

            if (warehouse == null)
            {
                throw new InvalidOperationException("Dealer chưa có kho để nhận xe. Vui lòng tạo kho trước.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var vehicle in vehicles)
                {
                    vehicle.WarehouseId = warehouse.Id;
                    vehicle.Status = VehicleStatus.IN_STOCK;
                    vehicle.ModifiedDate = DateTime.UtcNow;
                }

                _unitOfWork.Vehicles.UpdateRange(vehicles);

                order.Status = OrderStatus.COMPLETED;
                order.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception("Hoàn tất đơn hàng thất bại, vui lòng thử lại sau.", ex);
            }

            return await _unitOfWork.Orders.GetQueryable()
                .Where(o => o.Id == orderId)
                .ProjectTo<OrderResponse>(_mapper.ConfigurationProvider)
                .FirstAsync();
        }

        public async Task<OrderFlowResponseDto> RequestDealerManagerApprovalAsync(Guid orderId, DealerManagerApprovalRequestDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                return new OrderFlowResponseDto
                {
                    OrderId = orderId,
                    Success = false,
                    Message = "Order not found"
                };
            }

            order.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);

            var report = new Report
            {
                Type = "PENDING_DEALER_MANAGER_APPROVAL",
                Title = "Awaiting Dealer Manager approval",
                Content = $"Order {order.Code} requires approval from Dealer Manager. Requested by user {dto.RequestedByUserId}. Note: {dto.Note}",
                OrderId = order.Id,
                DealerId = order.DealerId,
                AccountId = dto.RequestedByUserId
            };
            await _unitOfWork.Reports.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();

            if (order.DealerId.HasValue)
            {
                var dealer = await _unitOfWork.Dealers.GetByIdAsync(order.DealerId.Value);
                if (dealer != null && !string.IsNullOrEmpty(dealer.Email))
                {
                    var subject = $"Order Approval Required - {order.Code}";
                    var body = $"<h3>Order Approval Request</h3>" +
                              $"<p>Order <strong>{order.Code}</strong> requires your approval.</p>" +
                              $"<p>Amount: {order.FinalAmount:N2} VND</p>" +
                              $"<p>Note: {dto.Note}</p>";
                    
                    await _emailService.SendEmailAsync(dealer.Email, subject, body, true);
                }
            }

            return new OrderFlowResponseDto
            {
                OrderId = order.Id,
                Status = order.Status,
                Success = true,
                Message = "Approval request sent to Dealer Manager"
            };
        }

        public async Task<OrderFlowResponseDto> ApproveDealerOrderRequestAsync(Guid orderId, Guid approvedByUserId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                return new OrderFlowResponseDto
                {
                    OrderId = orderId,
                    Success = false,
                    Message = "Order not found"
                };
            }

            order.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);

            var previousReport = await _unitOfWork.Reports.GetQueryable()
                .Where(r => r.OrderId == orderId && r.Type == "PENDING_DEALER_MANAGER_APPROVAL")
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            var report = new Report
            {
                Type = "AWAITING_EVM_FULFILLMENT",
                Title = "Awaiting EVM fulfillment",
                Content = $"Order {order.Code} approved by Dealer Manager. Waiting for EVM to fulfill vehicle order.",
                OrderId = order.Id,
                DealerId = order.DealerId,
                AccountId = approvedByUserId
            };
            await _unitOfWork.Reports.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();


            return new OrderFlowResponseDto
            {
                OrderId = order.Id,
                Status = order.Status,
                Success = true,
                Message = "Order approved, request sent to EVM"
            };
        }

        public async Task<bool> NotifyCustomerAsync(Guid orderId, CustomerNotificationRequestDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null || !order.CustomerId.HasValue)
            {
                return false;
            }

            var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId.Value);
            if (customer == null || string.IsNullOrEmpty(customer.Email))
            {
                return false;
            }

            // gửi mail đến customer
            var subject = dto.EmailSubject ?? $"Update on your order - {order.Code}";
            var body = $"<h3>Dear {customer.FullName ?? "Valued Customer"},</h3>" +
                      $"<p>{dto.Message}</p>" +
                      $"<p>Order Code: <strong>{order.Code}</strong></p>" +
                      $"<p>Thank you for your business!</p>";

            await _emailService.SendEmailAsync(customer.Email, subject, body, true);

            var report = new Report
            {
                Type = "CUSTOMER_NOTIFICATION",
                Title = subject,
                Content = dto.Message,
                OrderId = order.Id,
                DealerId = order.DealerId,
                AccountId = order.CreatedByUserId
            };
            await _unitOfWork.Reports.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<OrderResponse> UpdateCustomerConfirmationAsync(Guid orderId, CustomerConfirmationRequestDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception($"Order with ID {orderId} not found");
            }

            if (dto.IsConfirmed)
            {
                // khách xác nhận - cập nhật trạng thái CONFIRMED
                order.Status = OrderStatus.CONFIRMED;
                order.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.Orders.Update(order);

                var confirmReport = new Report
                {
                    Type = "CUSTOMER_CONFIRMED",
                    Title = "Customer confirmed order",
                    Content = $"Customer confirmed order {order.Code}. Note: {dto.CustomerNote}",
                    OrderId = order.Id,
                    DealerId = order.DealerId,
                    AccountId = order.CreatedByUserId
                };
                await _unitOfWork.Reports.AddAsync(confirmReport);
            }
            else
            {
                // khách từ chối - hủy đơn 
                order.Status = OrderStatus.CANCELED;
                order.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.Orders.Update(order);

                var cancelReport = new Report
                {
                    Type = "CUSTOMER_REJECTED",
                    Title = "Customer rejected order",
                    Content = $"Customer rejected order {order.Code}. Reason: {dto.CustomerNote}",
                    OrderId = order.Id,
                    DealerId = order.DealerId,
                    AccountId = order.CreatedByUserId
                };
                await _unitOfWork.Reports.AddAsync(cancelReport);
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(orderId) ?? throw new Exception("Failed to retrieve updated order");
        }

        public async Task<OrderFlowResponseDto> ConfirmPaymentAsync(Guid orderId, ConfirmPaymentRequestDto dto)
        {
            var order = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.Deposits)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return new OrderFlowResponseDto
                {
                    OrderId = orderId,
                    Success = false,
                    Message = "Order not found"
                };
            }

            // tính số tiền còn lại
            var totalOrderAmount = order.FinalAmount ?? 0;
            var totalDeposits = order.Deposits
                .Where(d => !d.IsDeleted && d.Status == DepositStatus.PAID)
                .Sum(d => d.Amount);
            var remainingAmount = totalOrderAmount - totalDeposits;

            if (remainingAmount < 0)
            {
                return new OrderFlowResponseDto
                {
                    OrderId = orderId,
                    Success = false,
                    Message = "Invalid payment amount calculation"
                };
            }

            // tạo mã hóa đơn
            var invoiceCode = $"INV-{order.Code}-{DateTime.UtcNow:yyyyMMddHHmmss}";

            var invoice = new Invoice
            {
                OrderId = orderId,
                InvoiceCode = invoiceCode,
                TotalAmount = remainingAmount,
                Status = InvoiceStatus.PAID
            };

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync(); 

            // tạo giao dịch
            var transaction = new Transaction
            {
                InvoiceId = invoice.Id,
                Amount = remainingAmount,
                Currency = "VND",
                Status = TransactionStatus.SUCCESS,
                TransactionTime = DateTime.UtcNow,
                PaymentGateway = dto.Method.ToString(),
                TransactionInfo = dto.Note ?? $"Payment for order {order.Code}",
                ResponseCode = "00" // Success code
            };

            if (!string.IsNullOrWhiteSpace(dto.TransactionReference))
            {
                transaction.VnpayTransactionCode = dto.TransactionReference;
            }

            await _unitOfWork.Transactions.AddAsync(transaction);

            // cập nhật trạng thái đơn
            order.Status = OrderStatus.READY_FOR_HANDOVER;
            order.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);

            // log ghi thanh toán
            var report = new Report
            {
                Type = "PAYMENT_CONFIRMED",
                Title = "Payment confirmed",
                Content = $"Payment confirmed for order {order.Code}. Amount: {remainingAmount:N2} VND. Method: {dto.Method}",
                OrderId = order.Id,
                DealerId = order.DealerId,
                AccountId = order.CreatedByUserId
            };
            await _unitOfWork.Reports.AddAsync(report);

            await _unitOfWork.SaveChangesAsync();

            return new OrderFlowResponseDto
            {
                OrderId = order.Id,
                Status = order.Status,
                Success = true,
                Message = $"Payment confirmed. Amount paid: {remainingAmount:N2} VND",
                Data = new
                {
                    InvoiceCode = invoiceCode,
                    AmountPaid = remainingAmount,
                    TotalDeposits = totalDeposits,
                    TotalOrderAmount = totalOrderAmount
                }
            };
        }


        public async Task<string?> GetOrderFlowStageAsync(Guid orderId)
        {
            var latestReport = await _unitOfWork.Reports.GetQueryable()
                .Where(r => r.OrderId == orderId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            return latestReport?.Type;
        }


        public async Task CreateOrderFlowReportAsync(Guid orderId, string type, string title, string content, Guid? accountId = null)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception($"Order with ID {orderId} not found");
            }

            var report = new Report
            {
                Type = type,
                Title = title,
                Content = content,
                OrderId = orderId,
                DealerId = order.DealerId,
                AccountId = accountId ?? order.CreatedByUserId
            };

            await _unitOfWork.Reports.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();
        }

        public IQueryable<Order> GetQueryableForOData()
        {
            return _unitOfWork.Orders.GetQueryable()
                .Include(o => o.Quotation)
                .Include(o => o.Customer)
                .Include(o => o.Dealer)
                .Include(o => o.CreatedByUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.VehicleVariant)
                        .ThenInclude(vv => vv!.VehicleModel)
                .Include(o => o.Contract)
                .Include(o => o.Deposits)
                .Include(o => o.HandoverRecord)
                .Where(o => !o.IsDeleted);
        }
    }
}
