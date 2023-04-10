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

            private static string BuildMethodSymbol(IMethodSymbol methodSymbol) {
                var parameters = string.Join(", ", methodSymbol.Parameters.Select(p => $"{p.Type} {p.Name}"));
                return $"(method) {methodSymbol.DeclaredAccessibility.ToString().ToLower()} {(methodSymbol.IsStatic ? "static " : "")}{methodSymbol.Name}({parameters}) : {methodSymbol.ReturnType}";
            }

            private static string BuildLocalSymbol(ILocalSymbol localSymbol) {
                return $"{localSymbol.Name} : {(localSymbol.IsConst ? "const " : "")}{localSymbol.Type}";
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
