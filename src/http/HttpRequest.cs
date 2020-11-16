using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.misc;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.http
{
    class HttpRequest
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static HttpClient _httpClient;

        public HttpRequest()
        {
            if( _httpClient == null)
            {
                var config = ConfigReader.LoadConnectorConfig();
                var handler = new SocketsHttpHandler
                {
                    ConnectTimeout = TimeSpan.FromSeconds(3d),
                    UseProxy = config.UseProxy,
                    Proxy = new WebProxy(config.ProxyUrl, config.ProxyPort)
                };
                _httpClient = new HttpClient(handler);
            }
        }

        public async Task<string> SendRequestAsync(string url)
        {

            _log.Debug(Util.GetFormattedResource("HttpRequestMessage01", url));
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (Exception e)
            {
                _log.Error(Util.GetFormattedResource("HttpRequestMessage04"), e);
                throw;
            }
        }

        private void ConfigureProxy(WebRequest request, string proxyUrl, int port)
        {
            if (string.IsNullOrWhiteSpace(proxyUrl))
            {
                return;
            }
            _log.Debug(Util.GetFormattedResource("HttpRequestMessage03", proxyUrl, port));
            WebProxy proxy = new WebProxy(proxyUrl, port);
            request.Proxy = proxy;
        }
    }
}
