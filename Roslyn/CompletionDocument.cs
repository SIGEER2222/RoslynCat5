using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Extensions.Logging;
using RoslynCat.Interface;
using System.Threading;
using System.Xml;

namespace RoslynCat.Roslyn
{
    public class CompletionDocument
    {
        private Document document;
        private SemanticModel semanticModel;
        private EmitResult emitResult;

        private readonly ICompleteProvider _completeProvider;
        private readonly ISignatureProvider _signatureProvider;
        private readonly IHoverProvider _hoverProvider;
        private readonly ICodeCheckProvider _codeCheckProvider;

        public ICompleteProvider complete { get; set; }
        //public ISignatureProvider signature { get; set; }
        public IHoverProvider hover { get; set; }
        public ICodeCheckProvider codeCheck { get; set; }

        public CompletionDocument() {
            _completeProvider = new CompletionProvider();
        }

        public void CreateDocument(Document doc,SemanticModel model,EmitResult emit) {
            document = doc;
            semanticModel = model;
            emitResult = emit;
        }

        public CompletionDocument(Document document,SemanticModel semanticModel,EmitResult emitResult) {
            this.document = document;
            this.semanticModel = semanticModel;
            this.emitResult = emitResult;
            //this.quickInfoProvider = new QuickInfoProvider(new DeferredQuickInfoContentProvider());
        }

        public CompletionDocument(ICompleteProvider completeProvider,IHoverProvider hoverProvider,ICodeCheckProvider codeCheckProvider) {
            _completeProvider = completeProvider;
            //_signatureProvider = signatureProvider;
            _hoverProvider = hoverProvider;
            _codeCheckProvider = codeCheckProvider;
            Console.WriteLine("创建了completion");
        }

        public void Test() {
            Console.WriteLine(1111);
        }

        public async Task Test(RequestType type,int position,CancellationToken cancellationToken) {
            IResponse i =  type switch {
                RequestType.Complete => await (new CompletionProvider()).Provide(document,position),
                //(RequestType.Signature, _, _) => await signature.Provide(document,position,semanticModel),
                RequestType.Hover => await (new HoverProvider()).Provide(document,position,semanticModel),
                RequestType.CodeCheck => await (new CodeCheckProvider()).Provide(emitResult,document,cancellationToken),
                _ =>null
            } ;
            await Console.Out.WriteLineAsync(i.ToString());
        }

        public async Task<IResponse> GetResult(RequestType type,int position,CancellationToken cancellationToken) => type switch {
            RequestType.Complete => await _completeProvider.Provide(document,position),
            //(RequestType.Signature, _, _) => await signature.Provide(document,position,semanticModel),
            RequestType.Hover => await hover.Provide(document,position,semanticModel),
            RequestType.CodeCheck => await _codeCheckProvider.Provide(emitResult,document,cancellationToken),
            RequestType.None => null
        };
    }
}
