using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynCat.Roslyn
{
	public class HoverInfo
	{

		public static class HoverInfoBuilder
		{
			/// <summary>
			/// TODO
			/// 获取字面量的值和描述
			/// </summary>
			/// <param name="literal"></param>
			/// <returns></returns>
			public static string Literal(LiteralExpressionSyntax literal) {
				Type type = literal.Token.Value.GetType();
				string literalValue = string.Empty;

				if (literal.Token.Value is double doubleValue) {
					byte[] bytes = BitConverter.GetBytes(doubleValue);
					double value = BitConverter.ToDouble(bytes, 0);
					string stringValue = doubleValue.ToString("R");
					literalValue = $"{type}: {stringValue}";
				}
				else {
					literalValue = $"{type}: {literal.Token.Value}";

				}
				return literalValue;
			}

			/// <summary>
			/// 获取符号的语义描述
			/// </summary>
			/// <param name="symbolInfo"></param>
			/// <returns></returns>
			public static string Build(SymbolInfo symbolInfo) {

				string s = symbolInfo.Symbol switch {
					IMethodSymbol method => BuildMethodSymbol(method),
					ILocalSymbol local => BuildLocalSymbol(local),
					IFieldSymbol field => BuildLocalSymbol(field),
					ITypeSymbol type => BuildTypeSymbol(type),
					ISymbol symbol => BuildSymbol(symbol),
					_ => BuildSymbol(symbolInfo.Symbol)
				};
				return s;
			}
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


			private static string BuildTypeSymbol(ITypeSymbol typeSymbol) {
				var accessModifier = typeSymbol.DeclaredAccessibility.ToString().ToLower();
				var isSealed = typeSymbol.IsSealed ? "cannot be inherited" : "can be inherited";
				var inheritance = GetInheritanceHierarchy(typeSymbol);
				var result = $"{typeSymbol.ToDisplayString()} class{Environment.NewLine}" +
							 $" Represents {accessModifier} {isSealed}.{Environment.NewLine}" +
							 $"{inheritance}";
				return result;
			}

			private static string BuildSymbol(ISymbol symbol) {
				if (symbol is null) {
					return string.Empty;
				}
				var result = $"{symbol.Name} : keyword{Environment.NewLine} Represents a {symbol.Kind} keyword.{Environment.NewLine}";
				return result;
			}

			private static string GetInheritanceHierarchy(ITypeSymbol typeSymbol) {
				var hierarchy = new List<string>();
				var currentType = typeSymbol;
				while (currentType != null) {
					hierarchy.Add(currentType.Name);
					currentType = currentType.BaseType;
				}
				hierarchy.Reverse();
				return $"Inherits: {string.Join("->",hierarchy)}{Environment.NewLine}";
			}
		}
	}
}
