using System.ComponentModel;
using System.Data;
using System.Xml;

namespace RoslynCat.Data
{
	public static class Constants
	{
		public static readonly string csharpId = "editorId";
		public static readonly string resultId = "resultId";
		public static readonly string gistId = "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";
		public static readonly string defultCode = @"using System;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(""欢迎使用RoslynCat"");
    }
}";
		public static MetadataReference[] DefaultMetadataReferences = new MetadataReference[]
		{
			MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
			MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
			MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(int).Assembly.Location),
			MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
			MetadataReference.CreateFromFile(typeof(DescriptionAttribute).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Dictionary<,>).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(DataSet).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(XmlDocument).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location)
		};
	}
	public class Dialogue
	{
		public string ask { get; set; } = string.Empty;
		public string message { get; set; } = string.Empty;
	}
}
