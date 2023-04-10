using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynCat.Roslyn
{
    //SyntaxToken
    public class Lexer
    {
        private readonly string _sourceCode;
        public Lexer(string sourceCode) {
            _sourceCode = sourceCode;
        }

        public IEnumerable<string> GetTypes() {
            var tree = CSharpSyntaxTree.ParseText(_sourceCode);
            var root = tree.GetRoot();
            //var keywords = root.DescendantTokens().Where(t => t.IsKeyword()).Select(t => t.Text).Distinct().ToList();
            var types = root.DescendantNodes()
                .OfType <TypeDeclarationSyntax>().Select(t => t.Identifier.Text).Distinct().ToList();
            return types;
        }

    }
}
