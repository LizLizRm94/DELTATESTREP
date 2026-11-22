using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DELTAAPI.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarConvocatoriaFormal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo",
                table: "CONVOCATORIA",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "contacto",
                table: "CONVOCATORIA",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "creada_por_usuario_id",
                table: "CONVOCATORIA",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "creada_utc",
                table: "CONVOCATORIA",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "cronograma",
                table: "CONVOCATORIA",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "descripcion",
                table: "CONVOCATORIA",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "entidad_emisora",
                table: "CONVOCATORIA",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_cierre",
                table: "CONVOCATORIA",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "instrucciones",
                table: "CONVOCATORIA",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "lugar",
                table: "CONVOCATORIA",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "modalidad",
                table: "CONVOCATORIA",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "objetivo",
                table: "CONVOCATORIA",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "participantes",
                table: "CONVOCATORIA",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "publicada",
                table: "CONVOCATORIA",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "requisitos",
                table: "CONVOCATORIA",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "resumen",
                table: "CONVOCATORIA",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CONVOCATORIA_codigo",
                table: "CONVOCATORIA",
                column: "codigo",
                unique: true,
                filter: "codigo <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_CONVOCATORIA_publicada_fecha_convocatoria_creada_utc",
                table: "CONVOCATORIA",
                columns: new[] { "publicada", "fecha_convocatoria", "creada_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONVOCATORIA_codigo",
                table: "CONVOCATORIA");

            migrationBuilder.DropIndex(
                name: "IX_CONVOCATORIA_publicada_fecha_convocatoria_creada_utc",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "codigo",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "contacto",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "creada_por_usuario_id",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "creada_utc",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "cronograma",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "descripcion",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "entidad_emisora",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "fecha_cierre",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "instrucciones",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "lugar",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "modalidad",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "objetivo",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "participantes",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "publicada",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "requisitos",
                table: "CONVOCATORIA");

            migrationBuilder.DropColumn(
                name: "resumen",
                table: "CONVOCATORIA");
        }
    }
}
