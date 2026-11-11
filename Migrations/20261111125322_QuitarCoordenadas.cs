using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgentinaAccesible.Migrations
{
    /// <inheritdoc />
    public partial class QuitarCoordenadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitud",
                table: "Destinos");

            migrationBuilder.DropColumn(
                name: "Longitud",
                table: "Destinos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitud",
                table: "Destinos",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitud",
                table: "Destinos",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
