using System.Net.Http.Json;
using DELTATEST.Models;

namespace DELTATEST.Services
{
    public class ReporteEvaluacionService
    {
        private readonly HttpClient _http;

        public ReporteEvaluacionService(HttpClient http)
        {
            _http = http;
        }

        public async Task<DetalleEvaluacionDto?> ObtenerEvaluacionAsync(int idEvaluacion)
        {
            try
            {
                var response = await _http.GetAsync($"api/evaluaciones/{idEvaluacion}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DetalleEvaluacionDto>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public string GenerarHtmlReporte(DetalleEvaluacionDto evaluacion)
        {
            var fechaFormato = evaluacion.FechaEvaluacion?.ToString("dd/MM/yyyy") ?? "N/A";
            var tipoEvaluacion = evaluacion.TipoEvaluacion ?? "N/A";
            var notaFormato = $"{evaluacion.Nota:F2}";
            var estado = ObtenerEstadoEvaluacion(evaluacion.Nota ?? 0);

            var html = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reporte de Evaluación</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{
            font-family: 'Arial', sans-serif;
            background-color: #f5f5f5;
            padding: 20px;
            color: #333;
        }}

        .container {{
            max-width: 800px;
            margin: 0 auto;
            background-color: white;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
        }}

        .header {{
            text-align: center;
            border-bottom: 3px solid #f58220;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }}

        .header h1 {{
            color: #f58220;
            font-size: 28px;
            margin-bottom: 5px;
        }}

        .header p {{
            color: #666;
            font-size: 14px;
        }}

        .section {{
            margin-bottom: 30px;
        }}

        .section-title {{
            font-size: 16px;
            font-weight: bold;
            color: #f58220;
            margin-bottom: 15px;
            padding-bottom: 10px;
            border-bottom: 2px solid #f58220;
        }}

        .info-grid {{
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
            margin-bottom: 20px;
        }}

        .info-item {{
            background-color: #f9f9f9;
            padding: 15px;
            border-radius: 5px;
            border-left: 4px solid #f58220;
        }}

        .info-label {{
            font-weight: bold;
            color: #333;
            font-size: 12px;
            text-transform: uppercase;
            margin-bottom: 5px;
        }}

        .info-value {{
            font-size: 16px;
            color: #333;
        }}

        .resultado-section {{
            background: linear-gradient(135deg, #f58220 0%, #ff9c42 100%);
            color: white;
            padding: 30px;
            border-radius: 10px;
            text-align: center;
            margin-bottom: 30px;
        }}

        .resultado-section h2 {{
            font-size: 24px;
            margin-bottom: 10px;
        }}

        .nota {{
            font-size: 48px;
            font-weight: bold;
            margin: 15px 0;
        }}

        .estado-badge {{
            display: inline-block;
            padding: 8px 16px;
            border-radius: 20px;
            font-weight: bold;
            margin-top: 10px;
            font-size: 14px;
        }}

        .estado-aprobado {{
            background-color: #28a745;
        }}

        .estado-desaprobado {{
            background-color: #dc3545;
        }}

        .pie-pagina {{
            text-align: center;
            border-top: 1px solid #ddd;
            padding-top: 20px;
            margin-top: 40px;
            color: #999;
            font-size: 12px;
        }}

        .timestamp {{
            color: #999;
            font-size: 11px;
            margin-top: 10px;
        }}

        @media print {{
            body {{
                background-color: white;
                padding: 0;
            }}

            .container {{
                box-shadow: none;
                max-width: 100%;
            }}

            .btn-group {{
                display: none;
            }}

            .no-print {{
                display: none;
            }}
        }}

        .btn-group {{
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #ddd;
        }}

        .btn {{
            padding: 10px 20px;
            margin: 5px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-size: 14px;
            font-weight: bold;
            text-decoration: none;
            display: inline-block;
        }}

        .btn-primary {{
            background-color: #f58220;
            color: white;
        }}

        .btn-primary:hover {{
            background-color: #d46a1a;
        }}

        .btn-secondary {{
            background-color: #6c757d;
            color: white;
        }}

        .btn-secondary:hover {{
            background-color: #5a6268;
        }}
    }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>REPORTE DE EVALUACIÓN</h1>
            <p>Sistema de Evaluación - DELTATEST</p>
        </div>

        <div class='resultado-section'>
            <h2>Resultado Final</h2>
            <div class='nota'>{notaFormato}/100</div>
            <p>Tipo de Evaluación: <strong>{tipoEvaluacion}</strong></p>
            <span class='estado-badge {(evaluacion.Nota >= 80 ? "estado-aprobado" : "estado-desaprobado")}'>
                {estado}
            </span>
        </div>

        <div class='section'>
            <div class='section-title'>Información del Evaluado</div>
            <div class='info-grid'>
                <div class='info-item'>
                    <div class='info-label'>Nombre Completo</div>
                    <div class='info-value'>{evaluacion.NombreEvaluado}</div>
                </div>
                <div class='info-item'>
                    <div class='info-label'>Cédula de Identidad</div>
                    <div class='info-value'>{evaluacion.CiEvaluado}</div>
                </div>
            </div>
        </div>

        <div class='section'>
            <div class='section-title'>Detalles de la Evaluación</div>
            <div class='info-grid'>
                <div class='info-item'>
                    <div class='info-label'>Fecha de Evaluación</div>
                    <div class='info-value'>{fechaFormato}</div>
                </div>
                <div class='info-item'>
                    <div class='info-label'>Estado</div>
                    <div class='info-value'>{evaluacion.EstadoEvaluacion}</div>
                </div>
                <div class='info-item'>
                    <div class='info-label'>Evaluador</div>
                    <div class='info-value'>{evaluacion.NombreAdministrador}</div>
                </div>
                <div class='info-item'>
                    <div class='info-label'>ID Evaluación</div>
                    <div class='info-value'>#{evaluacion.IdEvaluacion}</div>
                </div>
            </div>
        </div>

        <div class='pie-pagina'>
            <p>Este documento ha sido generado automáticamente por el sistema DELTATEST.</p>
            <div class='timestamp'>Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</div>
        </div>
    </div>

    <div class='btn-group no-print'>
        <button class='btn btn-primary' onclick='window.print()'>Descargar/Imprimir</button>
        <button class='btn btn-secondary' onclick='window.history.back()'>Volver</button>
    </div>
</body>
</html>";

            return html;
        }

        private string ObtenerEstadoEvaluacion(decimal nota)
        {
            return nota >= 80 ? "APROBADO" : "DESAPROBADO";
        }
    }

    public class DetalleEvaluacionDto
    {
        public int IdEvaluacion { get; set; }
        public int IdEvaluado { get; set; }
        public string? NombreEvaluado { get; set; }
        public string? CiEvaluado { get; set; }
        public string? NombreAdministrador { get; set; }
        public DateOnly? FechaEvaluacion { get; set; }
        public decimal? Nota { get; set; }
        public string? EstadoEvaluacion { get; set; }
        public string? TipoEvaluacion { get; set; }
    }
}
