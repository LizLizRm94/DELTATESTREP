using System.Net.Http.Json;
using DELTATEST.Models;

namespace DELTATEST.Services
{
    public class PreguntaService
    {
        private readonly HttpClient _http;

        public PreguntaService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Obtiene todas las preguntas de una evaluación específica
        /// </summary>
        public async Task<List<PreguntaDto>?> ObtenerPreguntasPorEvaluacion(int idEvaluacion)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<PreguntaDto>>($"api/preguntas/evaluacion/{idEvaluacion}");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene todas las respuestas de una evaluación específica
        /// </summary>
        public async Task<List<RespuestaDto>?> ObtenerRespuestasPorEvaluacion(int idEvaluacion)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<RespuestaDto>>($"api/respuestas/evaluacion/{idEvaluacion}");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene todas las respuestas de un usuario
        /// </summary>
        public async Task<List<RespuestaDto>?> ObtenerRespuestasPorUsuario(int idUsuario)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<RespuestaDto>>($"api/respuestas/usuario/{idUsuario}");
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class PreguntaDto
    {
        public int IdPregunta { get; set; }
        public string Texto { get; set; } = string.Empty;
        public bool TipoEvaluacion { get; set; }
        public string? Opciones { get; set; }
        public int? RespuestaCorrectaIndex { get; set; }
        public int Puntos { get; set; }
    }

    public class RespuestaDto
    {
        public int IdRespuesta { get; set; }
        public int IdPregunta { get; set; }
        public int IdUsuario { get; set; }
        public int IdEvaluacion { get; set; }
        public string TextoRespuesta { get; set; } = string.Empty;
    }
}
