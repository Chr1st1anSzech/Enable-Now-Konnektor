using Enable_Now_Konnektor_Bibliothek.src.config;
using Enable_Now_Konnektor_Bibliothek.src.misc;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.http
{
    internal class HttpRequest
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static HttpClient _httpClient;

        internal HttpRequest()
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

        internal async Task<string> SendRequestAsync(string url)
        {

            _log.Debug(LocalizationService.GetFormattedResource("HttpRequestMessage01", url));
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (TaskCanceledException timeoutException)
            {
                _log.Error(LocalizationService.GetFormattedResource("HttpRequestMessage02"), timeoutException);
                throw;
            }
            catch (Exception e)
            {
                _log.Error(LocalizationService.GetFormattedResource("HttpRequestMessage03"), e);
                throw;
            }
        }
    }
}
