using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVMManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDigitalSignatureTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvailableSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterSlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailableSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailableSlots_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvailableSlots_MasterTimeSlots_MasterSlotId",
                        column: x => x.MasterSlotId,
                        principalTable: "MasterTimeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvailableSlots_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DigitalSignatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SignerEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SignerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SignatureData = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EntityType = table.Column<int>(type: "integer", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    HandoverRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OtpCode = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OtpExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OtpAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationCode = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalSignatures_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DigitalSignatures_HandoverRecords_HandoverRecordId",
                        column: x => x.HandoverRecordId,
                        principalTable: "HandoverRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailableSlots_DealerId",
                table: "AvailableSlots",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailableSlots_MasterSlotId",
                table: "AvailableSlots",
                column: "MasterSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailableSlots_VehicleId",
                table: "AvailableSlots",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_ContractId_HandoverRecordId",
                table: "DigitalSignatures",
                columns: new[] { "ContractId", "HandoverRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_HandoverRecordId",
                table: "DigitalSignatures",
                column: "HandoverRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_SignerEmail_Status_OtpExpiresAt",
                table: "DigitalSignatures",
                columns: new[] { "SignerEmail", "Status", "OtpExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_VerificationCode",
                table: "DigitalSignatures",
                column: "VerificationCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailableSlots");

            migrationBuilder.DropTable(
                name: "DigitalSignatures");
        }
    }
}
