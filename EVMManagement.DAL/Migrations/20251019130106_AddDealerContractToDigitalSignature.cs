using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerContractToDigitalSignature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DigitalSignatures_ContractId_HandoverRecordId",
                table: "DigitalSignatures");

            migrationBuilder.AddColumn<Guid>(
                name: "DealerContractId",
                table: "DigitalSignatures",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_ContractId_HandoverRecordId_DealerContrac~",
                table: "DigitalSignatures",
                columns: new[] { "ContractId", "HandoverRecordId", "DealerContractId" });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_DealerContractId",
                table: "DigitalSignatures",
                column: "DealerContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalSignatures_DealerContracts_DealerContractId",
                table: "DigitalSignatures",
                column: "DealerContractId",
                principalTable: "DealerContracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DigitalSignatures_DealerContracts_DealerContractId",
                table: "DigitalSignatures");

            migrationBuilder.DropIndex(
                name: "IX_DigitalSignatures_ContractId_HandoverRecordId_DealerContrac~",
                table: "DigitalSignatures");

            migrationBuilder.DropIndex(
                name: "IX_DigitalSignatures_DealerContractId",
                table: "DigitalSignatures");

            migrationBuilder.DropColumn(
                name: "DealerContractId",
                table: "DigitalSignatures");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_ContractId_HandoverRecordId",
                table: "DigitalSignatures",
                columns: new[] { "ContractId", "HandoverRecordId" });
        }
    }
}
