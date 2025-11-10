using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MasterTimeSlotInheritFromBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MasterTimeSlots_Code",
                table: "MasterTimeSlots");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "MasterTimeSlots",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "MasterTimeSlots",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MasterTimeSlots",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "MasterTimeSlots",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "MasterTimeSlots");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "MasterTimeSlots");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MasterTimeSlots");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "MasterTimeSlots");

            migrationBuilder.CreateIndex(
                name: "IX_MasterTimeSlots_Code",
                table: "MasterTimeSlots",
                column: "Code",
                unique: true);
        }
    }
}
