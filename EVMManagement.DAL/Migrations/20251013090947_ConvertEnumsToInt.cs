using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ConvertEnumsToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert AccountRole enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Accounts"" ALTER COLUMN ""Role"" TYPE integer 
                USING CASE ""Role""
                    WHEN 'EVM_ADMIN' THEN 0
                    WHEN 'EVM_STAFF' THEN 1
                    WHEN 'DEALER_ADMIN' THEN 2
                    WHEN 'DEALER_STAFF' THEN 3
                    ELSE 0
                END;");

            // Convert WarehouseType enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Warehouses"" ALTER COLUMN ""Type"" TYPE integer 
                USING CASE ""Type""
                    WHEN 'DEALER' THEN 0
                    WHEN 'EVM' THEN 1
                    ELSE 0
                END;");

            // Convert VehicleStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Vehicles"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'IN_STOCK' THEN 0
                    WHEN 'RESERVED' THEN 1
                    WHEN 'SOLD' THEN 2
                    WHEN 'IN_TRANSIT' THEN 3
                    WHEN 'DELIVERED' THEN 4
                    ELSE 0
                END;");

            // Convert VehiclePurpose enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Vehicles"" ALTER COLUMN ""Purpose"" TYPE integer 
                USING CASE ""Purpose""
                    WHEN 'FOR_SALE' THEN 0
                    WHEN 'TEST_DRIVE' THEN 1
                    ELSE 0
                END;");

            // Convert TransportStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Transports"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'PENDING' THEN 0
                    WHEN 'IN_TRANSIT' THEN 1
                    WHEN 'DELIVERED' THEN 2
                    WHEN 'CANCELLED' THEN 3
                    ELSE 0
                END;");

            // Convert TransactionStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Transactions"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'PENDING' THEN 0
                    WHEN 'SUCCESS' THEN 1
                    WHEN 'FAILED' THEN 2
                    WHEN 'CANCELLED' THEN 3
                    ELSE 0
                END;");

            // Convert TimeSlotStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""VehicleTimeSlots"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'AVAILABLE' THEN 0
                    WHEN 'BOOKED' THEN 1
                    WHEN 'UNAVAILABLE' THEN 2
                    ELSE 0
                END;");

            // Convert TestDriveBookingStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""TestDriveBookings"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'BOOKED' THEN 0
                    WHEN 'CHECKED_IN' THEN 1
                    WHEN 'COMPLETED' THEN 2
                    WHEN 'CANCELLED' THEN 3
                    WHEN 'NO_SHOW' THEN 4
                    ELSE 0
                END;");

            // Convert QuotationStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Quotations"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'DRAFT' THEN 0
                    WHEN 'SENT' THEN 1
                    WHEN 'VIEWED' THEN 2
                    WHEN 'ACCEPTED' THEN 3
                    WHEN 'REJECTED' THEN 4
                    WHEN 'EXPIRED' THEN 5
                    ELSE 0
                END;");

            // Convert OrderStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'CONFIRMED' THEN 0
                    WHEN 'AWAITING_DEPOSIT' THEN 1
                    WHEN 'DEPOSIT_PAID' THEN 2
                    WHEN 'PROCESSING' THEN 3
                    WHEN 'READY_FOR_DELIVERY' THEN 4
                    WHEN 'IN_TRANSIT' THEN 5
                    WHEN 'DELIVERED' THEN 6
                    WHEN 'COMPLETED' THEN 7
                    WHEN 'CANCELLED' THEN 8
                    ELSE 0
                END;");

            // Convert OrderType enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders"" ALTER COLUMN ""OrderType"" TYPE integer 
                USING CASE ""OrderType""
                    WHEN 'B2C' THEN 0
                    WHEN 'B2B' THEN 1
                    ELSE 0
                END;");

            // Convert InvoiceStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Invoices"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'DRAFT' THEN 0
                    WHEN 'ISSUED' THEN 1
                    WHEN 'PARTIALLY_PAID' THEN 2
                    WHEN 'PAID' THEN 3
                    WHEN 'OVERDUE' THEN 4
                    WHEN 'CANCELLED' THEN 5
                    ELSE 0
                END;");

            // Convert InstallmentPlanStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""InstallmentPlans"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'ACTIVE' THEN 0
                    WHEN 'PAID_OFF' THEN 1
                    WHEN 'DEFAULTED' THEN 2
                    WHEN 'CANCELLED' THEN 3
                    ELSE 0
                END;");

            // Convert InstallmentPaymentStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""InstallmentPayments"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'PENDING' THEN 0
                    WHEN 'PAID' THEN 1
                    WHEN 'OVERDUE' THEN 2
                    WHEN 'CANCELLED' THEN 3
                    ELSE 0
                END;");

            // Convert DepositStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Deposits"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'PENDING' THEN 0
                    WHEN 'PAID' THEN 1
                    WHEN 'REFUNDED' THEN 2
                    WHEN 'CANCELLED' THEN 3
                    ELSE 0
                END;");

            // Convert PaymentMethod enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Deposits"" ALTER COLUMN ""Method"" TYPE integer 
                USING CASE ""Method""
                    WHEN 'CASH' THEN 0
                    WHEN 'BANK_TRANSFER' THEN 1
                    WHEN 'CREDIT_CARD' THEN 2
                    WHEN 'ONLINE_PAYMENT' THEN 3
                    ELSE 0
                END;");

            // Convert DealerContractStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""DealerContracts"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'DRAFT' THEN 0
                    WHEN 'PENDING_SIGNATURE' THEN 1
                    WHEN 'SIGNED' THEN 2
                    WHEN 'EXPIRED' THEN 3
                    WHEN 'TERMINATED' THEN 4
                    ELSE 0
                END;");

            // Convert ContractStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Contracts"" ALTER COLUMN ""Status"" TYPE integer 
                USING CASE ""Status""
                    WHEN 'DRAFT' THEN 0
                    WHEN 'PENDING_SIGNATURE' THEN 1
                    WHEN 'SIGNED' THEN 2
                    WHEN 'CANCELLED' THEN 3
                    WHEN 'COMPLETED' THEN 4
                    ELSE 0
                END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert AccountRole enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Accounts"" ALTER COLUMN ""Role"" TYPE text 
                USING CASE ""Role""
                    WHEN 0 THEN 'EVM_ADMIN'
                    WHEN 1 THEN 'EVM_STAFF'
                    WHEN 2 THEN 'DEALER_ADMIN'
                    WHEN 3 THEN 'DEALER_STAFF'
                    ELSE 'EVM_STAFF'
                END;");

            // Revert WarehouseType enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Warehouses"" ALTER COLUMN ""Type"" TYPE text 
                USING CASE ""Type""
                    WHEN 0 THEN 'DEALER'
                    WHEN 1 THEN 'EVM'
                    ELSE 'DEALER'
                END;");

            // Revert VehicleStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Vehicles"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'IN_STOCK'
                    WHEN 1 THEN 'RESERVED'
                    WHEN 2 THEN 'SOLD'
                    WHEN 3 THEN 'IN_TRANSIT'
                    WHEN 4 THEN 'DELIVERED'
                    ELSE 'IN_STOCK'
                END;");

            // Revert VehiclePurpose enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Vehicles"" ALTER COLUMN ""Purpose"" TYPE text 
                USING CASE ""Purpose""
                    WHEN 0 THEN 'FOR_SALE'
                    WHEN 1 THEN 'TEST_DRIVE'
                    ELSE 'FOR_SALE'
                END;");

            // Revert TransportStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Transports"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'PENDING'
                    WHEN 1 THEN 'IN_TRANSIT'
                    WHEN 2 THEN 'DELIVERED'
                    WHEN 3 THEN 'CANCELLED'
                    ELSE 'PENDING'
                END;");

            // Revert TransactionStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Transactions"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'PENDING'
                    WHEN 1 THEN 'SUCCESS'
                    WHEN 2 THEN 'FAILED'
                    WHEN 3 THEN 'CANCELLED'
                    ELSE 'PENDING'
                END;");

            // Revert TimeSlotStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""VehicleTimeSlots"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'AVAILABLE'
                    WHEN 1 THEN 'BOOKED'
                    WHEN 2 THEN 'UNAVAILABLE'
                    ELSE 'AVAILABLE'
                END;");

            // Revert TestDriveBookingStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""TestDriveBookings"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'BOOKED'
                    WHEN 1 THEN 'CHECKED_IN'
                    WHEN 2 THEN 'COMPLETED'
                    WHEN 3 THEN 'CANCELLED'
                    WHEN 4 THEN 'NO_SHOW'
                    ELSE 'BOOKED'
                END;");

            // Revert QuotationStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Quotations"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'DRAFT'
                    WHEN 1 THEN 'SENT'
                    WHEN 2 THEN 'VIEWED'
                    WHEN 3 THEN 'ACCEPTED'
                    WHEN 4 THEN 'REJECTED'
                    WHEN 5 THEN 'EXPIRED'
                    ELSE 'DRAFT'
                END;");

            // Revert OrderStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'CONFIRMED'
                    WHEN 1 THEN 'AWAITING_DEPOSIT'
                    WHEN 2 THEN 'DEPOSIT_PAID'
                    WHEN 3 THEN 'PROCESSING'
                    WHEN 4 THEN 'READY_FOR_DELIVERY'
                    WHEN 5 THEN 'IN_TRANSIT'
                    WHEN 6 THEN 'DELIVERED'
                    WHEN 7 THEN 'COMPLETED'
                    WHEN 8 THEN 'CANCELLED'
                    ELSE 'CONFIRMED'
                END;");

            // Revert OrderType enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders"" ALTER COLUMN ""OrderType"" TYPE text 
                USING CASE ""OrderType""
                    WHEN 0 THEN 'B2C'
                    WHEN 1 THEN 'B2B'
                    ELSE 'B2C'
                END;");

            // Revert InvoiceStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Invoices"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'DRAFT'
                    WHEN 1 THEN 'ISSUED'
                    WHEN 2 THEN 'PARTIALLY_PAID'
                    WHEN 3 THEN 'PAID'
                    WHEN 4 THEN 'OVERDUE'
                    WHEN 5 THEN 'CANCELLED'
                    ELSE 'DRAFT'
                END;");

            // Revert InstallmentPlanStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""InstallmentPlans"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'ACTIVE'
                    WHEN 1 THEN 'PAID_OFF'
                    WHEN 2 THEN 'DEFAULTED'
                    WHEN 3 THEN 'CANCELLED'
                    ELSE 'ACTIVE'
                END;");

            // Revert InstallmentPaymentStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""InstallmentPayments"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'PENDING'
                    WHEN 1 THEN 'PAID'
                    WHEN 2 THEN 'OVERDUE'
                    WHEN 3 THEN 'CANCELLED'
                    ELSE 'PENDING'
                END;");

            // Revert DepositStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Deposits"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'PENDING'
                    WHEN 1 THEN 'PAID'
                    WHEN 2 THEN 'REFUNDED'
                    WHEN 3 THEN 'CANCELLED'
                    ELSE 'PENDING'
                END;");

            // Revert PaymentMethod enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Deposits"" ALTER COLUMN ""Method"" TYPE text 
                USING CASE ""Method""
                    WHEN 0 THEN 'CASH'
                    WHEN 1 THEN 'BANK_TRANSFER'
                    WHEN 2 THEN 'CREDIT_CARD'
                    WHEN 3 THEN 'ONLINE_PAYMENT'
                    ELSE 'CASH'
                END;");

            // Revert DealerContractStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""DealerContracts"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'DRAFT'
                    WHEN 1 THEN 'PENDING_SIGNATURE'
                    WHEN 2 THEN 'SIGNED'
                    WHEN 3 THEN 'EXPIRED'
                    WHEN 4 THEN 'TERMINATED'
                    ELSE 'DRAFT'
                END;");

            // Revert ContractStatus enum
            migrationBuilder.Sql(@"
                ALTER TABLE ""Contracts"" ALTER COLUMN ""Status"" TYPE text 
                USING CASE ""Status""
                    WHEN 0 THEN 'DRAFT'
                    WHEN 1 THEN 'PENDING_SIGNATURE'
                    WHEN 2 THEN 'SIGNED'
                    WHEN 3 THEN 'CANCELLED'
                    WHEN 4 THEN 'COMPLETED'
                    ELSE 'DRAFT'
                END;");
        }
    }
}
