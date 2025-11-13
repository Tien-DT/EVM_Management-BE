using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTransportDetailIdWithTransportIdInHandoverRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HandoverRecords_TransportDetails_TransportDetailId",
                table: "HandoverRecords");

            migrationBuilder.DropIndex(
                name: "IX_HandoverRecords_TransportDetailId",
                table: "HandoverRecords");

            migrationBuilder.RenameColumn(
                name: "TransportDetailId",
                table: "HandoverRecords",
                newName: "TransportId");

            migrationBuilder.CreateIndex(
                name: "IX_HandoverRecords_TransportId",
                table: "HandoverRecords",
                column: "TransportId");

            migrationBuilder.AddForeignKey(
                name: "FK_HandoverRecords_Transports_TransportId",
                table: "HandoverRecords",
                column: "TransportId",
                principalTable: "Transports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HandoverRecords_Transports_TransportId",
                table: "HandoverRecords");

            migrationBuilder.DropIndex(
                name: "IX_HandoverRecords_TransportId",
                table: "HandoverRecords");

            migrationBuilder.RenameColumn(
                name: "TransportId",
                table: "HandoverRecords",
                newName: "TransportDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_HandoverRecords_TransportDetailId",
                table: "HandoverRecords",
                column: "TransportDetailId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HandoverRecords_TransportDetails_TransportDetailId",
                table: "HandoverRecords",
                column: "TransportDetailId",
                principalTable: "TransportDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
