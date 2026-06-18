using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace avans_1._4_ai_system_integration_api.Migrations
{
    /// <inheritdoc />
    public partial class TrashDetection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrashDetections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CameraLatitude = table.Column<double>(type: "float", nullable: false),
                    CameraLongitude = table.Column<double>(type: "float", nullable: false),
                    PhotoTakenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TemperatureCelsius = table.Column<double>(type: "float", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Statiegeld = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrashDetections", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrashDetections");
        }
    }
}
