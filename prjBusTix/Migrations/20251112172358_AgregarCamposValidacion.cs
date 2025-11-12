using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace prjBusTix.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposValidacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceValidationId",
                table: "RegistroValidacion",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstacionLat",
                table: "RegistroValidacion",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstacionLong",
                table: "RegistroValidacion",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "RegistroValidacion",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoValidacion",
                table: "RegistroValidacion",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceValidationId",
                table: "RegistroValidacion");

            migrationBuilder.DropColumn(
                name: "EstacionLat",
                table: "RegistroValidacion");

            migrationBuilder.DropColumn(
                name: "EstacionLong",
                table: "RegistroValidacion");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "RegistroValidacion");

            migrationBuilder.DropColumn(
                name: "TipoValidacion",
                table: "RegistroValidacion");
        }
    }
}
