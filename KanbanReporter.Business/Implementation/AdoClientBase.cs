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

        protected string _adoOrgName;
        protected string _adoProjectName;
        protected string _adoPersonalAccessToken;
        protected string _adoRepositoryId;
        protected string _repositoryName;
        protected string _adoBranchName;
        protected string _adoQueryGuid;
        protected string _markdownFilePath;

        // External dependencies
        protected readonly ISettings  _settings;
        protected readonly ILogger    _log;

        public AdoClientBase(ISettings settings, ILogger log)
        {
            _settings = settings;
            _log = log;

            _adoOrgName             = settings["AdoOrgName"];
            _adoProjectName         = settings["AdoProjectName"];
            _adoPersonalAccessToken = settings["AdoPersonalAccessToken"];
            _adoRepositoryId        = settings["AdoRepositoryId"];
            _repositoryName         = settings["AdoRepositoryName"];
            _adoBranchName          = settings["AdoBranchName"];
            _adoQueryGuid           = settings["AdoQueryGuid"];
            _repositoryName         = settings["AdoRepositoryName"];
            _markdownFilePath       = settings["MarkdownFilePath"];

            // All settings are required
            if (string.IsNullOrEmpty(_adoOrgName))             throw new InvalidProgramException("AdoOrgName was not set");
            if (string.IsNullOrEmpty(_adoProjectName))         throw new InvalidProgramException("AdoProjectName was not set");
            if (string.IsNullOrEmpty(_adoQueryGuid))           throw new InvalidProgramException("AdoQueryGuid was not set");
            if (string.IsNullOrEmpty(_adoPersonalAccessToken)) throw new InvalidProgramException("AdoPersonalAccessToken was not set");
            if (string.IsNullOrEmpty(_adoRepositoryId))        throw new InvalidProgramException("AdoRepositoryId was not set");
            if (string.IsNullOrEmpty(_repositoryName))         throw new InvalidProgramException("AdoRepositoryName was not set");
            if (string.IsNullOrEmpty(_markdownFilePath))       throw new InvalidProgramException("MarkdownFilePath was not set");
            if (string.IsNullOrEmpty(_adoBranchName))          throw new InvalidProgramException("The ADO Branch name was not set");
        }

        protected HttpClient HttpClient
        {
            get
            {
                return _httpClient;
            }
        }

        protected async Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            _log.Enter(this, args: uri.ToString());

            InitializeHttpClientWithPersonalAccessToken();

            var result = await HttpClient.GetAsync(uri);
            if (!result.IsSuccessStatusCode)
            {
                _log.LogError($"Get failed: {result.ReasonPhrase} (url: {uri.ToString()}");
            }
            return result;

        }

        protected async Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content)
        {
            _log.Enter(this);

            InitializeHttpClientWithPersonalAccessToken();
            var result = await _httpClient.PostAsync(uri, content);
            if(!result.IsSuccessStatusCode)
            {
                _log.LogError($"Post failed: {result.ReasonPhrase} (url: {uri.ToString()}");
            }
            return result;
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
