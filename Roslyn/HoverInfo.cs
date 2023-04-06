using Microsoft.CodeAnalysis;
using System.Text;
using System.Xml.Linq;

namespace RoslynCat.Roslyn
{
    public class HoverInfo
    {
        public static class  HoverInfoBuilder
        {
            public static string Build(SymbolInfo symbolInfo) => symbolInfo.Symbol switch {
                IMethodSymbol method => BuildMethodSymbol(method),
                ILocalSymbol local => BuildLocalSymbol(local),
                IFieldSymbol field => BuildLocalSymbol(field),
                _ => string.Empty
            };

            private static string BuildMethodSymbol(IMethodSymbol symbol) {
                var xml = symbol.GetDocumentationCommentXml();
                var doc = XDocument.Parse(xml);
                var summary = doc.Descendants("summary").FirstOrDefault();
                string accessibility = symbol.DeclaredAccessibility.ToString().ToLower();
                string isStatic = symbol.IsStatic ? "static " : "";
                string parameters = string.Join(", ", symbol.Parameters.Select(p => $"{p.Type} {p.Name}"));
                return $"(method) {accessibility} {isStatic}{symbol.Name}({parameters}) : {symbol.ReturnType}";
            }

            private static string BuildLocalSymbol(ILocalSymbol symbol) {
                string isConst = symbol.IsConst ? "const " : "";
                Console.WriteLine();
                return $"{symbol.Name} : {isConst}{symbol.Type}";
            }

            private static string BuildLocalSymbol(IFieldSymbol symbol) {
                string accessibility = symbol.DeclaredAccessibility.ToString().ToLower();
                string isStatic = symbol.IsStatic ? "static " : "";
                string isReadOnly = symbol.IsReadOnly ? "readonly " : "";
                string isConst = symbol.IsConst ? "const " : "";

                return $"{symbol.Name} : {accessibility} {isStatic}{isReadOnly}{isConst}{symbol.Type}";
            }
        }
    }
}
