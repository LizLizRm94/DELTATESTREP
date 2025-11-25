using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DELTATEST.Models;

namespace DELTATEST.Services
{
    public class EvaluadoService
    {
        private readonly HttpClient _http;

        public EvaluadoService(HttpClient http)
        {
            _http = http;
        }

        public async Task<EvaluadoEstado?> ObtenerEstadoAsync(int idUsuario)
        {
            return await _http.GetFromJsonAsync<EvaluadoEstado>($"api/evaluados/estado/{idUsuario}");
        }
    }
}
