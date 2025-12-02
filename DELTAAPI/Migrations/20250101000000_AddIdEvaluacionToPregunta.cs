using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DELTAAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIdEvaluacionToPregunta : Migration
    {
  /// <inheritdoc />
   protected override void Up(MigrationBuilder migrationBuilder)
  {
migrationBuilder.AddColumn<int>(
   name: "id_evaluacion",
    table: "PREGUNTA",
          type: "int",
      nullable: true);

migrationBuilder.CreateIndex(
                name: "IX_PREGUNTA_id_evaluacion",
      table: "PREGUNTA",
      column: "id_evaluacion");

      migrationBuilder.AddForeignKey(
       name: "FK_Pregunta_Evaluacion",
             table: "PREGUNTA",
          column: "id_evaluacion",
principalTable: "EVALUACION",
          principalColumn: "id_evaluacion");
        }

  /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.DropForeignKey(
    name: "FK_Pregunta_Evaluacion",
     table: "PREGUNTA");

   migrationBuilder.DropIndex(
                name: "IX_PREGUNTA_id_evaluacion",
      table: "PREGUNTA");

            migrationBuilder.DropColumn(
         name: "id_evaluacion",
    table: "PREGUNTA");
        }
    }
}
