using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DELTAAPI.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AREA",
                columns: table => new
                {
                    id_area = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_area = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AREA__8A8C837B5453672E", x => x.id_area);
                });

            migrationBuilder.CreateTable(
                name: "CONVOCATORIA",
                columns: table => new
                {
                    id_convocatoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_convocatoria = table.Column<DateOnly>(type: "date", nullable: true),
                    nombre_convocatoria = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    estado_convocatoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CONVOCAT__2EAE64DB1B80B372", x => x.id_convocatoria);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_completo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    contraseña = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ci = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    correo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    expedicion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tipo_evaluado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    fecha_ingreso = table.Column<DateOnly>(type: "date", nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    puesto_actual = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    puesto_solicitado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    puesto_a_rotar = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    id_supervisor = table.Column<int>(type: "integer", nullable: true),
                    id_creado_por = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__USUARIO__4E3E04AD77A0E458", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK__USUARIO__id_crea__3A81B327",
                        column: x => x.id_creado_por,
                        principalTable: "USUARIO",
                        principalColumn: "id_usuario");
                    table.ForeignKey(
                        name: "FK__USUARIO__id_supe__398D8EEE",
                        column: x => x.id_supervisor,
                        principalTable: "USUARIO",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "DATO_ACADEMICO",
                columns: table => new
                {
                    id_dato_academico = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    titulo_academico = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    lugar_estudio = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    carrera = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DATO_ACA__AB51D4842666D8A3", x => x.id_dato_academico);
                    table.ForeignKey(
                        name: "FK__DATO_ACAD__id_us__3D5E1FD2",
                        column: x => x.id_usuario,
                        principalTable: "USUARIO",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "EVALUACION",
                columns: table => new
                {
                    id_evaluacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_evaluado = table.Column<int>(type: "integer", nullable: false),
                    id_administrador = table.Column<int>(type: "integer", nullable: true),
                    id_area = table.Column<int>(type: "integer", nullable: true),
                    fecha_evaluacion = table.Column<DateOnly>(type: "date", nullable: true),
                    nota = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    estado_evaluacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tipo_evaluacion = table.Column<bool>(type: "boolean", nullable: true),
                    recomendaciones = table.Column<string>(type: "text", nullable: true) // ensured text for PostgreSQL compatibility
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EVALUACI__65DE60C5F6D38ADE", x => x.id_evaluacion);
                    table.ForeignKey(
                        name: "FK__EVALUACIO__id_ad__49C3F6B7",
                        column: x => x.id_administrador,
                        principalTable: "USUARIO",
                        principalColumn: "id_usuario");
                    table.ForeignKey(
                        name: "FK__EVALUACIO__id_ar__4AB81AF0",
                        column: x => x.id_area,
                        principalTable: "AREA",
                        principalColumn: "id_area");
                    table.ForeignKey(
                        name: "FK__EVALUACIO__id_ev__48CFD27E",
                        column: x => x.id_evaluado,
                        principalTable: "USUARIO",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "EVALUADO_CONVOCATORIA",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_convocatoria = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EVALUADO__ECD4E2E08E56B8A5", x => new { x.id_usuario, x.id_convocatoria });
                    table.ForeignKey(
                        name: "FK__EVALUADO___id_co__45F365D3",
                        column: x => x.id_convocatoria,
                        principalTable: "CONVOCATORIA",
                        principalColumn: "id_convocatoria");
                    table.ForeignKey(
                        name: "FK__EVALUADO___id_us__44FF419A",
                        column: x => x.id_usuario,
                        principalTable: "USUARIO",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICACION",
                columns: table => new
                {
                    id_notificacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_evaluacion = table.Column<int>(type: "integer", nullable: true),
                    id_administrador = table.Column<int>(type: "integer", nullable: true),
                    id_usuario_destino = table.Column<int>(type: "integer", nullable: false),
                    mensaje = table.Column<string>(type: "text", nullable: true),
                    fecha_envio = table.Column<DateTime>(type: "timestamp", nullable: true),
                    tipo_notificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NOTIFICA__8270F9A58C1F6715", x => x.id_notificacion);
                    table.ForeignKey(
                        name: "FK__NOTIFICAC__id_ad__4E88ABD4",
                        column: x => x.id_administrador,
                        principalTable: "USUARIO",
                        principalColumn: "id_usuario");
                    table.ForeignKey(
                        name: "FK__NOTIFICAC__id_ev__4D94879B",
                        column: x => x.id_evaluacion,
                        principalTable: "EVALUACION",
                        principalColumn: "id_evaluacion");
                    table.ForeignKey(
                        name: "FK__NOTIFICAC__id_us__4F7CD00D",
                        column: x => x.id_usuario_destino,
                        principalTable: "USUARIO",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "PREGUNTA",
                columns: table => new
                {
                    id_pregunta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    texto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tipo_evaluacion = table.Column<bool>(type: "boolean", nullable: false),
                    id_evaluacion = table.Column<int>(type: "integer", nullable: true),
                    opciones = table.Column<string>(type: "text", nullable: true),
                    respuesta_correcta_index = table.Column<int>(type: "integer", nullable: true),
                    puntos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PREGUNTA__3C69FB99", x => x.id_pregunta);
                    table.ForeignKey(
                        name: "FK_Pregunta_Evaluacion",
                        column: x => x.id_evaluacion,
                        principalTable: "EVALUACION",
                        principalColumn: "id_evaluacion");
                });

            migrationBuilder.CreateTable(
                name: "RESPUESTA",
                columns: table => new
                {
                    id_respuesta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_pregunta = table.Column<int>(type: "integer", nullable: false),
                    id_evaluacion = table.Column<int>(type: "integer", nullable: false),
                    texto_respuesta = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RESPUEST__1AAA640C", x => x.id_respuesta);
                    table.ForeignKey(
                        name: "FK_Respuesta_Evaluacion",
                        column: x => x.id_evaluacion,
                        principalTable: "EVALUACION",
                        principalColumn: "id_evaluacion");
                    table.ForeignKey(
                        name: "FK_Respuesta_Pregunta",
                        column: x => x.id_pregunta,
                        principalTable: "PREGUNTA",
                        principalColumn: "id_pregunta");
                    table.ForeignKey(
                        name: "FK_Respuesta_Usuario",
                        column: x => x.id_usuario,
                        principalTable: "USUARIO",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateIndex(
                name: "UQ__AREA__1346D27AC7230296",
                table: "AREA",
                column: "nombre_area",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DATO_ACADEMICO_id_usuario",
                table: "DATO_ACADEMICO",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_EVALUACION_id_administrador",
                table: "EVALUACION",
                column: "id_administrador");

            migrationBuilder.CreateIndex(
                name: "IX_EVALUACION_id_area",
                table: "EVALUACION",
                column: "id_area");

            migrationBuilder.CreateIndex(
                name: "IX_EVALUACION_id_evaluado",
                table: "EVALUACION",
                column: "id_evaluado");

            migrationBuilder.CreateIndex(
                name: "IX_EVALUADO_CONVOCATORIA_id_convocatoria",
                table: "EVALUADO_CONVOCATORIA",
                column: "id_convocatoria");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_id_administrador",
                table: "NOTIFICACION",
                column: "id_administrador");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_id_evaluacion",
                table: "NOTIFICACION",
                column: "id_evaluacion");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_id_usuario_destino",
                table: "NOTIFICACION",
                column: "id_usuario_destino");

            migrationBuilder.CreateIndex(
                name: "IX_PREGUNTA_id_evaluacion",
                table: "PREGUNTA",
                column: "id_evaluacion");

            migrationBuilder.CreateIndex(
                name: "IX_RESPUESTA_id_evaluacion",
                table: "RESPUESTA",
                column: "id_evaluacion");

            migrationBuilder.CreateIndex(
                name: "IX_RESPUESTA_id_pregunta",
                table: "RESPUESTA",
                column: "id_pregunta");

            migrationBuilder.CreateIndex(
                name: "IX_RESPUESTA_id_usuario",
                table: "RESPUESTA",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_id_creado_por",
                table: "USUARIO",
                column: "id_creado_por");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_id_supervisor",
                table: "USUARIO",
                column: "id_supervisor");

            migrationBuilder.CreateIndex(
                name: "UQ__USUARIO__2A586E0B2E389AD7",
                table: "USUARIO",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__USUARIO__32136662237C1C6E",
                table: "USUARIO",
                column: "ci",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DATO_ACADEMICO");

            migrationBuilder.DropTable(
                name: "EVALUADO_CONVOCATORIA");

            migrationBuilder.DropTable(
                name: "NOTIFICACION");

            migrationBuilder.DropTable(
                name: "RESPUESTA");

            migrationBuilder.DropTable(
                name: "CONVOCATORIA");

            migrationBuilder.DropTable(
                name: "PREGUNTA");

            migrationBuilder.DropTable(
                name: "EVALUACION");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "AREA");
        }
    }
}
