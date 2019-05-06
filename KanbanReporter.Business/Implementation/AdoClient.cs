using KanbanReporter.Business.Contracts;
using KanbanReporter.Business.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Implementation
{
    internal class AdoClient : AdoClientBase, IAdoClient
    {
        // 0=organization, 1=project, 2=repositoryName
        private const string ADO_FILES_PATH_TEMPLATE = "https://dev.azure.com/{0}/{1}/_apis/git/repositories/{2}/items?api-version=5.0&recursionLevel=oneLevel";

        // 0=organization, 1=project, 2=repositoryId
        private const string ADO_LIST_BRANCHES_TEMPLATE = "https://dev.azure.com/{0}/{1}/_apis/git/repositories/{2}/refs?api-version=5.0";

        // Required Settings
        private readonly string _adoOrgName;
        private readonly string _adoProjectName;
        private readonly string _adoQueryGuid;
        private readonly string _adoPersonalAccessToken;
        private readonly string _adoRepositoryId;
        private readonly string _repositoryName;
        private readonly string _markdownFilePath;
        private readonly string _adoBranchName;

        // External dependencies
        private readonly ILogger _log;

        public AdoClient(ISettings settings, ILogger log, IGuidValidator guidValidator) : base(settings, log)
        {
            _adoOrgName             = settings["AdoOrgName"            ];
            _adoProjectName         = settings["AdoProjectName"        ];
            _adoQueryGuid           = settings["AdoQueryGuid"          ];
            _adoPersonalAccessToken = settings["AdoPersonalAccessToken"];
            _adoRepositoryId        = settings["AdoRepositoryId"       ];
            _repositoryName         = settings["AdoRepositoryName"     ];
            _markdownFilePath       = settings["MarkdownFilePath"      ];
            _adoBranchName          = settings["AdoBranchName"         ];

            // All settings are required
            if (string.IsNullOrEmpty(_adoOrgName))             throw new InvalidProgramException("AdoOrgName was not set");
            if (string.IsNullOrEmpty(_adoProjectName))         throw new InvalidProgramException("AdoProjectName was not set");
            if (string.IsNullOrEmpty(_adoQueryGuid))           throw new InvalidProgramException("AdoQueryGuid was not set");
            if (string.IsNullOrEmpty(_adoPersonalAccessToken)) throw new InvalidProgramException("AdoPersonalAccessToken was not set");
            if (string.IsNullOrEmpty(_adoRepositoryId))        throw new InvalidProgramException("AdoRepositoryId was not set");
            if (string.IsNullOrEmpty(_repositoryName))         throw new InvalidProgramException("AdoRepositoryName was not set");
            if (string.IsNullOrEmpty(_markdownFilePath))       throw new InvalidProgramException("MarkdownFilePath was not set");
            if (string.IsNullOrEmpty(_adoBranchName))          throw new InvalidProgramException("The ADO Branch name was not set");

            // We can test GUID's for validity
            if (!guidValidator.IsValid(_adoQueryGuid))         throw new InvalidProgramException("Query GUID does not appear to be a valid GUID");
            if (!guidValidator.IsValid(_adoRepositoryId))      throw new InvalidProgramException("The Ado Repository ID does not appear to be a valid GUID");

            _log = log;
        }

        /// <summary>
        /// Use this command to push a new final report, replacing the current Markdown file
        /// </summary>
        /// <param name="finalReport">string contents of the new report to push to source control</param>
        /// <param name="readmefileDetails">Provides information about the git repository, necessary for the push</param>
        /// <returns></returns>
        public async Task<bool> CommitReportAndCreatePullRequestAsync(string finalReport, VersionedFileDetails readmefileDetails)
        {
            _log.Enter(this, args: readmefileDetails.path);

            // Get branch info
            var gitBranchReference = await GetBranchAsync();
            if(gitBranchReference == null)
            {
                _log.LogError($"Unable to find branch: '{_adoBranchName}'");
                return false;
            }

            // Create a commit for the finalReport
            var commit = BuildCommitForReport(finalReport, gitBranchReference);

            // Push the finalReport to our branch
            var commitResponse = await CommitToAdoAsync(commit);
            if(commitResponse == null)
            {
                _log.LogError("Unable to commit the report to ADO");
                return false;
            }

            // Create a pull request from the newly pushed document
            return await CreatePullRequestForBranch(gitBranchReference);
        }

        /// <summary>
        /// Retrieve commit details for the Readme file
        /// </summary>
        public async Task<VersionedFileDetails> GetVersionDetailsForReadmeFileAsync()
        {
            _log.Enter(this);

            var finalUrl = string.Format(ADO_FILES_PATH_TEMPLATE, _adoOrgName, _adoProjectName, _repositoryName);
            var uri = new Uri(finalUrl);

            var httpResponse = await ExecuteRestCallAsync(uri);
            if (!httpResponse.IsSuccessStatusCode)
            {
                _log.LogError("Unable to obtain results from query");
                return null;
            }
            var content = await httpResponse.Content.ReadAsStringAsync();
            var result  = JsonConvert.DeserializeObject<VersionedFileResult>(content);

            if (result == null || !result.value.Any())
                return null;

            return result.value.FirstOrDefault(i => i.path == _markdownFilePath);
        }

        /// <summary>
        /// Execute the query in ADO and transform the results into a list of CompleteWorkItems
        /// </summary>
        public async Task<List<CompleteWorkItem>> GetWorkItemsFromQueryAsync(AdoQuery adoQuery)
        {
            _log.Enter(this);
            
            var uri      = new Uri(adoQuery.Url + "?$top=1000&api-version=5.0");

            HttpResponseMessage httpResponse = await ExecuteRestCallAsync(uri);
            if (httpResponse.IsSuccessStatusCode)
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<QueryResult>(content);

                if (result != null && result.workItems.Any() && result.columns.Any())
                    return await ExtractAllWorkitemsAsync(result);
            }
            return new List<CompleteWorkItem>();
        }

        #region Private Helper Methods 





        private async Task<bool> CreatePullRequestForBranch(Value gitBranchReference)
        {
            _log.Enter(this, args: gitBranchReference.name);

            var pullRequestUri = new Uri($"https://dev.azure.com/{_adoOrgName}/_apis/git/repositories/{_adoRepositoryId}/pullrequests?api-version=5.0");
            
            var pullRequest = new PullRequest
            {
                title = "KanbanReporter updated the last Sprint Report (README.md)",
                description = "This is an automatically generated pull request",
                sourceRefName = gitBranchReference.name,
                targetRefName = "refs/heads/master"
            };

            var pullRequestJson = JsonConvert.SerializeObject(pullRequest);
            var pullRequestContent = new StringContent(pullRequestJson, Encoding.UTF8, "application/json");
            var pullRequestResponse = await HttpClient.PostAsync(pullRequestUri, pullRequestContent);

            // If the pull request already exists then that is perfectly fine and we will return success
            if (pullRequestResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                return true;

            return pullRequestResponse.IsSuccessStatusCode;
        }

        private async Task<CommitResponse> CommitToAdoAsync(GitCommit commit)
        {
            _log.Enter(this);

            var uri = new Uri($"https://dev.azure.com/{_adoOrgName}/_apis/git/repositories/{_adoRepositoryId}/pushes?api-version=5.0");
            var bodyAsJson = JsonConvert.SerializeObject(commit, Formatting.None);
            var httpContent = new StringContent(bodyAsJson, Encoding.UTF8, "application/json");

            var postReponse = await HttpClient.PostAsync(uri, httpContent);
            if (postReponse.IsSuccessStatusCode)
            {
                var rawContent = await postReponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CommitResponse>(rawContent);
            }
            return null;
        }

        private GitCommit BuildCommitForReport(string finalReport, Value gitBranchReference)
        {
            _log.Enter(this, args: gitBranchReference.name);

            return new GitCommit
            {
                refUpdates = new Refupdate[] { new Refupdate { name = gitBranchReference.name, oldObjectId = gitBranchReference.objectId, repositoryId = _adoRepositoryId } },
                commits = new Commit[] {
                    new Commit{
                        comment = $"Update Sprint report on {DateTime.Now.ToShortDateString()}",
                        changes = new Change[]{
                            new Change{
                                changeType = "edit",
                                item = new Item{
                                    path = _markdownFilePath
                                },
                                newContent = new Newcontent{
                                    content = finalReport,
                                    contentType = "rawtext"}
                            }
                        }
                    }
                }
            };
        }

        private async Task<Value> GetBranchAsync()
        {
            _log.Enter(this);

            var uri = new Uri(string.Format(ADO_LIST_BRANCHES_TEMPLATE, _adoOrgName, _adoProjectName, _adoRepositoryId));
            var httpResponse = await ExecuteRestCallAsync(uri);

            if (httpResponse.IsSuccessStatusCode)
            {
                var rawJson       = await httpResponse.Content.ReadAsStringAsync();
                var getRefsResult = JsonConvert.DeserializeObject<GetRefsResult>(rawJson);

                return getRefsResult.value.FirstOrDefault(r => r.name == _adoBranchName);
            }

            return await CreateBranchAsync();
        }

        private Task<Value> CreateBranchAsync()
        {
            throw new NotImplementedException();
        }

        private async Task<List<CompleteWorkItem>> ExtractAllWorkitemsAsync(QueryResult result)
        {
            _log.Enter(this);

            var destinationBag = new ConcurrentBag<CompleteWorkItem>();

            var allTasks = result.workItems.Select(async workItem => {
                var uri = new Uri(workItem.url);
                var httpResponse = await HttpClient.GetAsync(uri);
                if(httpResponse.IsSuccessStatusCode)
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    var completeWorkItem = JsonConvert.DeserializeObject<CompleteWorkItem>(content);
                    if (completeWorkItem != null)
                    {
                        await AddUpdatesToWorkItemAsync(completeWorkItem);
                        destinationBag.Add(completeWorkItem);
                    }
                }
            });
            await Task.WhenAll(allTasks);

            return destinationBag.ToList();
        }

        private async Task AddUpdatesToWorkItemAsync(CompleteWorkItem completeWorkItem)
        {
            _log.Enter(this, args: completeWorkItem.Id);

            var uri          = new Uri($"{completeWorkItem.Url}/updates");
            var httpResponse = await HttpClient.GetAsync(uri);
            if(httpResponse.IsSuccessStatusCode)
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                var updates = JsonConvert.DeserializeObject<WorkItemUpdate>(content);
                completeWorkItem.Updates = updates;
            }
        }

        #endregion
    }
}
