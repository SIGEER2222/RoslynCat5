using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
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
        [Inject] IGistService GistService { get; set; }
        [Inject] IJSRuntime JS { get; set; }
        [Inject] IWorkSpaceService WorkSpaceService { get; set; }
        [Inject] Roslyn.CompletionProvider CompletionProvider { get; set; }
        [Parameter] public string gistId { get; set; }

        private string shareId = string.Empty;
        Uri uri;
        protected override async void OnInitialized() {
            uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            //if (true) {

            //}
            //if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("GistId",out var gistId)) {
            //    code = await GistService.GetGistContentAsync(gistId);
            //}
            await Console.Out.WriteLineAsync("111");
        }

        //protected override async Task OnParametersSetAsync() {
        //    //this.MonacoService.DiagnosticsUpdated += this.OnDiagnosticsUpdated;
        //    if (gistId is object) {
        //        code = await GistService.GetGistContentAsync(gistId);
        //    }
        //    base.OnParametersSet();
        //}

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            await Console.Out.WriteLineAsync(typeof(Index).Assembly.FullName);
            if (firstRender) {
                JsRuntimeExt.Shared = JS;
                if (gistId is object) {
                    code = await GistService.GetGistContentAsync(gistId);
                }
                Result = "等待编译……";
                await JsRuntimeExt.Shared.CreateMonacoEditorAsync(editorId,code);
                await JsRuntimeExt.Shared.CreateMonacoEditorAsync(resultId,Result);
                await CompliterService.CreatCompilation(code);
                await JsRuntimeExt.Shared.InvokeVoidAsync("monacoInterop.registerMonacoProviders",DotNetObjectReference.Create(this));
            }
        }

        [JSInvokable("FormatCode")]
        public async Task<string> FormatCode(string code) {
            // 方法实现
            string format = await CompletionProvider.FormatCode(code);
            return format;
        }

        [JSInvokable("HoverInfoProvide")]
        public async Task<string> HoverInfoProvide(string code,int position) {
            SourceInfo sourceInfo = new SourceInfo(code,string.Empty,position);
            sourceInfo.Type = RequestType.Hover;
            await CompletionProvider.CreateProviderAsync(WorkSpaceService,sourceInfo);
            IResponse respone = await CompletionProvider.GetResultAsync();
            HoverInfoResult result = respone as HoverInfoResult;
            return JsonSerializer.Serialize(result);
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
        [JSInvokable("GetModelMarkers")]
        public async Task<string> GetModelMarkers(string code,int position) {
            SourceInfo sourceInfo = new SourceInfo(code,string.Empty,position);
            sourceInfo.Type = RequestType.CodeCheck;
            await CompletionProvider.CreateProviderAsync(WorkSpaceService,sourceInfo);
            IResponse respone = await CompletionProvider.GetResultAsync();
            CodeCheckResult result = respone as CodeCheckResult;
            return JsonSerializer.Serialize(result.codeChecks);
        }

        protected async Task Test() {
            code = await JsRuntimeExt.Shared.GetValue(editorId);
            Result = CompliterService.CompileAndRun(code);
            await JsRuntimeExt.Shared.SetValue(resultId,Result);
            await Console.Out.WriteLineAsync(Result);
        }

        protected async Task AskGPT() {
            string ask =  await JsRuntimeExt.Shared.GetValue(editorId);
            Result = "思考中……请等待";
            askGpt = "正在思考，请勿重复点击";
            StateHasChanged();
            await JsRuntimeExt.Shared.SetValue(resultId,Result);
            Result = await new ChatGPT().Reply(ask);
            await JsRuntimeExt.Shared.SetValue(resultId,Result);
            askGpt = "问问ChatGPT?";
        }
        protected async Task CodeSharing() {
            code = await JsRuntimeExt.Shared.GetValue(editorId);
            if (string.IsNullOrEmpty(code)) return;
            CodeSharing share = new CodeSharing();
            await share.CreateGistAsync(code);
            shareId = "https://localhost:7175/codeshare/" + share.GistId;
            await JsRuntimeExt.Shared.CopyUrl();
        }

        private void OnMyParameterChanged() {
            JsRuntimeExt.Shared.CreateMonacoEditorAsync(resultId,Result);
        }

        private void OnDiagnosticsUpdated(object sender,List<Diagnostic> diagnostics) {
            Diagnostics = diagnostics;
            InvokeAsync(() => { this.StateHasChanged(); });
            //_ = JS.SetMonacoDiagnosticsAsync(_editorId, diagnostics);
        }
    }

}