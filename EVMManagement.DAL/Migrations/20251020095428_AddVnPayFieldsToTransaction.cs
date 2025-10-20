using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddVnPayFieldsToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardType",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentGateway",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseCode",
                table: "Transactions",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecureHash",
                table: "Transactions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionInfo",
                table: "Transactions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VnpayTransactionCode",
                table: "Transactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VnpayTransactionNo",
                table: "Transactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CardType",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PaymentGateway",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ResponseCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SecureHash",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionInfo",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "VnpayTransactionCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "VnpayTransactionNo",
                table: "Transactions");
        }
    }
}
