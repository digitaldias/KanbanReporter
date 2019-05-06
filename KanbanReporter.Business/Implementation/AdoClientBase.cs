using KanbanReporter.Business.Contracts;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Implementation
{
    internal class AdoClientBase
    {
        // State
        private static HttpClient _httpClient;

        private  string           _adoPersonalAccessToken;

        // External dependencies
        private readonly ISettings  _settings;
        private readonly ILogger    _log;

        public AdoClientBase(ISettings settings, ILogger log)
        {
            _settings = settings;
            _log = log;

            if(string.IsNullOrEmpty(_adoPersonalAccessToken))
            {
                _adoPersonalAccessToken = settings["AdoPersonalAccessToken"];
            }
        }

        protected HttpClient HttpClient
        {
            get
            {
                return _httpClient;
            }
        }

        protected async Task<HttpResponseMessage> ExecuteRestCallAsync(Uri uri)
        {
            _log.Enter(this, args: uri.ToString());

            InitializeHttpClientWithPersonalAccessToken();

            return await HttpClient.GetAsync(uri);
        }

        private void InitializeHttpClientWithPersonalAccessToken()
        {
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
                var byteArray = Encoding.ASCII.GetBytes($"username:{_adoPersonalAccessToken}");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));            
            }
        }
    }
}
