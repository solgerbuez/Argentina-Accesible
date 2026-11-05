using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgentinaAccesible.Migrations
{
    /// <inheritdoc />
    public partial class RestringirBorrados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DestinoCaracteristicas_Caracteristicas_CaracteristicaAccesibilidadId",
                table: "DestinoCaracteristicas");

            migrationBuilder.DropForeignKey(
                name: "FK_Destinos_Categorias_CategoriaId",
                table: "Destinos");

            migrationBuilder.DropForeignKey(
                name: "FK_Destinos_Provincias_ProvinciaId",
                table: "Destinos");

            migrationBuilder.AddForeignKey(
                name: "FK_DestinoCaracteristicas_Caracteristicas_CaracteristicaAccesibilidadId",
                table: "DestinoCaracteristicas",
                column: "CaracteristicaAccesibilidadId",
                principalTable: "Caracteristicas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Destinos_Categorias_CategoriaId",
                table: "Destinos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Destinos_Provincias_ProvinciaId",
                table: "Destinos",
                column: "ProvinciaId",
                principalTable: "Provincias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DestinoCaracteristicas_Caracteristicas_CaracteristicaAccesibilidadId",
                table: "DestinoCaracteristicas");

            migrationBuilder.DropForeignKey(
                name: "FK_Destinos_Categorias_CategoriaId",
                table: "Destinos");

            migrationBuilder.DropForeignKey(
                name: "FK_Destinos_Provincias_ProvinciaId",
                table: "Destinos");

            migrationBuilder.AddForeignKey(
                name: "FK_DestinoCaracteristicas_Caracteristicas_CaracteristicaAccesibilidadId",
                table: "DestinoCaracteristicas",
                column: "CaracteristicaAccesibilidadId",
                principalTable: "Caracteristicas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Destinos_Categorias_CategoriaId",
                table: "Destinos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Destinos_Provincias_ProvinciaId",
                table: "Destinos",
                column: "ProvinciaId",
                principalTable: "Provincias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
