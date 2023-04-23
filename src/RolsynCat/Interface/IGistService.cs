namespace RoslynCat.Interface
{
	public interface IGistService
	{
		public Langauage Langauage { get; set; }
		public string FileName { get; set; }
		public string Description { get; set; }
		public string Code { get; set; }
		public string GistId { get; set; }
		public Task CreateGistAsync(string code);
		public Task<string> GetGistContentAsync(string gistId);
	}

	public enum Langauage
	{
		CSharp,
		C,
		Java,
	}
}
