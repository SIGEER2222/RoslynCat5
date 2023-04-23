using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynCat.Interface;

namespace RoslynCat.Roslyn
{
	public class CompletionProvider
	{
		private SemanticModel semanticModel;
		private int position;
		private RequestType type;
		private EmitResult _emitResult;

		private readonly ICompleteProvider  _completeProvider;
		//private readonly ISignatureProvider _signatureProvider;
		private readonly IHoverProvider     _hoverProvider;
		private readonly ICodeCheckProvider _codeCheckProvider;

		private readonly IWorkSpaceService _workSpace;

		public CompletionProvider(IWorkSpaceService workSpace,ICompleteProvider completeProvider,IHoverProvider hoverProvider,ICodeCheckProvider codeCheckProvider) {
			_workSpace = workSpace;
			_completeProvider = completeProvider;
			//_signatureProvider = signatureProvider;
			_hoverProvider = hoverProvider;
			_codeCheckProvider = codeCheckProvider;
			_emitResult = _workSpace.GetEmitResultAsync().Result;
		}

		/// <summary>
		/// 异步创建提供程序的方法，接受一个 SourceInfo 参数。
		/// </summary>
		/// <param name="sourceInfo">SourceInfo 对象，包含源代码信息和位置信息。</param>
		public async Task CreateProviderAsync(SourceInfo sourceInfo) {
			_workSpace.OnDocumentChange(sourceInfo.SourceCode);
			_emitResult = await _workSpace.GetEmitResultAsync();
			semanticModel = await _workSpace.GetSmanticModelAsync();
			position = sourceInfo.Position;
			type = sourceInfo.Type;
		}

		/// <summary>
		/// 异步获取结果的方法，根据 type 属性的值，返回不同类型的响应。
		/// </summary>
		/// <returns>一个实现了 IResponse 接口的对象。</returns>
		public async Task<IResponse> GetResultAsync() => type switch {
			RequestType.Complete => await _completeProvider.Provide(_workSpace.Document,position),
			//(RequestType.Signature) => await _signatureProvider.Provide(document,position,semanticModel),
			RequestType.Hover => await _hoverProvider.Provide(_workSpace.Document,position,semanticModel),
			RequestType.CodeCheck => await _codeCheckProvider.Provide(_emitResult,_workSpace.Document),
			RequestType.None => null
		};

		/// <summary>
		/// 异步格式化代码的方法，接受一个字符串参数 code。
		/// </summary>
		/// <param name="code">要格式化的代码。</param>
		/// <returns>格式化后的代码字符串。</returns>
		public async Task<string> FormatCode(string code) {
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
			CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
			SyntaxNode formattedNode = root.NormalizeWhitespace();
			return formattedNode.ToFullString();
		}


		/// <summary>
		/// 运行代码的方法，接受两个字符串参数，一个是代码，另一个是读取（可选，默认为空字符串）。
		/// </summary>
		/// <param name="code">要运行的代码</param>
		/// <param name="read">可选参数，用于标准输入的字符串</param>
		/// <returns>一个 Task，该任务解析为一个字符串。</returns>
		public async Task<string> RunCode(string code,string read = "") {
			string res = string.Empty;
			_workSpace.OnDocumentChange(code);
			var syntaxTree = await _workSpace.Document.GetSyntaxTreeAsync();

			if (_emitResult.Success is not true) {
				res = string.Join(Environment.NewLine,_emitResult.Diagnostics
					   .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
					   .Select(diagnostic => $"{syntaxTree.GetLineSpan(diagnostic.Location.SourceSpan).StartLinePosition.Line + 1} : {diagnostic.Id}, {diagnostic.GetMessage()}"));
			}

			else {
				var compilation = CreateCompilation(syntaxTree);
				using MemoryStream ms = new MemoryStream();
				//var compilation = await _workSpace.Project.GetCompilationAsync();
				compilation.Emit(ms);
				ms.Seek(0,SeekOrigin.Begin);
				var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(ms);
				var entryPoint = assembly.EntryPoint;
				var args = new string[] { };

				StringWriter writer = new StringWriter();
				var stdout = Console.Out;
				Console.SetOut(writer);
				Console.SetIn(new StringReader(read));

				var para = entryPoint.GetParameters();
				_ = para.Length > 0 ? entryPoint?.Invoke(null,new object[] { args }) : entryPoint?.Invoke(null,null);

				res = writer.ToString();
				writer.Close();
				Console.SetOut(stdout);
			}
			return res;
		}

		/// <summary>
		/// 创建C#编译器
		/// 不知道为什么_workSpace.Project.GetCompilationAsync()会有问题
		/// </summary>
		/// <param name="syntaxTree"></param>
		/// <returns></returns>
		private CSharpCompilation CreateCompilation(SyntaxTree syntaxTree) => CSharpCompilation.Create(
			Path.GetRandomFileName(),
			syntaxTrees: new[] { syntaxTree },
			references: Constants.DefaultMetadataReferences,
			options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));
	}
}
