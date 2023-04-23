using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel;
using System.Data;
using System.Numerics;
using System.Xml;

namespace RoslynCat.Abandon
{
	/// <summary>
	/// 创建工作区、添加dll引用
	/// TODO添加using引用
	/// </summary>
	public class CompletionWorkspace
	{
		private Project _project;
		private AdhocWorkspace _workspace;
		private List<MetadataReference> _metadataReferences;

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
			MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(BigInteger).Assembly.Location),
		};


		public static CompletionWorkspace Create(List<string> usings) {
			Assembly[] lst = new[] {
				Assembly.Load("Microsoft.CodeAnalysis.Workspaces"),
				Assembly.Load("Microsoft.CodeAnalysis.CSharp.Workspaces"),
				Assembly.Load("Microsoft.CodeAnalysis.Features"),
				Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features")
			};

			var host = MefHostServices.Create(lst);
			var workspace = new AdhocWorkspace(host);
			var references = DefaultMetadataReferences.ToList();

			//TODO
			//DownloadNugetPackages.DownloadAllPackages(sourceInfo.Nuget);
			//var assemblies = DownloadNugetPackages.LoadPackages(sourceInfo.Nuget);
			ScriptOptions.Default.AddReferences(usings).AddReferences(DefaultMetadataReferences);

			var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "TempProject", "TempProject", LanguageNames.CSharp);
			workspace.AddProject(projectInfo);
			var project = workspace.CurrentSolution.GetProject(projectInfo.Id).WithMetadataReferences(references);

			return new CompletionWorkspace() { _workspace = workspace,_project = project,_metadataReferences = references };
		}

		//public async Task<CompletionDocument> CreateDocument(string code) {
		//    var document = _workspace.AddDocument(_project.Id, "MyFile2.cs", SourceText.From(code));
		//    var st = await document.GetSyntaxTreeAsync();
		//    var compilation =CSharpCompilation.Create("Temp",
		//            new[] { st },
		//            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
		//            references: _metadataReferences
		//        );
		//    var result = compilation.Emit("temp");

		//    SemanticModel semanticModel = compilation.GetSemanticModel(st, true);
		//    completionDocument = completionDocument.CreateDocument(document,semanticModel,result);
		//    return completionDocument;
		//}
	}
}
