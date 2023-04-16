using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RoslynCat.Interface;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace RoslynCat.Controllers
{
    public class CodeSharing : IGistService
    {
        public Langauage Langauage { get; set; } = Langauage.CSharp;
        public string FileName { get; set; } = "UserCode.cs";
        public string Description { get; set; } = "Testing...";
        public string Code { get; set; }
        public string GistId { get; set; }
        private string url = "https://api.github.com";
        private IHttpClientFactory _httpClientFactory;

        public CodeSharing(IHttpClientFactory httpClientFactory) {
            _httpClientFactory = httpClientFactory;
        }
        /// <summary>
        /// 创建新的gist并设置值到GistId
        /// </summary>
        /// <param name="code">要上传的代码</param>
        /// <returns></returns>
        public async Task CreateGistAsync(string code) {
            if (code is null) return;

            GetConfig config = new GetConfig();
            string token = config.GistId;
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GistExample","1.0"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token",token);

            var createGistContent = new JObject
            {
                {"description", Description},
                {"public", true},
                {"files", new JObject {{ FileName, new JObject {{"content",code } }}}}
            };
            var createGistResponse = await httpClient.PostAsync(
                $"{url}/gists",
                new StringContent(createGistContent.ToString(), Encoding.UTF8, "application/json"));
            createGistResponse.EnsureSuccessStatusCode();

            var createdGist = JObject.Parse(await createGistResponse.Content.ReadAsStringAsync());
            var createdGistUrl = createdGist["html_url"].Value<string>();
            GistId = createdGistUrl.Split('/').Last();
        }

        /// <summary>
        /// 解析gist中的代码
        /// </summary>
        /// <param name="gistId"></param>
        /// <returns>如果能正确解析就返回gist中的代码，否则返回默认代码</returns>
        public async Task<string> GetGistContentAsync(string gistId) {
            if (gistId is null) return Constants.defultCode;

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var configuration = configurationBuilder.Build();
            string token = configuration["gist"];

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GistExample","1.0"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token",token);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var getGistResponse = await httpClient.GetAsync($"{url}/gists/{gistId}");

            stopwatch.Stop();
            Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");
            if (getGistResponse.IsSuccessStatusCode) {
                getGistResponse.EnsureSuccessStatusCode();
                var gist = JObject.Parse(await getGistResponse.Content.ReadAsStringAsync());
                var gistContent = gist["files"][FileName]["content"].Value<string>();
                return gistContent;
            }
            else { return Constants.defultCode; }
        }
    }
}
