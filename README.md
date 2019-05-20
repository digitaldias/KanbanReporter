# KanbanReporter Solution Overview

KanbanReporter is an Azure Function originally designed to run every Sunday at 6AM to generate a report over delivered user stories, grouped by Sprints. The report is delivered in the form of a a `markdown` file. <br />
<br />
The project is written in C#, it follows the [DDD style](https://airbrake.io/blog/software-design/domain-driven-design) of architecture and uses [XUnit](https://xunit.github.io/) as it's test framework

## Purpose
The purpose of the tool is to provide the reader with an overview of what the system as a whole is capable of after each sprint, in addition to inform the reader about the *Lead Time* on each user story as well as the total number of user stories delivered in each sprint.

## Overall strategy
The program works by connecting to and utilizing the [Azure Devops REST Api](https://docs.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-5.0) (version 5.0). It relies on a pre-written query that will list all closed user stories, as well as a source control repository that it utilizes to commit the latest report. In short: 
- Executes the "get all closed user stories" query to retrieve a detailed list of all closed work items in the project
- Generates a markdown report in-memory which contains:
  - User stories grouped by Sprints
  - The number of items closed per sprint, as a sub-heading
  - The Lead Time for each user story 
- It commits the markdown file to source control under it's own branch
- If no pull request exist, one will be created for the commit to be pulled into master. 


## Maintenance
In order to ensure KanbanReporter is healthy, ensure the following: 

- Ensure the Personal Access Token is valid and not expired. These can only be set to a full year
- Monitor the Azure Function to verify that its alive. Verify that it actually ran every sunday at 6AM
- The pull request needs to be manually approved by someone other than the owner of the Personal Access Token

## Developer Notes

KanbanReporter relies on a set of settings in order to function. 

### KanbanReporter (as an Azure Function)
Create a `local.settings.json` file to give the program the necessary secrets during development runs.<br />
Once deployed onto Azure, the settings are configured as part of the Azure Function, and this file will not be required or used. 

### KanbanReporterCmd (as a command line tool)
In order to run KanbanReporter as a command line tool, the settings are provided as a file reference: 
<pre>
> KanbanReporter --input-file c:\settings\kanbanreporter.settings.json
</pre>

> DO NOT ADD `local.settings.json` TO SOURCE CONTROL <br />
> The file is already listed in the **.gitignore** file, and should only exist on the developer's computer 

Sample `local.settings.json` file contents:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage "  : "UseDevelopmentStorage=true",
    "AdoOrgName"            : "myOrgName",
    "AdoProjectName"        : "My VSO Project",    
    "AdoPersonalAccessToken": "------------------------------------",
    "AdoRepositoryName"     : "RepositoryName i.e. 'development'",
    "AdoBranchName"         : "refs/heads/KanbanReporter/Reports",
    "MarkdownFilePath"      : "/README.md",
    "CreatePullRequest"     : true 
  }
}
```
<br />

### Settings explained

| Setting | Purpose | 
| ------- | ------- |
| IsEncrypted | This is a construct of Azure Functions and needs to be there for the program to work |
| Values.AzureWebJobsStorage | Connection string to the Azure Storage that the Azure Function uses | 
| Values.AdoOrgName | The name of the Organization, used to construct the REST url | 
| Values.AdoProjectName | The name of the project within the organization for which the report will be generated | 
| Valies.AdoPersonalAccessToken | A [Personal Access Token](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops) with rights to read and execute queries, commit to source control, and create pull requests | 
| Values.AdoRepositoryName | Name of the repository that will contain the pushed report | 
| Values.AdoBranchName | The branch to which KanbanReporter will submit code and create pull requests for |
| Values.MarkdownFilePath | Complete path to the report filename within the relative source control tree | 
| Values.CreatePullRequest | (bool) Wether or not the KanbanReporter should create a pull request to master after committing the report or not | 

> **NOTES** <br />
> - The value of the Personal Access Token has been anonymized in this document <br />
