using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynCat.Interface;
using System.Collections.Concurrent;
using static RoslynCat.Roslyn.HoverInfo;

namespace RoslynCat.Roslyn
{
	public class CompleteProvider : ICompleteProvider
	{
		// Thanks to https://www.strathweb.com/2018/12/using-roslyn-c-completion-service-programmatically/

		/// <summary>
		/// 提供代码补全功能的异步方法，接受 Document 和 position 参数。
		/// </summary>
		/// <param name="document">要获取代码补全信息的 Document 对象。</param>
		/// <param name="position">代码补全信息的位置。</param>
		/// <returns>一个实现了 CompletionResult 接口的对象。</returns>
		public async Task<CompletionResult> Provide(Document document,int position) {

			var dict = new ConcurrentDictionary<string, string>();
			var completionService = CompletionService.GetService(document);
			CompletionList results = completionService.GetCompletionsAsync(document, position).Result;

			if (results is null) {
				return new CompletionResult();
			}
			var items = results.ItemsList;
			//Parallel.ForEach(items,async x => {
			//    CompletionDescription description = await completionService.GetDescriptionAsync(document, x);
			//    dict.TryAdd(x.DisplayText,description.Text);
			//});

			await Task.WhenAll(items.Select(async x =>
			{
				CompletionDescription description = await completionService.GetDescriptionAsync(document, x);
				dict.TryAdd(x.DisplayText,description.Text);
			}));

			CompletionResult result = new CompletionResult(){
				Suggestions = dict,
			};
			return result;
		}
	}

	public class HoverProvider : IHoverProvider
	{
		public async Task<HoverInfoResult> Provide(Document document,int position,SemanticModel semanticModel) {
			SyntaxNode expressionNode = semanticModel.SyntaxTree.GetRoot().FindToken(position).Parent;
			string result = expressionNode switch{
				VariableDeclaratorSyntax vd => $"Variable: {vd.Identifier.Text} ({semanticModel.GetTypeInfo(vd).Type})",
				PropertyDeclarationSyntax prop => $"Property: {prop.Identifier.Text} ({prop.Type})",
				MethodDeclarationSyntax method => $"Method: {method.Identifier.Text} ({method.ReturnType})",
				ParameterSyntax param => $"Parameter: {param.Identifier.Text} ({param.Type})",
				LiteralExpressionSyntax literal => HoverInfoBuilder.Literal(literal),
				_ => HoverInfoBuilder.Build(semanticModel.GetSymbolInfo(expressionNode))
			};
			await Console.Out.WriteLineAsync(result);
			Location location = expressionNode.GetLocation();
			if (string.IsNullOrWhiteSpace(result)) {
				return default;
			}
			else {
				return new HoverInfoResult() {
					Information = result,
					OffsetFrom = location.SourceSpan.Start,
					OffsetTo = location.SourceSpan.End
				};
			}
		}
	}

	public class CodeCheckProvider : ICodeCheckProvider
	{
		public async Task<CodeCheckResult> Provide(EmitResult emitResult,Document document) {
			CodeCheckResult result = new CodeCheckResult();

			var codeChecks = result.codeChecks;
			foreach (var r in emitResult.Diagnostics) {
				var sev = r.Severity == DiagnosticSeverity.Error ? CodeCheckSeverity.Error : r.Severity == DiagnosticSeverity.Warning ? CodeCheckSeverity.Warning : r.Severity == DiagnosticSeverity.Info ? CodeCheckSeverity.Info : CodeCheckSeverity.Hint;
				var keyword = (await document.GetTextAsync()).GetSubText(r.Location.SourceSpan).ToString();
				var msg = new CodeCheckResult.CodeCheck() { Id = r.Id, Keyword = keyword, Message = r.GetMessage(), OffsetFrom = r.Location.SourceSpan.Start, OffsetTo = r.Location.SourceSpan.End, Severity = sev, SeverityNumeric = (int)sev };
				codeChecks.Add(msg);
			}
			return result;
		}
	}

	//public class SignatureProvider : ISignatureProvider
	//{
	//    public Task<SignatureHelpResult> Provide(Document document,int positionm,SemanticModel semanticModel) {

