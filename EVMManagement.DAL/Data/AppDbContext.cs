using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Models;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Users & Access Control
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Customer> Customers { get; set; }

        // Products & Inventory
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<VehicleVariant> VehicleVariants { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        // Sales & Quotations
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationDetail> QuotationDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        // Legal & Agreements
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<DealerContract> DealerContracts { get; set; }

        // Financials & Payments
        public DbSet<Deposit> Deposits { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InstallmentPlan> InstallmentPlans { get; set; }
        public DbSet<InstallmentPayment> InstallmentPayments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }

        // Promotions
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<VehiclePromotion> VehiclePromotions { get; set; }

        // Logistics & Operations
        public DbSet<Transport> Transports { get; set; }
        public DbSet<TransportDetail> TransportDetails { get; set; }
        public DbSet<HandoverRecord> HandoverRecords { get; set; }
        public DbSet<MasterTimeSlot> MasterTimeSlots { get; set; }
        public DbSet<VehicleTimeSlot> VehicleTimeSlots { get; set; }
        public DbSet<TestDriveBooking> TestDriveBookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Dealer
            modelBuilder.Entity<Dealer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Account
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role)
                    .HasConversion<string>();
            });

            // Configure UserProfile
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.HasIndex(e => e.CardId).IsUnique();
                entity.HasOne(e => e.Account)
                    .WithOne(a => a.UserProfile)
                    .HasForeignKey<UserProfile>(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Dealer)
                    .WithMany(d => d.UserProfiles)
                    .HasForeignKey(e => e.DealerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.CardId).IsUnique();
            });

            // Configure VehicleModel
            modelBuilder.Entity<VehicleModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // Configure VehicleVariant
            modelBuilder.Entity<VehicleVariant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.VehicleModel)
                    .WithMany(m => m.VehicleVariants)
                    .HasForeignKey(e => e.ModelId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Warehouse
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type)
                    .HasConversion<string>();
                entity.HasOne(e => e.Dealer)
                    .WithMany(d => d.Warehouses)
                    .HasForeignKey(e => e.DealerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Vehicle
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Vin).IsUnique();
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.Property(e => e.Purpose)
                    .HasConversion<string>();
                entity.HasOne(e => e.VehicleVariant)
                    .WithMany(v => v.Vehicles)
                    .HasForeignKey(e => e.VariantId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Warehouse)
                    .WithMany(w => w.Vehicles)
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Quotation
            modelBuilder.Entity<Quotation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Quotations)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.CreatedQuotations)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure QuotationDetail
            modelBuilder.Entity<QuotationDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Quotation)
                    .WithMany(q => q.QuotationDetails)
                    .HasForeignKey(e => e.QuotationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.VehicleVariant)
                    .WithMany(v => v.QuotationDetails)
                    .HasForeignKey(e => e.VehicleVariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.Property(e => e.OrderType)
                    .HasConversion<string>();
                entity.HasOne(e => e.Quotation)
                    .WithOne(q => q.Order)
                    .HasForeignKey<Order>(e => e.QuotationId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Dealer)
                    .WithMany(d => d.Orders)
                    .HasForeignKey(e => e.DealerId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.CreatedOrders)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure OrderDetail
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderDetails)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.VehicleVariant)
                    .WithMany(v => v.OrderDetails)
                    .HasForeignKey(e => e.VehicleVariantId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.OrderDetails)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Contract
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.HasOne(e => e.Order)
                    .WithOne(o => o.Contract)
                    .HasForeignKey<Contract>(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Contracts)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.CreatedContracts)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure DealerContract
            modelBuilder.Entity<DealerContract>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ContractCode).IsUnique();
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.HasOne(e => e.Dealer)
                    .WithOne(d => d.DealerContract)
                    .HasForeignKey<DealerContract>(e => e.DealerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.SignedByDealerUser)
                    .WithMany(u => u.SignedDealerContractsAsDealer)
                    .HasForeignKey(e => e.SignedByDealerUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.SignedByEvmUser)
                    .WithMany(u => u.SignedDealerContractsAsEVM)
                    .HasForeignKey(e => e.SignedByEvmUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Deposit
            modelBuilder.Entity<Deposit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Method)
                    .HasConversion<string>();
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.Deposits)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ReceivedByUser)
                    .WithMany(a => a.ReceivedDeposits)
                    .HasForeignKey(e => e.ReceivedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Invoice
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.InvoiceCode).IsUnique();
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.HasOne(e => e.Order)
                    .WithOne(o => o.Invoice)
                    .HasForeignKey<Invoice>(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure InstallmentPlan
            modelBuilder.Entity<InstallmentPlan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.HasOne(e => e.Order)
                    .WithOne(o => o.InstallmentPlan)
                    .HasForeignKey<InstallmentPlan>(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure InstallmentPayment
            modelBuilder.Entity<InstallmentPayment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.HasOne(e => e.InstallmentPlan)
                    .WithMany(p => p.InstallmentPayments)
                    .HasForeignKey(e => e.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.HasOne(e => e.Invoice)
                    .WithMany(i => i.Transactions)
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Deposit)
                    .WithMany(d => d.Transactions)
                    .HasForeignKey(e => e.DepositId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.InstallmentPayment)
                    .WithMany(i => i.Transactions)
                    .HasForeignKey(e => e.InstallmentPaymentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure BankAccount
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AccountNumber).IsUnique();
                entity.HasOne(e => e.Dealer)
                    .WithMany(d => d.BankAccounts)
                    .HasForeignKey(e => e.DealerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Promotion
            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // Configure VehiclePromotion (Many-to-Many)
            modelBuilder.Entity<VehiclePromotion>(entity =>
            {
                entity.HasKey(vp => new { vp.VariantId, vp.PromotionId });
                entity.HasOne(vp => vp.VehicleVariant)
                    .WithMany(v => v.VehiclePromotions)
                    .HasForeignKey(vp => vp.VariantId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(vp => vp.Promotion)
                    .WithMany(p => p.VehiclePromotions)
                    .HasForeignKey(vp => vp.PromotionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Transport
            modelBuilder.Entity<Transport>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status)
                    .HasConversion<string>();
            });

            // Configure TransportDetail
            modelBuilder.Entity<TransportDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Transport)
                    .WithMany(t => t.TransportDetails)
                    .HasForeignKey(e => e.TransportId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Vehicle)
                    .WithOne(v => v.TransportDetail)
                    .HasForeignKey<TransportDetail>(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure HandoverRecord
            modelBuilder.Entity<HandoverRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Order)
                    .WithOne(o => o.HandoverRecord)
                    .HasForeignKey<HandoverRecord>(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Vehicle)
                    .WithOne(v => v.HandoverRecord)
                    .HasForeignKey<HandoverRecord>(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.TransportDetail)
                    .WithOne(t => t.HandoverRecord)
                    .HasForeignKey<HandoverRecord>(e => e.TransportDetailId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure MasterTimeSlot
            modelBuilder.Entity<MasterTimeSlot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // Configure VehicleTimeSlot
            modelBuilder.Entity<VehicleTimeSlot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.VehicleTimeSlots)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Dealer)
                    .WithMany(d => d.VehicleTimeSlots)
                    .HasForeignKey(e => e.DealerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.MasterSlot)
                    .WithMany(m => m.VehicleTimeSlots)
                    .HasForeignKey(e => e.MasterSlotId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure TestDriveBooking
            modelBuilder.Entity<TestDriveBooking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status)
                    .HasConversion<string>();
                entity.HasOne(e => e.VehicleTimeSlot)
                    .WithOne(v => v.TestDriveBooking)
                    .HasForeignKey<TestDriveBooking>(e => e.VehicleTimeslotId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.TestDriveBookings)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.DealerStaff)
                    .WithMany(u => u.AssistedTestDriveBookings)
                    .HasForeignKey(e => e.DealerStaffId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
