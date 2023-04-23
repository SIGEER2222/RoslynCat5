//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis;
//using RoslynCat.Data;
//using static RoslynCat.Roslyn.HoverInfo;
//using Microsoft.CodeAnalysis.Text;

//namespace RoslynCat.Roslyn
//{
//    public class HoverInfoProvider {
//        private readonly Document _document;
//        private readonly SemanticModel _semanticModel;
//        public async Task<HoverInfoResult> Provide(Document document,int position,SemanticModel semanticModel) {

//            SyntaxNode syntaxRoot = await document.GetSyntaxRootAsync();
//            SyntaxNode expressionNode = syntaxRoot.FindToken(position).Parent;
//            //var result = expressionNode switch{
//            //    VariableDeclaratorSyntax vd => semanticModel.GetTypeInfo(vd.ChildNodes()?.FirstOrDefault()?.ChildNodes()?.FirstOrDefault()).Type.ToString(),
//            //    PropertyDeclarationSyntax prop => prop.Type.ToString(),
//            //    ParameterSyntax p => p.Type.ToString(),
//            //    IdentifierNameSyntax i => semanticModel.GetTypeInfo(i).Type.ToString(),
//            //    SyntaxNode s =>HoverInfoBuilder.Build(semanticModel.GetSymbolInfo(s)),
//            //    _ => string.Empty
//            //};
//            var result = expressionNode switch{
//                VariableDeclaratorSyntax vd => semanticModel.GetTypeInfo(vd.ChildNodes().FirstOrDefault()?.ChildNodes().FirstOrDefault()).Type.ToString(),
//                PropertyDeclarationSyntax prop => prop.Type.ToString(),
//                ParameterSyntax p => p.Type.ToString(),
//                IdentifierNameSyntax i => semanticModel.GetTypeInfo(i).Type.ToString(),
//                SyntaxNode s => HoverInfoBuilder.Build(semanticModel.GetSymbolInfo(s)),
//                _ => string.Empty
//            };

//            var location = expressionNode.GetLocation();
//            if (!string.IsNullOrWhiteSpace(result)) {
//                return new HoverInfoResult() {
//                    Information = result,
//                    OffsetFrom = location.SourceSpan.Start,
//                    OffsetTo = location.SourceSpan.End
//                };
//            }

//            return null;
//        }

//        public string GetHoverTips(int position) {
//            SyntaxTree tree = _document.GetSyntaxTreeAsync().Result;
//            SyntaxNode node = tree.GetRoot().FindNode(new TextSpan(position, 0));
//            // 获取节点的符号信息
//            ISymbol symbol = _semanticModel.GetSymbolInfo(node).Symbol;
//            if (symbol != null) {
//                // 获取符号信息的注释
//                string docComment = symbol.GetDocumentationCommentXml();
//                if (!string.IsNullOrEmpty(docComment)) {
//                    return docComment;
//                }
//                // 如果没有注释，则返回符号的完整名称
//                return symbol.ToDisplayString();
//            }

//            return string.Empty;
//        }
//    }
//}
