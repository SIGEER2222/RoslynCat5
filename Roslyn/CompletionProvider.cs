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
            _completeProvider    = completeProvider;
            //_signatureProvider = signatureProvider;
            _hoverProvider       = hoverProvider;
            _codeCheckProvider   = codeCheckProvider;
        }

        public async Task<CompletionProvider> CreateProviderAsync(IWorkSpaceService workSpace, SourceInfo sourceInfo) {
            workSpace.OnDocumentChange(sourceInfo.SourceCode);
            document      = workSpace.Document;
            semanticModel = await workSpace.GetSmanticModelAsync();
            emitResult    = await workSpace.GetEmitResultAsync();
            position      = sourceInfo.Position;
            type          = sourceInfo.Type;
            return this;
        }

        public async Task<IResponse> GetResultAsync() => type switch {
            RequestType.Complete      => await _completeProvider.Provide(document,position),
            //(RequestType.Signature) => await _signatureProvider.Provide(document,position,semanticModel),
            RequestType.Hover         => await _hoverProvider.Provide(document,position,semanticModel),
            RequestType.CodeCheck     => await _codeCheckProvider.Provide(emitResult,document),
            RequestType.None          => null
        };
    }
}
