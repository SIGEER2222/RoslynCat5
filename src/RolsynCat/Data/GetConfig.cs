namespace RoslynCat.Data
{
	/// <summary>
	/// 获取配置信息
	/// </summary>
	public class GetConfig
	{
		private IConfigurationRoot _configuration;
		public GetConfig() {
			ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json");
			_configuration = configurationBuilder.Build();
		}

		public string GistId { get => _configuration["gist"]; }
		public string OpenAI { get => _configuration["OpneAI"]; }
		public string ConnectionString { get => _configuration.GetConnectionString("DbConnect"); }

	}
}