	//        var invocation = await InvocationContext.GetInvocation(document, position);
	//        if (invocation == null) return null;

	//        int activeParameter = 0;
	//        foreach (var comma in invocation.Separators) {
	//            if (comma.Span.Start > invocation.Position)
	//                break;

	//            activeParameter += 1;
	//        }

	//        var signaturesSet = new HashSet<Signatures>();
	//        var bestScore = int.MinValue;
	//        Signatures bestScoredItem = null;

	//        var types = invocation.ArgumentTypes;
	//        ISymbol throughSymbol = null;
	//        ISymbol throughType = null;
	//        var methodGroup = invocation.SemanticModel.GetMemberGroup(invocation.Receiver).OfType<IMethodSymbol>();
	//        if (invocation.Receiver is MemberAccessExpressionSyntax) {
	//            var throughExpression = ((MemberAccessExpressionSyntax)invocation.Receiver).Expression;
	//            var typeInfo = semanticModel.GetTypeInfo(throughExpression);
	//            throughSymbol = invocation.SemanticModel.GetSpeculativeSymbolInfo(invocation.Position,throughExpression,SpeculativeBindingOption.BindAsExpression).Symbol;
	//            throughType = invocation.SemanticModel.GetSpeculativeTypeInfo(invocation.Position,throughExpression,SpeculativeBindingOption.BindAsTypeOrNamespace).Type;
	//            var includeInstance = (throughSymbol != null && !(throughSymbol is ITypeSymbol)) ||
	//                throughExpression is LiteralExpressionSyntax ||
	//                throughExpression is TypeOfExpressionSyntax;
	//            var includeStatic = (throughSymbol is INamedTypeSymbol) || throughType != null;
	//            if (throughType == null) {
	//                throughType = typeInfo.Type;
	//                includeInstance = true;
	//            }
	//            methodGroup = methodGroup.Where(m => (m.IsStatic && includeStatic) || (!m.IsStatic && includeInstance));
	//        }
	//        else if (invocation.Receiver is SimpleNameSyntax && invocation.IsInStaticContext) {
	//            methodGroup = methodGroup.Where(m => m.IsStatic || m.MethodKind == MethodKind.LocalFunction);
	//        }

	//        foreach (var methodOverload in methodGroup) {
	//            var signature = BuildSignature(methodOverload);
	//            signaturesSet.Add(signature);

	//            var score = InvocationScore(methodOverload, types);
	//            if (score > bestScore) {
	//                bestScore = score;
	//                bestScoredItem = signature;
	//            }
	//        }

	//        return new SignatureHelpResult() {
	//            Signatures = signaturesSet.ToArray(),
	//            ActiveParameter = activeParameter,
	//            ActiveSignature = Array.IndexOf(signaturesSet.ToArray(),bestScoredItem)
	//        };
	//    }

	//    private static Signatures BuildSignature(IMethodSymbol symbol) {
	//        var parameters = new List<Parameter>();
	//        foreach (var parameter in symbol.Parameters) {
	//            parameters.Add(new Parameter() { Label = parameter.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) });
	//        };
	//        var signature = new Signatures
	//        {
	//            Documentation = symbol.GetDocumentationCommentXml(),
	//            Label = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
	//            Parameters = parameters.ToArray()
	//        };

	//        return signature;
	//    }

	//    private int InvocationScore(IMethodSymbol symbol,IEnumerable<TypeInfo> types) {
	//        var parameters = symbol.Parameters;
	//        if (parameters.Count() < types.Count())
	//            return int.MinValue;

	//        var score = 0;
	//        var invocationEnum = types.GetEnumerator();
	//        var definitionEnum = parameters.GetEnumerator();
	//        while (invocationEnum.MoveNext() && definitionEnum.MoveNext()) {
	//            if (invocationEnum.Current.ConvertedType == null)
	//                score += 1;

	//            else if (SymbolEqualityComparer.Default.Equals(invocationEnum.Current.ConvertedType,definitionEnum.Current.Type))
	//                score += 2;
	//        }
	//        return score;
	//    }
	//}

}
