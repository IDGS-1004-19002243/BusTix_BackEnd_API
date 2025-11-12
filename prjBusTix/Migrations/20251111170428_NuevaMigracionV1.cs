using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace prjBusTix.Migrations
{
    /// <inheritdoc />
    public partial class NuevaMigracionV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "ViajesStaff",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "ViajesStaff");
        }
    }
}
