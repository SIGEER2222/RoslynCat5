namespace RoslynCat.Interface
{
	public interface IProvider<T>
	{

	}

	public interface ICompleteProvider : IProvider<CompletionResult>
	{
		Task<CompletionResult> Provide(Document document,int position);
	}
	public interface ISignatureProvider : IProvider<SignatureHelpResult>
	{
		Task<SignatureHelpResult> Provide(Document document,int positionm,SemanticModel semanticModel);
	}

	public interface IHoverProvider : IProvider<HoverInfoResult>
	{
		Task<HoverInfoResult> Provide(Document document,int position,SemanticModel semanticModel);
	}
	public interface ICodeCheckProvider : IProvider<CodeCheckResult>
	{
		Task<CodeCheckResult> Provide(EmitResult emitResult,Document document);
	}
}
