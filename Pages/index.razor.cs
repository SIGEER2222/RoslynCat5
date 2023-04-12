using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.JSInterop;
using RoslynCat.Controllers;
using RoslynCat.Interface;
using RoslynCat.Roslyn;
using System.Collections.Concurrent;
using System.Text.Json;

namespace RoslynCat.Pages
{
    public partial class Index
    {
        public List<Diagnostic> Diagnostics { get; set; }
        [Inject] Compiler CompliterService { get; set; }
        //[Inject] CompletionDocument doc { get; set; }
        [Inject] IJSRuntime JS { get; set; }
        [Inject] IWorkSpaceService WorkSpaceService { get; set; }
        [Inject] Roslyn.CompletionProvider CompletionProvider { get; set; }
        private string result = "等待编译...";
        private string shareId = string.Empty;
        protected override void OnParametersSet() {
            //this.MonacoService.DiagnosticsUpdated += this.OnDiagnosticsUpdated;
            base.OnParametersSet();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            await Console.Out.WriteLineAsync(typeof(Index).Assembly.FullName);
            if (firstRender) {
                JsRuntimeExt.Shared = JS;
                await JsRuntimeExt.Shared.CreateMonacoEditorAsync(editorId,code);
                await CompliterService.CreatCompilation(code);
                await JsRuntimeExt.Shared.InvokeVoidAsync("monacoInterop.registerMonacoProviders",DotNetObjectReference.Create(this));
                //doc.Test();
            }
        }

        [JSInvokable("ProvideCompletionItems2")]
        public static async Task<CompletionItem[]> ProvideCompletionItems3(string a,int b) {
            // 方法实现
            return null;
        }

        [JSInvokable("ProvideCompletionItems")]
        public async Task<string> ProvideCompletionItems(string jsObj) {

            var obj = JsonSerializer.Deserialize<JsObj>(jsObj);
            //string code = await JsRuntimeExt.Shared.GetValue(editorId);
            SourceInfo sourceInfo = new SourceInfo(obj.Code,string.Empty,obj.Position);
            sourceInfo.Type = RequestType.Complete;
            await CompletionProvider.CreateProviderAsync(WorkSpaceService,sourceInfo);
            IResponse respone = await CompletionProvider.GetResultAsync();
            CompletionResult result = respone as CompletionResult;
            string jsonString = JsonSerializer.Serialize(result.Suggestions);
            return jsonString;
        }
        [JSInvokable("ProvideCompletionItems2")]
        public async Task<string> ProvideCompletionItems(string code,int position) {
            SourceInfo sourceInfo = new SourceInfo(code,string.Empty,position);
            sourceInfo.Type = RequestType.Complete;
            await CompletionProvider.CreateProviderAsync(WorkSpaceService,sourceInfo);
            IResponse respone = await CompletionProvider.GetResultAsync();
            CompletionResult result = respone as CompletionResult;
            return JsonSerializer.Serialize(result.Suggestions);
        }
        public class JsObj
        {
            public string Code { get; set; }
            public int Position { get; set; }
        }

        public class CompletionItem
        {
            public Label Label { get; set; }
            public string Suggestion { get; set; }
            public string Description { get; set; }
            public string ItemType { get; set; }
        }

        public class Label
        {
            public string LabelText { get; set; }
            public string Description { get; set; }
        }
        protected async Task Test() {
            code = await JsRuntimeExt.Shared.GetValue(editorId);
            result = CompliterService.CompileAndRun(code);
            await Console.Out.WriteLineAsync(result);
        }
        protected async Task CodeSharing() {
            code = await JsRuntimeExt.Shared.GetValue(editorId);
            if (string.IsNullOrEmpty(code)) return;
            CodeSharing share = new CodeSharing();
            await share.CreateGistAsync(code);
            shareId = share.GistId;
            await JsRuntimeExt.Shared.CopyUrl();
        }

        private void OnDiagnosticsUpdated(object sender,List<Diagnostic> diagnostics) {
            Diagnostics = diagnostics;
            InvokeAsync(() => { this.StateHasChanged(); });
            //_ = JS.SetMonacoDiagnosticsAsync(_editorId, diagnostics);
        }
    }

}