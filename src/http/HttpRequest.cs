using Enable_Now_Konnektor.src.misc;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.http
{
    class HttpRequest
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<string> SendRequestAsync(string url, string proxyUrl = null, int port = 0)
        {
            try
            {
                _log.Info( Util.GetFormattedResource("HttpRequestMessage01", url) );
                var request = WebRequest.Create(url);
                ConfigureProxy(request, proxyUrl, port);
                using var response = await request.GetResponseAsync();
                using StreamReader reader = new StreamReader(response.GetResponseStream());
                return (await reader.ReadToEndAsync());
            }
            catch (Exception e)
            {
                _log.Error(Util.GetFormattedResource("HttpRequestMessage02", url), e);
                throw;
            }
            
        }

        private static void ConfigureProxy(WebRequest request, string proxyUrl, int port)
        {
            if (string.IsNullOrWhiteSpace(proxyUrl))
            {
                return;
            }
            _log.Debug( Util.GetFormattedResource("HttpRequestMessage03", proxyUrl, port) );
            WebProxy proxy = new WebProxy(proxyUrl, port);
            request.Proxy = proxy;
        }
    }
}
