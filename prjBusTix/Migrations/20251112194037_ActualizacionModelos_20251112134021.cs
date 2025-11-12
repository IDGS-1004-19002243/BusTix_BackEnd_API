using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace prjBusTix.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionModelos_20251112134021 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CheckInLat",
                table: "ManifiestoPasajeros",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CheckInLong",
                table: "ManifiestoPasajeros",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstatusCheckIn",
                table: "ManifiestoPasajeros",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCheckIn",
                table: "ManifiestoPasajeros",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesCheckIn",
                table: "ManifiestoPasajeros",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInLat",
                table: "ManifiestoPasajeros");

            migrationBuilder.DropColumn(
                name: "CheckInLong",
                table: "ManifiestoPasajeros");

            migrationBuilder.DropColumn(
                name: "EstatusCheckIn",
                table: "ManifiestoPasajeros");

            migrationBuilder.DropColumn(
                name: "FechaCheckIn",
                table: "ManifiestoPasajeros");

            migrationBuilder.DropColumn(
                name: "ObservacionesCheckIn",
                table: "ManifiestoPasajeros");
        }
    }
}
