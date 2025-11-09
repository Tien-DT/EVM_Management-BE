using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddManagedByToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ManagedBy",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ManagedBy",
                table: "Customers",
                column: "ManagedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Accounts_ManagedBy",
                table: "Customers",
                column: "ManagedBy",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Accounts_ManagedBy",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_ManagedBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ManagedBy",
                table: "Customers");
        }
    }
}
