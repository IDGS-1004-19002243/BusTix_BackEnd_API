using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace prjBusTix.Migrations
{
    public partial class AddDeviceValidationUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create a filtered unique index on DeviceValidationId to enforce idempotency at DB level
            migrationBuilder.CreateIndex(
                name: "IX_RegistroValidacion_DeviceValidationId",
                table: "RegistroValidacion",
                column: "DeviceValidationId",
                unique: true,
                filter: "[DeviceValidationId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistroValidacion_DeviceValidationId",
                table: "RegistroValidacion");
        }
    }
}
