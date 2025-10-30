using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerIdToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DealerId",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_DealerId",
                table: "Customers",
                column: "DealerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Dealers_DealerId",
                table: "Customers",
                column: "DealerId",
                principalTable: "Dealers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Dealers_DealerId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_DealerId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DealerId",
                table: "Customers");
        }
    }
}
