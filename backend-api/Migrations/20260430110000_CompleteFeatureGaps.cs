using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WeatherAPI.Infrastructure.Data;

#nullable disable

namespace WeatherAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260430110000_CompleteFeatureGaps")]
    public partial class CompleteFeatureGaps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Parts",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Vendors_VendorId",
                table: "Parts");

            migrationBuilder.Sql("UPDATE \"Parts\" SET \"VendorId\" = NULL WHERE \"VendorId\" = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.AlterColumn<Guid>(
                name: "VendorId",
                table: "Parts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVehicles_VehicleNumber",
                table: "CustomerVehicles",
                column: "VehicleNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartCode",
                table: "Parts",
                column: "PartCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Vendors_VendorId",
                table: "Parts",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Vendors_VendorId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_CustomerVehicles_VehicleNumber",
                table: "CustomerVehicles");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PartCode",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Parts");

            migrationBuilder.AlterColumn<Guid>(
                name: "VendorId",
                table: "Parts",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Vendors_VendorId",
                table: "Parts",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
