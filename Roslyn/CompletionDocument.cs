using Microsoft.AspNetCore.Components;
using RoslynCat.Interface;

namespace RoslynCat.Roslyn
{
    public class CompletionDocument
    {
        private  Document document;
        private  SemanticModel semanticModel;
        private  EmitResult emitResult;

        private readonly ICompleteProvider _completeProvider;
        private readonly ISignatureProvider _signatureProvider;
        private readonly IHoverProvider _hoverProvider;
        private readonly ICodeCheckProvider _codeCheckProvider;
        public void CreateDocument(Document document,SemanticModel semanticModel,EmitResult emitResult) {
            this.document = document;
            this.semanticModel = semanticModel;
            this.emitResult = emitResult;
            //this.quickInfoProvider = new QuickInfoProvider(new DeferredQuickInfoContentProvider());
        }
        public CompletionDocument() {

        }
        public CompletionDocument(ICompleteProvider completeProvider,ISignatureProvider signatureProvider,IHoverProvider hoverProvider,ICodeCheckProvider codeCheckProvider) {
            _completeProvider = completeProvider;
            _signatureProvider = signatureProvider;
            _hoverProvider = hoverProvider;
            _codeCheckProvider = codeCheckProvider;
        }

        public async Task<IResponse> GetResult(RequestType type,int position,CancellationToken cancellationToken) => (type, position, cancellationToken) switch {
            (RequestType.Complete, _, _) => await _completeProvider.Provide(document,position),
            (RequestType.Signature, _, _) => await _signatureProvider.Provide(document,position,semanticModel),
            (RequestType.Hover, _, _) => await _hoverProvider.Provide(document,position,semanticModel),
            (RequestType.CodeCheck, _, _) => await _codeCheckProvider.Provide(emitResult,document,cancellationToken),
            (RequestType.None, _, _) => null
        };

        //public async Task<CompletionResult[]> GetTabCompletion(int position,CancellationToken cancellationToken) {
        //    return await completionProvider.Provide(document,position,semanticModel);
        //}

        //public async Task<SignatureHelpResult> GetSignatureHelp(int position,CancellationToken cancellationToken) {
        //    return await signatureProvider.Provide(document,position,semanticModel);
        //}


        //public async Task<HoverInfoResult> GetHoverInformation(int position,CancellationToken cancellationToken) {
        //    //var info = await quickInfoProvider.GetItemAsync(document, position, cancellationToken);
        //    //return new HoverInfoResult() { Information = info.Create().ToString() };
        //    return await hoverProvider.Provide(document,position,semanticModel);
        //}

        //public async Task<CodeCheckResult[]> GetCodeCheckResults(int position,CancellationToken cancellationToken) {
        //    return await codeCheckProvider.Provide(document,emitResult,cancellationToken);
        //}

    }
}
