﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using RoslynCat.Interface;

namespace RoslynCat.Roslyn
{
    /// <summary>
    /// 创建项目和工作区
    /// </summary>
    public class WorkSpaceService : IWorkSpaceService
    {
        private readonly AdhocWorkspace _workspace;
        private readonly Project _project;
        private  Document _document;

        public Document Document { get => _document; }

        public WorkSpaceService() {
            Assembly[] lst = new[] {
                Assembly.Load("Microsoft.CodeAnalysis.Workspaces"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Workspaces"),
                Assembly.Load("Microsoft.CodeAnalysis.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features")
            };

            var host = MefHostServices.Create(lst);
            _workspace = new AdhocWorkspace(host);

            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                                .WithOverflowChecks(true)
                                .WithOptimizationLevel(OptimizationLevel.Release)
                                .WithUsings(new[] { "System" });

            var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(),
                                version: VersionStamp.Create(),
                                name: "MyProject",
                                assemblyName: "MyAssembly",
                                language: LanguageNames.CSharp,
                                compilationOptions: options);

            var references =  Constants.DefaultMetadataReferences;

            _project = _workspace.AddProject(projectInfo).AddMetadataReferences(references);
            _document = _project.AddDocument("RoslynCat.cs",SourceText.From(Constants.defultCode));

            //_document = _workspace.AddDocument(_project.Id,"RoslynCat.cs",SourceText.From(Constants.defultCode));
        }

        /// <summary>
        /// 获取文档的语义模型
        /// </summary>
        /// <returns></returns>
        public async Task<SemanticModel> GetSmanticModelAsync() {
            return await _document.GetSemanticModelAsync();
        }

        /// <summary>
        /// 获取文档的编译结果
        /// </summary>
        /// <returns></returns>
        public async Task<EmitResult> GetEmitResultAsync() {
            var model = await _document.GetSemanticModelAsync();
            var ms = new MemoryStream();
            var emitResult = model.Compilation.Emit(ms);
            return emitResult;
        }

        /// <summary>
        /// 更新文档（其实就是重新添加一次）
        /// </summary>
        /// <param name="newCode"></param>
        public async void OnDocumentChange(string newCode) {
            var newSolution = _document.Project.Solution.WithDocumentText(_document.Id, SourceText.From(newCode));
            _workspace.TryApplyChanges(newSolution);
            _document = _project.AddDocument("RoslynCat.cs",SourceText.From(newCode));
            Console.WriteLine(_project.DocumentIds.Count());
        }

        //TODO
        private void AddUsings(string usings) {

            //DownloadNugetPackages.DownloadAllPackages(sourceInfo.Nuget);
            //var assemblies = DownloadNugetPackages.LoadPackages(sourceInfo.Nuget);
            //ScriptOptions.Default.AddReferences(usings).AddReferences(DefaultMetadataReferences);
        }

        private void AddNuget() {


        }
    }
}