using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnumMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================
            // MIGRATE OrderStatus ENUM
            // ============================================
            // OLD MAPPING:
            // 0=CONFIRMED, 1=AWAITING_DEPOSIT, 2=DEPOSIT_PAID, 3=PROCESSING, 
            // 4=READY_FOR_DELIVERY, 5=IN_TRANSIT, 6=DELIVERED, 7=COMPLETED, 8=CANCELLED
            //
            // NEW MAPPING:
            // 0=CONFIRMED, 1=QUOTATION_RECEIVED, 2=QUOTATION_ACCEPTED, 3=QUOTATION_REJECTED,
            // 4=CREATED_CONTRACT, 5=SIGNED_CONTRACT, 6=AWAITING_DEPOSIT, 7=DEPOSIT_SUCCESS,
            // 8=IN_PROGRESS, 9=IN_TRANSIT, 10=READY_FOR_HANDOVER, 11=COMPLETED, 12=CANCELED

            migrationBuilder.Sql(@"
                UPDATE ""Orders""
                SET ""Status"" = CASE ""Status""
                    WHEN 0 THEN 0   -- CONFIRMED stays at 0
                    WHEN 1 THEN 6   -- AWAITING_DEPOSIT: 1 -> 6
                    WHEN 2 THEN 7   -- DEPOSIT_PAID -> DEPOSIT_SUCCESS: 2 -> 7
                    WHEN 3 THEN 8   -- PROCESSING -> IN_PROGRESS: 3 -> 8
                    WHEN 4 THEN 10  -- READY_FOR_DELIVERY -> READY_FOR_HANDOVER: 4 -> 10
                    WHEN 5 THEN 9   -- IN_TRANSIT: 5 -> 9
                    WHEN 6 THEN 9   -- DELIVERED -> IN_TRANSIT: 6 -> 9 (merged with IN_TRANSIT)
                    WHEN 7 THEN 11  -- COMPLETED: 7 -> 11
                    WHEN 8 THEN 12  -- CANCELLED -> CANCELED: 8 -> 12
                    ELSE ""Status""
                END
                WHERE ""Status"" IN (1, 2, 3, 4, 5, 6, 7, 8);
            ");

            // ============================================
            // MIGRATE TransportStatus ENUM
            // ============================================
            // OLD MAPPING:
            // 0=PENDING, 1=IN_TRANSIT, 2=DELIVERED, 3=CANCELLED
            //
            // NEW MAPPING:
            // 0=PENDING, 1=IN_TRANSIT, 2=DELIVERED, 3=FAILED, 4=COMPLETED, 5=CANCELED

            migrationBuilder.Sql(@"
                UPDATE ""Transports""
                SET ""Status"" = CASE ""Status""
                    WHEN 0 THEN 0   -- PENDING stays at 0
                    WHEN 1 THEN 1   -- IN_TRANSIT stays at 1
                    WHEN 2 THEN 2   -- DELIVERED stays at 2
                    WHEN 3 THEN 5   -- CANCELLED -> CANCELED: 3 -> 5
                    ELSE ""Status""
                END
                WHERE ""Status"" = 3;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ============================================
            // REVERT OrderStatus ENUM
            // ============================================
            migrationBuilder.Sql(@"
                UPDATE ""Orders""
                SET ""Status"" = CASE ""Status""
                    WHEN 0 THEN 0   -- CONFIRMED stays at 0
                    WHEN 6 THEN 1   -- AWAITING_DEPOSIT: 6 -> 1
                    WHEN 7 THEN 2   -- DEPOSIT_SUCCESS -> DEPOSIT_PAID: 7 -> 2
                    WHEN 8 THEN 3   -- IN_PROGRESS -> PROCESSING: 8 -> 3
                    WHEN 10 THEN 4  -- READY_FOR_HANDOVER -> READY_FOR_DELIVERY: 10 -> 4
                    WHEN 9 THEN 5   -- IN_TRANSIT: 9 -> 5
                    WHEN 11 THEN 7  -- COMPLETED: 11 -> 7
                    WHEN 12 THEN 8  -- CANCELED -> CANCELLED: 12 -> 8
                    -- NEW values that don't exist in old schema will be set to CONFIRMED
                    WHEN 1 THEN 0   -- QUOTATION_RECEIVED -> CONFIRMED
                    WHEN 2 THEN 0   -- QUOTATION_ACCEPTED -> CONFIRMED
                    WHEN 3 THEN 0   -- QUOTATION_REJECTED -> CONFIRMED
                    WHEN 4 THEN 0   -- CREATED_CONTRACT -> CONFIRMED
                    WHEN 5 THEN 0   -- SIGNED_CONTRACT -> CONFIRMED
                    ELSE ""Status""
                END
                WHERE ""Status"" IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);
            ");

            // ============================================
            // REVERT TransportStatus ENUM
            // ============================================
            migrationBuilder.Sql(@"
                UPDATE ""Transports""
                SET ""Status"" = CASE ""Status""
                    WHEN 0 THEN 0   -- PENDING stays at 0
                    WHEN 1 THEN 1   -- IN_TRANSIT stays at 1
                    WHEN 2 THEN 2   -- DELIVERED stays at 2
                    WHEN 5 THEN 3   -- CANCELED -> CANCELLED: 5 -> 3
                    -- NEW values that don't exist in old schema will be set to PENDING
                    WHEN 3 THEN 0   -- FAILED -> PENDING
                    WHEN 4 THEN 2   -- COMPLETED -> DELIVERED
                    ELSE ""Status""
                END
                WHERE ""Status"" IN (3, 4, 5);
            ");
        }
    }
}
