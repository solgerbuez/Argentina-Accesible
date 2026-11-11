using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgentinaAccesible.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPendienteTestimonio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Pendiente",
                table: "Testimonios",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pendiente",
                table: "Testimonios");
        }
    }
}
