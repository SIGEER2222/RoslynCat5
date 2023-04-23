namespace RoslynCat.Interface
{
	public interface IRequest
	{
		public string SourceCode { get; set; }
		public string Nuget { get; set; }
		public int Position { get; set; }
		public RequestType Type { get; set; }
		public List<string> Usings { get; }
	}
}
