using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynCat.Interface;

namespace RoslynCat.Data
{
	public class SourceInfo : IRequest
	{
		public SourceInfo() {
		}
		public SourceInfo(string code,string nuget,int position) {
			SourceCode = code;
			Nuget = nuget;
			Position = position;
		}

		public string SourceCode { get; set; }
		public string Nuget { get; set; }
		public int Position { get; set; }
		public RequestType Type { get; set; }

		public List<string> Usings {
			get {
				SyntaxNode root = CSharpSyntaxTree.ParseText(SourceCode).GetRoot();
				List<string> usings =  root.DescendantNodes().OfType<UsingDirectiveSyntax>().Select(x => x.Name.ToString()).ToList();
				return usings;
			}
		}
		public int LineNumberOffsetFromTemplate { get; set; }
		internal int CalculateVisibleLineNumber(int compilerLineError) => compilerLineError - LineNumberOffsetFromTemplate;
		public enum RequestType
		{
			Complete,
			Signature,
			Hover,
			CodeCheck,
			None
		}
	}
}
