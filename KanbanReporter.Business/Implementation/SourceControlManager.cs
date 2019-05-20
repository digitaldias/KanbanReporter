using KanbanReporter.Business.Contracts;
using KanbanReporter.Business.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Implementation
{
    internal class SourceControlManager : AdoClientBase, ISourceControlManager
    {
        public SourceControlManager(ISettings settings, ILogger log): base(settings, log) {
        }

        public async Task<Value> CommitReport(string finalReport, VersionedFileDetails readmefileDetails)
        {
            _log.Enter(this, args: readmefileDetails.path);

            // Get branch info
            var gitBranchReference = await GetOrCreateBranchAsync();
            if (gitBranchReference == null)
            {
                _log.LogError($"Unable to find branch: '{_settings["adoBranchName"]}'");
                return null;
            }

            // Create a commit for the finalReport
            var commit = BuildCommitForReport(finalReport, gitBranchReference);

            // Push the finalReport to our branch
            var commitResponse = await CommitToAdoAsync(commit);
            if (commitResponse == null)
            {
                _log.LogError("Unable to commit the report to ADO");
                return null;
            }
            return gitBranchReference;
        }

        public async Task<bool> CreatePullRequest(Value gitBranchReference)
        {
            _log.Enter(this, args: gitBranchReference.name);

            var pullRequestUri = new Uri($"https://dev.azure.com/{_adoOrgName}/_apis/git/repositories/{_adoRepositoryId}/pullrequests?api-version=5.0");

            var pullRequest = new PullRequest
            {
                title         = $"KanbanReporter updated the last Sprint Report ({gitBranchReference.name})",
                description   = "This is an automatically generated pull request",
                sourceRefName = gitBranchReference.name,
                targetRefName = "refs/heads/master"
            };

            var pullRequestJson     = JsonConvert.SerializeObject(pullRequest);
            var pullRequestContent  = new StringContent(pullRequestJson, Encoding.UTF8, "application/json");
            var pullRequestResponse = await HttpClient.PostAsync(pullRequestUri, pullRequestContent);

            // If the pull request already exists then that is perfectly fine and we will return success
            if (pullRequestResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                return true;

            return pullRequestResponse.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieve commit details for the Readme file
        /// </summary>
        public async Task<VersionedFileDetails> GetVersionDetailsForReadmeFileAsync()
        {        
            const string ADO_FILES_PATH_TEMPLATE = "https://dev.azure.com/{0}/{1}/_apis/git/repositories/{2}/items?api-version=5.0&recursionLevel=oneLevel";

            _log.Enter(this);

            var finalUrl = string.Format(ADO_FILES_PATH_TEMPLATE, _adoOrgName, _adoProjectName, _repositoryName);
            var uri = new Uri(finalUrl);

            var httpResponse = await GetAsync(uri);
            if (!httpResponse.IsSuccessStatusCode)
            {
                _log.LogError("Unable to obtain results from query");
                return null;
            }
            var content = await httpResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<VersionedFileResult>(content);

            if (result == null || !result.value.Any())
                return null;

            return result.value.FirstOrDefault(i => i.path == _markdownFilePath);
        }

        private async Task<string> GetLatestCommitToMaster(Value masterBranch)
        {
            const string LIST_COMMITS_TEMPLATE = "https://dev.azure.com/{0}/{1}/_apis/git/repositories/{2}/commits?searchCriteria.$top=1&api-version=5.0";
            var uri = new Uri(string.Format(LIST_COMMITS_TEMPLATE, _adoOrgName, _adoProjectName, _adoRepositoryId));

            var httpResult = await GetAsync(uri);
            if(httpResult.IsSuccessStatusCode)
            {
                var json = JObject.Parse(await httpResult.Content.ReadAsStringAsync());
                return json["value"][0]["commitId"].Value<string>();
            }
            return await Task.FromResult(string.Empty);
        }

        internal async Task<Value> GetOrCreateBranchAsync()
        {                    
            const string MASTER_BRANCH_NAME         = "refs/heads/master";
            const string ADO_LIST_BRANCHES_TEMPLATE = "https://dev.azure.com/{0}/{1}/_apis/git/repositories/{2}/refs?api-version=5.0";

            _log.Enter(this);

            var uri = new Uri(string.Format(ADO_LIST_BRANCHES_TEMPLATE, _adoOrgName, _adoProjectName, _adoRepositoryId));
            var httpResponse = await GetAsync(uri);

            if (httpResponse.IsSuccessStatusCode)
            {
                var rawJson = await httpResponse.Content.ReadAsStringAsync();
                var getRefsResult = JsonConvert.DeserializeObject<GetRefsResult>(rawJson);

                if (getRefsResult.value.Any(r => r.name == _adoBranchName))
                {
                    return getRefsResult.value.FirstOrDefault(r => r.name == _adoBranchName);
                }
                else if (getRefsResult.value.Any(r => r.name == MASTER_BRANCH_NAME))
                {
                    var masterBranch = getRefsResult.value.FirstOrDefault(r => r.name == MASTER_BRANCH_NAME);
                    return await CreateBranchAsync(masterBranch);
                }
            }
            return null;
        }

        internal async Task<Value> CreateBranchAsync(Value masterBranch)
        {
            _log.Enter(this);
            const string CREATE_BRANCH_TEMPLATE = "https://dev.azure.com/{0}/{1}/_apis/git/repositories/{2}/refs?api-version=5.0";

            var uri = new Uri(string.Format(CREATE_BRANCH_TEMPLATE, _adoOrgName, _adoProjectName, _adoRepositoryId));
            var latestCommitToMaster = await GetLatestCommitToMaster(masterBranch);

            var package = new
            {
                name = _adoBranchName,
                oldObjectId = "0000000000000000000000000000000000000000",
                newObjectId = latestCommitToMaster // This should be the last commit from master
            };
            var jArray = new JArray(JObject.FromObject(package));
            var stringContent = new StringContent(jArray.ToString(Formatting.None), Encoding.UTF8, "application/json");

            var httpPostResult = await PostAsync(uri, stringContent);
            if (httpPostResult.IsSuccessStatusCode)
            {
                var rawText = await httpPostResult.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(rawText);
                return new Value
                {
                    name = jObject["value"][0]["name"].Value<string>(),
                    objectId = jObject["value"][0]["repositoryId"].Value<string>()
                };
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

        private async Task<CommitResponse> CommitToAdoAsync(GitCommit commit)
        {
            _log.Enter(this);

            var uri = new Uri($"https://dev.azure.com/{_adoOrgName}/{_adoProjectName}/_apis/git/repositories/{_adoRepositoryId}/pushes?api-version=5.0");
            var bodyAsJson = JsonConvert.SerializeObject(commit, Formatting.None);
            var httpContent = new StringContent(bodyAsJson, Encoding.UTF8, "application/json");

            var postReponse = await PostAsync(uri, httpContent);
            if (postReponse.IsSuccessStatusCode)
            {
                var rawContent = await postReponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CommitResponse>(rawContent);
            }
            return null;
        }

        private string GenerateGuidOfLength(int length)
        {
            var builder = new StringBuilder();
            while (builder.Length < length)
            {
                builder.Append(Guid.NewGuid().ToString("N"));
            }
            return builder.ToString(0, length);
        }
    }
}
