using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DELTAAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPuntosAndOpcionesAEliminarToPregunta : Migration
    {
        /// <inheritdoc />
   protected override void Up(MigrationBuilder migrationBuilder)
        {
   migrationBuilder.AddColumn<int>(
 name: "puntos",
     table: "PREGUNTA",
     type: "int",
                nullable: false,
       defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
   name: "permite_eliminar_opciones",
     table: "PREGUNTA",
     type: "bit",
           nullable: false,
     defaultValue: true);
        }

    /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
      {
            migrationBuilder.DropColumn(
        name: "puntos",
         table: "PREGUNTA");

            migrationBuilder.DropColumn(
    name: "permite_eliminar_opciones",
      table: "PREGUNTA");
        }
    }
}
