using KanbanReporter.Business.Contracts;
using KanbanReporter.Business.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Implementation
{
    internal class QueryGenerator : AdoClientBase, IQueryGenerator
    {                
        private const string LIST_QUERIES_TEMPLATE = "https://dev.azure.com/{0}/{1}/_apis/wit/queries?$depth=2&api-version=5.0";

        // External dependencies
        private readonly ILogger _log;
        private readonly ISettings _settings;

        // State
        private string _adoOrgName;
        private string _adoProjectName;
        private string _adoPersonalAccessToken;
        private string _sharedQueriesId;

        public QueryGenerator(ISettings settings, ILogger log, IGuidValidator guidValidator): base(settings, log)
        {
            _log                    = log;
            _settings               = settings;
            _adoOrgName             = settings["AdoOrgName"];
            _adoProjectName         = settings["AdoProjectName"];
            _adoPersonalAccessToken = settings["AdoPersonalAccessToken"];
        }

        public async Task<AdoQuery> GenerateKanbanReportQueryAsync(string queryName)
        {
            if (string.IsNullOrEmpty(_sharedQueriesId))
                return null;

            var postNewQueryUri = new Uri($"https://dev.azure.com/{_adoOrgName}/{_adoProjectName}/_apis/wit/queries/{_sharedQueriesId}?api-version=5.0");
            var query = new JObject {
                ["name"] = queryName,
                ["wiql"] = "Select System.ID, System.Title, [System.AssignedTo], [System.IterationPath], [System.CreatedDate], [Microsoft.VSTS.Common.ClosedDate] from WorkItems Where System.WorkItemType = 'User Story' and System.State = 'Closed' order by System.IterationPath asc"
            };
            
            var httpContent = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

            var httpResponse = await HttpClient.PostAsync(postNewQueryUri, httpContent);
            if (!httpResponse.IsSuccessStatusCode)
                return null;
            var rawText = await httpResponse.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<JObject>(rawText);

            return new AdoQuery{
                Id   = jObject["id"  ].Value<string>(),
                Name = jObject["name"].Value<string>(),
                Url  = jObject["_links"]["wiql" ]["href"].Value<string>(),
            };
        }

        public async Task<IEnumerable<AdoQuery>> LoadAllAsync()
        {
            var listQueriesUri = new Uri(string.Format(LIST_QUERIES_TEMPLATE, _adoOrgName, _adoProjectName));
            var responseMessage = await ExecuteRestCallAsync(listQueriesUri);

            if (!responseMessage.IsSuccessStatusCode)
                return null;

            var rawText  = await responseMessage.Content.ReadAsStringAsync();
            var jObjects = JsonConvert.DeserializeObject<JObject>(rawText);
            var queries  = (JArray)jObjects["value"];

            var match = queries.FirstOrDefault(obj => obj["name"].Value<string>() == "Shared Queries");
            if (match == null)
                return null;

            _sharedQueriesId = match["id"].Value<string>();

            var allSharedQueries = (JArray) match["children"];
            var result = allSharedQueries.Select(q => new AdoQuery {
                Name = q["name"].Value<string>(),
                Url  = q["url"].Value<string>(),
                Id   = q["id"].Value<string>()
            });

            return result;
        }
    }
}
