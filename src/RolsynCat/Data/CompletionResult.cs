using RoslynCat.Interface;
using System.Collections.Concurrent;

namespace RoslynCat.Data
{
	public class CompletionResult : IResponse
	{
		//private static readonly CompletionResult instance = new CompletionResult();

		//public static CompletionResult GetInstance() {
		//    return instance;
		//}
		public CompletionResult() { }

		public ConcurrentDictionary<string,string> Suggestions { get; set; }
	}
	public class HoverInfoResult : IResponse
	{
		public HoverInfoResult() { }

		public virtual string Information { get; set; }

		public virtual int OffsetFrom { get; set; }

		public virtual int OffsetTo { get; set; }
	}

	public class CodeCheckResult : IResponse
	{
		public CodeCheckResult() { }

		public List<CodeCheck> codeChecks { get; set; } = new List<CodeCheck>();

		public class CodeCheck
		{
			public virtual string Id { get; set; }

			public virtual string Keyword { get; set; }

			public virtual string Message { get; set; }

			public virtual int OffsetFrom { get; set; }

			public virtual int OffsetTo { get; set; }

			public virtual CodeCheckSeverity Severity { get; set; }

			public virtual int SeverityNumeric { get; set; }
		}


	}
	public class SignatureHelpResult : IResponse
	{
		public SignatureHelpResult() { }

		public virtual Signatures[] Signatures { get; set; }
		public virtual int ActiveParameter { get; set; }
		public virtual int ActiveSignature { get; set; }
	}

	public class Signatures
	{
		public virtual string Label { get; set; }

		public virtual string Documentation { get; set; }

		public virtual Parameter[] Parameters { get; set; }
	}

	public class Parameter
	{
		public virtual string Label { get; set; }

		public virtual string Documentation { get; set; }
	}

	public enum CodeCheckSeverity
	{
		Unkown = 0,
		Hint = 1,
		Info = 2,
		Warning = 4,
		Error = 8
	}
}
