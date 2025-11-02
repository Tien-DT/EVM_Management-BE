using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContractSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Customers_CustomerId",
                table: "Contracts");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Contracts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "ContractType",
                table: "Contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DealerId",
                table: "Contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SignedByUserId",
                table: "Contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_DealerId",
                table: "Contracts",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_SignedByUserId",
                table: "Contracts",
                column: "SignedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Customers_CustomerId",
                table: "Contracts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Dealers_DealerId",
                table: "Contracts",
                column: "DealerId",
                principalTable: "Dealers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_UserProfiles_SignedByUserId",
                table: "Contracts",
                column: "SignedByUserId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Customers_CustomerId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Dealers_DealerId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_UserProfiles_SignedByUserId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_DealerId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_SignedByUserId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ContractType",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "DealerId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SignedByUserId",
                table: "Contracts");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Contracts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Customers_CustomerId",
                table: "Contracts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
