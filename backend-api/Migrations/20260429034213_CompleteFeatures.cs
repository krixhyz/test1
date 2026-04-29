using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherAPI.Migrations
{
    /// <inheritdoc />
    public partial class CompleteFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "UnavailablePartRequests",
                newName: "RequestDate");

            migrationBuilder.AddColumn<string>(
                name: "Urgency",
                table: "UnavailablePartRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId",
                table: "UnavailablePartRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Technician = table.Column<string>(type: "text", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceHistories_CustomerVehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "CustomerVehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceHistories_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnavailablePartRequests_VehicleId",
                table: "UnavailablePartRequests",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHistories_CustomerId",
                table: "ServiceHistories",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHistories_VehicleId",
                table: "ServiceHistories",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_UnavailablePartRequests_CustomerVehicles_VehicleId",
                table: "UnavailablePartRequests",
                column: "VehicleId",
                principalTable: "CustomerVehicles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnavailablePartRequests_CustomerVehicles_VehicleId",
                table: "UnavailablePartRequests");

            migrationBuilder.DropTable(
                name: "ServiceHistories");

            migrationBuilder.DropIndex(
                name: "IX_UnavailablePartRequests_VehicleId",
                table: "UnavailablePartRequests");

            migrationBuilder.DropColumn(
                name: "Urgency",
                table: "UnavailablePartRequests");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "UnavailablePartRequests");

            migrationBuilder.RenameColumn(
                name: "RequestDate",
                table: "UnavailablePartRequests",
                newName: "Date");
        }
    }
}
