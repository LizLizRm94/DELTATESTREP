using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DELTAAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRespuestaCorrectaIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
       migrationBuilder.AddColumn<int>(
        name: "respuesta_correcta_index",
       table: "PREGUNTA",
      type: "int",
   nullable: true);
        }

    /// <inheritdoc />
     protected override void Down(MigrationBuilder migrationBuilder)
        {
      migrationBuilder.DropColumn(
           name: "respuesta_correcta_index",
          table: "PREGUNTA");
        }
    }
}
