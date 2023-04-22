using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using RoslynCat.Interface;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography.Xml;

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

        public async Task CreateProviderAsync(SourceInfo sourceInfo) {
            _workSpace.OnDocumentChange(sourceInfo.SourceCode);
            _emitResult = await _workSpace.GetEmitResultAsync();
            semanticModel = await _workSpace.GetSmanticModelAsync();
            position = sourceInfo.Position;
            type = sourceInfo.Type;
        }

        public async Task<IResponse> GetResultAsync() => type switch {
            RequestType.Complete => await _completeProvider.Provide(_workSpace.Document,position),
            //(RequestType.Signature) => await _signatureProvider.Provide(document,position,semanticModel),
            RequestType.Hover => await _hoverProvider.Provide(_workSpace.Document,position,semanticModel),
            RequestType.CodeCheck => await _codeCheckProvider.Provide(_emitResult,_workSpace.Document),
            RequestType.None => null
        };

        public async Task<string> FormatCode(string code) {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
            SyntaxNode formattedNode = root.NormalizeWhitespace();
            return formattedNode.ToFullString();
        }
        

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

                entryPoint?.Invoke(null,new object[] { args });

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
        private CSharpCompilation CreateCompilation(SyntaxTree syntaxTree) =>CSharpCompilation.Create(
            Path.GetRandomFileName(),
            syntaxTrees: new[] { syntaxTree },
            references: Constants.DefaultMetadataReferences,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}
