using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace avans_1._4_ai_system_integration_api.Migrations
{
    /// <inheritdoc />
    public partial class AddTrashDetectionAndFetchLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FetchedAtUtc",
                table: "TrashDetections",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "TrashDataFetchLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RangeFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RangeTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FetchedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrashDataFetchLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrashDataFetchLogs");

            migrationBuilder.DropColumn(
                name: "FetchedAtUtc",
                table: "TrashDetections");
        }
    }
}
