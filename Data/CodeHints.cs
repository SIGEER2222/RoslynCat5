namespace RoslynCat.Data
{
    public class CodeHints
    {
        public string Suggestion { get; set; }
        public string Description { get; set; }
    }

    public class ErrorMarker
    {
        public int StartLineNumber { get; }
        public int StartColumn { get; }
        public int EndLineNumber { get; }
        public int EndColumn { get; }
        public string Message { get; }
        public DiagnosticSeverity Severity { get; }

        public ErrorMarker(int startLineNumber,int startColumn,int endLineNumber,int endColumn,string message,DiagnosticSeverity severity) {
            StartLineNumber = startLineNumber;
            StartColumn = startColumn;
            EndLineNumber = endLineNumber;
            EndColumn = endColumn;
            Message = message;
            Severity = severity;
        }
    }
}
