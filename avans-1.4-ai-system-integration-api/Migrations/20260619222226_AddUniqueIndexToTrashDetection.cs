using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace avans_1._4_ai_system_integration_api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToTrashDetection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TrashDetections_CameraLatitude_CameraLongitude_PhotoTakenAtUtc",
                table: "TrashDetections",
                columns: new[] { "CameraLatitude", "CameraLongitude", "PhotoTakenAtUtc" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrashDetections_CameraLatitude_CameraLongitude_PhotoTakenAtUtc",
                table: "TrashDetections");
        }
    }
}
