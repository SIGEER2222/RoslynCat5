namespace RoslynCat.Interface
{
	public interface IWorkSpaceService
	{
		public Document Document { get; }
		public Project Project { get; }
		public Task<SemanticModel> GetSmanticModelAsync();
		public Task<EmitResult> GetEmitResultAsync();
		public void OnDocumentChange(string newCode);
	}
}
