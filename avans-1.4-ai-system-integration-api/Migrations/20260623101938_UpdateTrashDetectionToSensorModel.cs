using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace avans_1._4_ai_system_integration_api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTrashDetectionToSensorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrashDetections_CameraLatitude_CameraLongitude_PhotoTakenAtUtc",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "CameraLatitude",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "CameraLongitude",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Statiegeld",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "TemperatureCelsius",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TrashDetections");

            migrationBuilder.RenameColumn(
                name: "PhotoTakenAtUtc",
                table: "TrashDetections",
                newName: "DateTime");

            migrationBuilder.AddColumn<float>(
                name: "Confidence",
                table: "TrashDetections",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "TrashDetections",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<float>(
                name: "Latitude",
                table: "TrashDetections",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Longitude",
                table: "TrashDetections",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Rain",
                table: "TrashDetections",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<Guid>(
                name: "SensorId",
                table: "TrashDetections",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<float>(
                name: "Temperature",
                table: "TrashDetections",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "TrashType",
                table: "TrashDetections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TrashDetections_Latitude_Longitude_DateTime",
                table: "TrashDetections",
                columns: new[] { "Latitude", "Longitude", "DateTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrashDetections_Latitude_Longitude_DateTime",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Rain",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "SensorId",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "TrashType",
                table: "TrashDetections");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "TrashDetections",
                newName: "PhotoTakenAtUtc");

            migrationBuilder.AddColumn<double>(
                name: "CameraLatitude",
                table: "TrashDetections",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CameraLongitude",
                table: "TrashDetections",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "Statiegeld",
                table: "TrashDetections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "TemperatureCelsius",
                table: "TrashDetections",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TrashDetections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrashDetections_CameraLatitude_CameraLongitude_PhotoTakenAtUtc",
                table: "TrashDetections",
                columns: new[] { "CameraLatitude", "CameraLongitude", "PhotoTakenAtUtc" },
                unique: true);
        }
    }
}
