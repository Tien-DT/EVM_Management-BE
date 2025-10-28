using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerIdToMasterTimeSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Deposits\" DROP COLUMN IF EXISTS \"BillImageUrl\";");

            migrationBuilder.AddColumn<Guid>(
                name: "DealerId",
                table: "MasterTimeSlots",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MasterTimeSlots_DealerId",
                table: "MasterTimeSlots",
                column: "DealerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MasterTimeSlots_Dealers_DealerId",
                table: "MasterTimeSlots",
                column: "DealerId",
                principalTable: "Dealers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MasterTimeSlots_Dealers_DealerId",
                table: "MasterTimeSlots");

            migrationBuilder.DropIndex(
                name: "IX_MasterTimeSlots_DealerId",
                table: "MasterTimeSlots");

            migrationBuilder.DropColumn(
                name: "DealerId",
                table: "MasterTimeSlots");

            migrationBuilder.AddColumn<string>(
                name: "BillImageUrl",
                table: "Deposits",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
