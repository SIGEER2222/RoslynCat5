using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynCat.Interface;

namespace RoslynCat.Roslyn
{
    public class CompletionProvider
    {
        private Document document;
        private SemanticModel semanticModel;
        private EmitResult emitResult;
        private int position;
        private RequestType type;

        private readonly ICompleteProvider  _completeProvider;
        private readonly ISignatureProvider _signatureProvider;
        private readonly IHoverProvider     _hoverProvider;
        private readonly ICodeCheckProvider _codeCheckProvider;

        public CompletionProvider(ICompleteProvider completeProvider,IHoverProvider hoverProvider,ICodeCheckProvider codeCheckProvider) {
            _completeProvider = completeProvider;
            //_signatureProvider = signatureProvider;
            _hoverProvider = hoverProvider;
            _codeCheckProvider = codeCheckProvider;
        }

        public async Task<CompletionProvider> CreateProviderAsync(IWorkSpaceService workSpace,SourceInfo sourceInfo) {
            workSpace.OnDocumentChange(sourceInfo.SourceCode);
            document = workSpace.Document;
            semanticModel = await workSpace.GetSmanticModelAsync();
            emitResult = await workSpace.GetEmitResultAsync();
            position = sourceInfo.Position;
            type = sourceInfo.Type;
            return this;
        }

        public async Task<IResponse> GetResultAsync() => type switch {
            RequestType.Complete => await _completeProvider.Provide(document,position),
            //(RequestType.Signature) => await _signatureProvider.Provide(document,position,semanticModel),
            RequestType.Hover => await _hoverProvider.Provide(document,position,semanticModel),
            RequestType.CodeCheck => await _codeCheckProvider.Provide(emitResult,document),
            RequestType.None => null
        };

        public async Task<string> FormatCode(string code) {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
            SyntaxNode formattedNode = root.NormalizeWhitespace();
            return formattedNode.ToFullString();
        }

        public async Task<string> RunCode(string code , string read = "") {

            string res = string.Empty;
            var syntaxTree = await document.GetSyntaxTreeAsync();
            using (MemoryStream ms = new MemoryStream()) {

                if (!emitResult.Success) {
                    res = string.Join(Environment.NewLine,emitResult.Diagnostics
                           .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                           .Select(diagnostic => $"{syntaxTree.GetLineSpan(diagnostic.Location.SourceSpan).StartLinePosition.Line + 1} : {diagnostic.Id}, {diagnostic.GetMessage()}"));
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
}
