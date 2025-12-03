using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DELTAAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOpcionesToPregunta : Migration
    {
        /// <inheritdoc />
     protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
          name: "opciones",
       table: "PREGUNTA",
                type: "nvarchar(max)",
    nullable: true);
        }

        /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.DropColumn(
          name: "opciones",
             table: "PREGUNTA");
        }
    }
}
