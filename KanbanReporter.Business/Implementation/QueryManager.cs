using KanbanReporter.Business.Contracts;
using KanbanReporter.Business.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Implementation
{
    internal class QueryManager : AdoClientBase, IQueryManager
    {
        private const string LIST_QUERIES_TEMPLATE = "https://dev.azure.com/{0}/{1}/_apis/wit/queries?$depth=2&api-version=5.0";

        // State
        private string _sharedQueriesId;

        public QueryManager(ISettings settings, ILogger log, IGuidValidator guidValidator) : base(settings, log)
        {
        }

        public async Task<AdoQuery> GenerateKanbanReportQueryAsync(string queryName)
        {
            _log.Enter(this, args: queryName);

            if (string.IsNullOrEmpty(_sharedQueriesId))
                return null;

            var postNewQueryUri = new Uri($"https://dev.azure.com/{_adoOrgName}/{_adoProjectName}/_apis/wit/queries/{_sharedQueriesId}?api-version=5.0");
            var query = new JObject
            {
                ["name"] = queryName,
                ["wiql"] = $"Select System.ID, System.Title, [System.AssignedTo], [System.IterationPath], [System.CreatedDate], [Microsoft.VSTS.Common.ClosedDate] from WorkItems Where [System.TeamProject] = '{_adoProjectName}' and System.WorkItemType = 'User Story' and System.State = 'Closed' order by System.IterationPath asc"
            };

            var httpContent = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

            var httpResponse = await HttpClient.PostAsync(postNewQueryUri, httpContent);
            if (!httpResponse.IsSuccessStatusCode)
                return null;
            var rawText = await httpResponse.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<JObject>(rawText);

            return new AdoQuery
            {
                Id = jObject["id"].Value<string>(),
                Name = jObject["name"].Value<string>(),
                Url = jObject["_links"]["wiql"]["href"].Value<string>(),
            };
        }

        /// <summary>
        /// Execute the query in ADO and transform the results into a list of CompleteWorkItems
        /// </summary>
        public async Task<List<CompleteWorkItem>> GetWorkItemsFromQueryAsync(AdoQuery adoQuery)
        {
            _log.Enter(this, args: adoQuery.Name);

            var wiqlUri = await GetWiqlFromQueryAsync(adoQuery);

            var uri = new Uri(wiqlUri + "?$top=1000&api-version=5.0");

            HttpResponseMessage httpResponse = await GetAsync(uri);
            if (httpResponse.IsSuccessStatusCode)
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<QueryResult>(content);

                if (result != null && result.workItems.Any() && result.columns.Any())
                    return await ExtractAllWorkitemsAsync(result);
            }
            return new List<CompleteWorkItem>();
        }

        public async Task<IEnumerable<AdoQuery>> LoadAllAsync()
        {
            var listQueriesUri = new Uri(string.Format(LIST_QUERIES_TEMPLATE, _adoOrgName, _adoProjectName));
            var responseMessage = await GetAsync(listQueriesUri);

            if (!responseMessage.IsSuccessStatusCode)
                return null;

            var rawText = await responseMessage.Content.ReadAsStringAsync();
            var jObjects = JsonConvert.DeserializeObject<JObject>(rawText);
            var queries = (JArray)jObjects["value"];

            var match = queries.FirstOrDefault(obj => obj["name"].Value<string>() == "Shared Queries");
            if (match == null)
                return null;

            _sharedQueriesId = match["id"].Value<string>();
            if (match["hasChildren"].Value<bool>() == false)
                return new List<AdoQuery>();

            var allSharedQueries = (JArray)match["children"];
            var result = allSharedQueries.Select(q => new AdoQuery
            {
                Name = q["name"].Value<string>(),
                Url = q["url"].Value<string>(),
                Id = q["id"].Value<string>()
            });

            return result;
        }

        private async Task<List<CompleteWorkItem>> ExtractAllWorkitemsAsync(QueryResult result)
        {
            _log.Enter(this);

            var destinationBag = new ConcurrentBag<CompleteWorkItem>();

            // Parallel.ForEach(result.workItems, async workItem =>
            foreach(var workItem in result.workItems)
            {
                var uri = new Uri(workItem.url);
                var httpResponse = await GetAsync(uri);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    var completeWorkItem = JsonConvert.DeserializeObject<CompleteWorkItem>(content);
                    if (completeWorkItem != null)
                    {
                        await AddUpdatesToWorkItemAsync(completeWorkItem);
                        destinationBag.Add(completeWorkItem);
                    }
                }
            };
            return await Task.FromResult(destinationBag.ToList());
        }

        private async Task AddUpdatesToWorkItemAsync(CompleteWorkItem completeWorkItem)
        {
            _log.Enter(this, args: completeWorkItem.Id);

            var uri = new Uri($"{completeWorkItem.Url}/updates");
            var httpResponse = await HttpClient.GetAsync(uri);
            if (httpResponse.IsSuccessStatusCode)
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                var updates = JsonConvert.DeserializeObject<WorkItemUpdate>(content);
                completeWorkItem.Updates = updates;
            }
        }

        private async Task<string> GetWiqlFromQueryAsync(AdoQuery adoQuery)
        {
            _log.Enter(this, args: adoQuery.Name);

            var uri = new Uri(adoQuery.Url + "?$top=1000&api-version=5.0");

            HttpResponseMessage httpResponse = await GetAsync(uri);
            if (httpResponse.IsSuccessStatusCode)
            {
                var rawText = await httpResponse.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(rawText);

                return jObject["_links"]["wiql"]["href"].Value<string>();
            }
            return string.Empty;
        }
    }
}
