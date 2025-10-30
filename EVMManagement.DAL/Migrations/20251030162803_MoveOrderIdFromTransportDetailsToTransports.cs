using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MoveOrderIdFromTransportDetailsToTransports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportDetails_Orders_OrderId",
                table: "TransportDetails");

            migrationBuilder.DropIndex(
                name: "IX_TransportDetails_OrderId",
                table: "TransportDetails");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "TransportDetails");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "Transports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transports_OrderId",
                table: "Transports",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transports_Orders_OrderId",
                table: "Transports",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transports_Orders_OrderId",
                table: "Transports");

            migrationBuilder.DropIndex(
                name: "IX_Transports_OrderId",
                table: "Transports");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Transports");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "TransportDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportDetails_OrderId",
                table: "TransportDetails",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportDetails_Orders_OrderId",
                table: "TransportDetails",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
