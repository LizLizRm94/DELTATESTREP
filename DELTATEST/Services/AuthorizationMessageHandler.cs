using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DELTATEST.Services
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        public AuthorizationMessageHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Con autenticación por cookies, la cookie se envía automáticamente con cada solicitud
            // Este handler ahora es principalmente un pass-through
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
