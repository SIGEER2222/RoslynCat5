using Newtonsoft.Json.Linq;
using RoslynCat.Interface;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace RoslynCat.Controllers
{
	public class CodeSharing : IGistService
	{
		public Langauage Langauage { get; set; } = Langauage.CSharp;
		public string FileName { get; set; } = "UserCode.cs";
		public string Description { get; set; } = "Testing...";
		public string Code { get; set; }
		public string GistId { get; set; }
		private HttpClient _githubClient;

		public CodeSharing(IHttpClientFactory httpClientFactory) {
			_githubClient = httpClientFactory.CreateClient("GithubApi");
			GetConfig config = new GetConfig();
			string token = config.GistId;
			_githubClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GistExample","1.0"));
			_githubClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token",token);
		}
		/// <summary>
		/// 创建新的gist并设置值到GistId
		/// </summary>
		/// <param name="code">要上传的代码</param>
		/// <returns></returns>
		public async Task CreateGistAsync(string code) {
			if (code is null) return;
			var createGistContent = new JObject
			{
				{"description", Description},
				{"public", true},
				{"files", new JObject {{ FileName, new JObject {{"content",code } }}}}
			};
			//var createGistResponse = await _githubClient.PostAsJsonAsync("/gists", createGistContent);
			var response = await _githubClient.PostAsync("/gists", new StringContent(createGistContent.ToString()));
			var result = await response.Content.ReadFromJsonAsync<JsonObject>();
			GistId = result["id"].AsValue().ToString();
		}

		/// <summary>
		/// 解析gist中的代码
		/// </summary>
		/// <param name="gistId"></param>
		/// <returns>如果能正确解析就返回gist中的代码，否则返回默认代码</returns>
		public async Task<string> GetGistContentAsync(string gistId) {
			if (gistId is null) return Constants.defultCode;

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var getGistResponse = await _githubClient.GetAsync($"/gists/{gistId}");

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
