using System.Security.Cryptography.Xml;

namespace RoslynCat.Interface
{
    public interface IProvider<T>
    {

    }

    public interface ICompleteProvider : IProvider<CompletionResult>
    {
        public Task<CompletionResult> Provide(Document document,int position);
    }
    public interface ISignatureProvider : IProvider<SignatureHelpResult>
    {
        public Task<SignatureHelpResult> Provide(Document document,int positionm,SemanticModel semanticModel);
    }

    public interface IHoverProvider : IProvider<HoverInfoResult>
    {
        public Task<HoverInfoResult> Provide(Document document,int position,SemanticModel semanticModel);
    }
    public interface ICodeCheckProvider : IProvider<CodeCheckResult>
    {
        public Task<CodeCheckResult> Provide(EmitResult emitResult,Document document);
    }
}
