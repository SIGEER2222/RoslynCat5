using RoslynCat.Roslyn;
using System.ComponentModel.DataAnnotations;

namespace RoslynCat.SQL
{
    public class CodeValidationAttribute : ValidationAttribute
    {
        public CodeValidationAttribute() {
        }

        protected override ValidationResult IsValid(object value,ValidationContext validationContext) {
            if (value is string code) {
                WorkSpaceService workSpace = new WorkSpaceService ();
                workSpace.OnDocumentChange(value.ToString());
                var syntaxTree = workSpace.Document.GetSyntaxTreeAsync().Result;
                var result =  workSpace.GetEmitResultAsync().Result;
                if (result.Success is not true) {
                    string res = string.Join(Environment.NewLine,result.Diagnostics
                       .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                       .Select(diagnostic => $"{syntaxTree.GetLineSpan(diagnostic.Location.SourceSpan).StartLinePosition.Line + 1} : {diagnostic.Id}, {diagnostic.GetMessage()}"));
                    return new ValidationResult($"{res}");
                }
                return ValidationResult.Success;
            }
            return ValidationResult.Success;
        }
    }
}
