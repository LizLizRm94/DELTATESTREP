using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace DELTATEST.Services
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        public AuthorizationMessageHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Configurar para enviar cookies con cada solicitud
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
