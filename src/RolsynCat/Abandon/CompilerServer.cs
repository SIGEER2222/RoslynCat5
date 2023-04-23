using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.Text;
using Microsoft.JSInterop;
using System.Web;

namespace RoslynCat.Abandon
{
	public class Compiler
	{

		AdhocWorkspace workspace;
		string code;
		Document document;
		Project project;

		private CSharpCompilation compilation;

		public Compiler() {

		}

		public async Task CreatCompilation(string sourcecode) {
			code = sourcecode;
			// 创建一个 AdhocWorkspace 对象
			var workspace = new AdhocWorkspace();

			// 创建一个项目信息对象
			var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(),
				"MyProject", "MyAssembly.dll", LanguageNames.CSharp);

			// 将项目信息对象添加到工作区中
			workspace.AddProject(projectInfo);

			// 创建一个元数据引用对象，用于引用所需的程序集
			var mscorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

			// 将元数据引用对象添加到项目中
			var project = workspace.CurrentSolution.GetProject(projectInfo.Id).AddMetadataReference(mscorlibReference);

			// 创建一个文档信息对象，用于表示编译的代码
			var documentInfo = DocumentInfo.Create(
			DocumentId.CreateNewId(project.Id),
			"MyFile.cs",
			loader: TextLoader.From(TextAndVersion.Create(SourceText.From(code), VersionStamp.Create())));

			// 将文档信息对象添加到工作区中
			workspace.AddDocument(documentInfo);

			// 编译代码并获取结果
			var compilation = project.GetCompilationAsync().Result;
			var result = compilation.Emit(Path.Combine(Directory.GetCurrentDirectory(), "MyAssembly.dll"));

			// 输出编译结果
			if (result.Success) {
				Console.WriteLine("Compilation succeeded");
			}
			else {
				Console.WriteLine("Compilation failed");
				foreach (var diagnostic in result.Diagnostics) {
					Console.WriteLine(diagnostic.ToString());
				}
			}
		}

		/// <summary>
		/// 运行C#代码并返回结果
		/// </summary>
		/// <param name="code"></param>
		/// <param name="read"></param>
		/// <returns></returns>
		public string CompileAndRun(string code,string read = "") {
			string res = string.Empty;
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
			string assemblyName = Path.GetRandomFileName();
			MetadataReference[] references = Constants.DefaultMetadataReferences;

			CSharpCompilation compilation = CSharpCompilation.Create(
				assemblyName,
				syntaxTrees: new[] { syntaxTree },
				references: references,
				options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

			using (MemoryStream ms = new MemoryStream()) {
				EmitResult emitresult = compilation.Emit(ms);
				if (!emitresult.Success) {
					IEnumerable<Diagnostic> failures = emitresult.Diagnostics
						.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

					foreach (Diagnostic diagnostic in failures) {
						var span = syntaxTree.GetLineSpan(diagnostic.Location.SourceSpan);
						int line = span.StartLinePosition.Line + 1;
						res += $"{line} : {diagnostic.Id}, {diagnostic.GetMessage()}" + Environment.NewLine;
					}
				}
				else {
					ms.Seek(0,SeekOrigin.Begin);
					var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(ms);
					var entryPoint = assembly.EntryPoint;

					StringWriter writer = new StringWriter();
					var stdout = Console.Out; // 保存标准输出流
					Console.SetOut(writer); // 将输出流更改为文本写入器

					Console.SetIn(new StringReader(read));

					entryPoint?.Invoke(null,new object[] { new string[] { } });
					res = writer.ToString();
					writer.Close(); // 关闭文本写入器
					Console.SetOut(stdout); // 将输出流还原为标准输出流
				}
				return res;
			}
		}
	}


	public class CompilerServer
	{
		private ElementReference inputElem;
		public bool Disabled { get; set; } = true;
		public string Output { get; set; } = "";
		public string Input { get; set; } = "";
		private CSharpCompilation _previousCompilation;
		private IEnumerable<MetadataReference> _references;
		private object[] _submissionStates = new object[] { null, null };
		private int _submissionIndex = 0;
		private List<string> _history = new List<string>();
		private int _historyIndex = 0;

		[Inject] private IJSRuntime _JSRuntime { get; set; }
		public async Task RunSubmisson(string code) {
			Output += $@"<br /><span class=""info"">{HttpUtility.HtmlEncode(code)}</span>";
			var previousOut = Console.Out;
			bool successComplite = TryCompile(code, out var script, out var errorDiagnostics);
			if (successComplite) {
				var writer = new StringWriter();
				Console.SetOut(writer);

				var entryPoint = _previousCompilation.GetEntryPoint(CancellationToken.None);
				var type = script.GetType($"{entryPoint.ContainingNamespace.MetadataName}.{entryPoint.ContainingType.MetadataName}");
				var entryPointMethod = type.GetMethod(entryPoint.MetadataName);

				var submission = (Func<object[], Task>)entryPointMethod.CreateDelegate(typeof(Func<object[], Task>));

				if (_submissionIndex >= _submissionStates.Length) {
					Array.Resize(ref _submissionStates,Math.Max(_submissionIndex,_submissionStates.Length * 2));
				}

				var returnValue = await (Task<object>)submission(_submissionStates);
				//if (returnValue != null) {
				//    Console.WriteLine(CSharpObjectFormatter.Instance.FormatObject(returnValue));
				//}

				var output = HttpUtility.HtmlEncode(writer.ToString());
				if (!string.IsNullOrWhiteSpace(output)) {
					Output += $"<br />{output}";
				}
			}
			else {
				foreach (var diag in errorDiagnostics) {
					Output += $@"<br / ><span class=""error"">{HttpUtility.HtmlEncode(diag)}</span>";
				}
			}
			Console.SetOut(previousOut);
		}
		private bool TryCompile(string source,out Assembly assembly,out IEnumerable<Diagnostic> errorDiagnostics) {
			assembly = null;
			var scriptCompilation = CSharpCompilation.CreateScriptCompilation(
				Path.GetRandomFileName(),
				CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithKind(SourceCodeKind.Script).WithLanguageVersion(LanguageVersion.Preview)),
				_references,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: new[]
				{
					"System",
					"System.IO",
					"System.Collections.Generic",
					"System.Console",
					"System.Diagnostics",
					"System.Dynamic",
					"System.Linq",
					"System.Linq.Expressions",
					"System.Net.Http",
					"System.Text",
					"System.Threading.Tasks"
						}),
				_previousCompilation
			);

			errorDiagnostics = scriptCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error);
			if (errorDiagnostics.Any()) {
				return false;
			}

			using (var peStream = new MemoryStream()) {
				var emitResult = scriptCompilation.Emit(peStream);

				if (emitResult.Success) {
					_submissionIndex++;
					_previousCompilation = scriptCompilation;
					assembly = Assembly.Load(peStream.ToArray());
					return true;
				}
			}

			return false;
		}
		public static async void RunInternal() {
			string outPut = string.Empty;
			//开始计时
			var sw = Stopwatch.StartNew();
			//重定向输出流
			var currentOut = Console.Out;
			var writer = new StringWriter();
			Console.SetOut(writer);
			Exception exception = null;
			Console.SetOut(currentOut);
			void Compile(string source) {
				var sw = Stopwatch.StartNew();
				var compilation = CSharpCompilation.Create("DynamicCode")
				.WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                //.AddReferences(References)
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp8)));
				sw.Stop();
			}
		}
	}
}
