using Microsoft.EntityFrameworkCore.Migrations;

namespace DELTAAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPermiteEliminarOpcionesAndPuntosToPreg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        migrationBuilder.AddColumn<bool>(
          name: "permite_eliminar_opciones",
     table: "PREGUNTA",
         type: "bit",
         nullable: false,
   defaultValue: true);

       migrationBuilder.AddColumn<int>(
       name: "puntos",
             table: "PREGUNTA",
         type: "int",
                nullable: false,
    defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
     name: "permite_eliminar_opciones",
     table: "PREGUNTA");

            migrationBuilder.DropColumn(
 name: "puntos",
      table: "PREGUNTA");
   }
    }
}
