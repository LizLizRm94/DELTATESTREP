using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DELTAAPI.Migrations
{
    /// <inheritdoc />
    public partial class ConservarDetalleYPuntajes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "puntos_obtenidos",
                table: "PREGUNTA",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "detalle_practico_json",
                table: "EVALUACION",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "puntos_obtenidos",
                table: "PREGUNTA");

            migrationBuilder.DropColumn(
                name: "detalle_practico_json",
                table: "EVALUACION");
        }
    }
}
