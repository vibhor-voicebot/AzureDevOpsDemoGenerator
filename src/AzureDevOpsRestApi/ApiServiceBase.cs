using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace AzureDevOpsAPI
{
    public abstract class ApiServiceBase
    {
        public string LastFailureMessage { get; set; }
        protected readonly IAppConfiguration Configuration;
        protected readonly string Credentials;
        protected readonly string Project;
        protected readonly string ProjectId;
        protected readonly string Account;
        protected readonly string Team;
        protected readonly string BaseAddress;
        protected readonly string MediaType;
        protected readonly string Scheme;
        protected readonly string GitCredential;
        protected readonly string UserName;
        Logger logger = LogManager.GetLogger("*");

        public ApiServiceBase(IAppConfiguration configuration)
        {
            Configuration = configuration;
            logger.Info("Configuration #######################" + Configuration);
            Credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", Configuration.PersonalAccessToken)));//configuration.PersonalAccessToken;
            Project = configuration.Project;
            logger.Info("ConfigurationProject #######################" + Project);
            Account = configuration.AccountName;
            logger.Info("ConfigurationAccount #######################" + Account);
            Team = configuration.Team;
            logger.Info("ConfigurationTeam #######################" + Team);
            ProjectId = configuration.ProjectId;
            logger.Info("ConfigurationProjectId #######################" + ProjectId);

            BaseAddress = configuration.GitBaseAddress;
            logger.Info("ConfigurationBaseAddress #######################" + BaseAddress);
            MediaType = configuration.MediaType;
            Scheme = configuration.Scheme;
            GitCredential = configuration.GitCredential;
            UserName = configuration.UserName;
            logger.Info("ConfigurationUserName #######################" + UserName);
        }

        protected HttpClient GetHttpClient()
        {
            logger.Info("Configuration.UriString ++++++++++++++################################" + Configuration.UriString + "---> " + new Uri("https://dev.azure.com/" + Configuration.UriString));
            //logger.Info("Configuration.UriString ++++++++++++++################################" + Configuration.UriString);
            //logger.Info("Configuration ++++++++++++++################################" + Configuration);
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://dev.azure.com/"+Configuration.UriString)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Credentials);

            return client;
        }
        protected HttpClient GitHubHttpClient()
        {
            logger.Info("GitHubHttpClient BaseAddress ++++++++++++++################################" + BaseAddress);
            var client = new HttpClient
            {
                BaseAddress = new Uri(BaseAddress)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Scheme, GitCredential);
            return client;
        }
    }
}
