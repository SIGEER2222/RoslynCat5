using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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
        [Inject] IGistService GistService { get; set; }
        [Inject] IJSRuntime JS { get; set; }
        [Inject] Roslyn.CompletionProvider CompletionProvider { get; set; }
        [Parameter] public string gistId { get; set; }

        private string shareId = string.Empty;
        string baseUri;

        protected override async void OnInitialized() {
            baseUri = NavigationManager.BaseUri;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                JsRuntimeExt.Shared = JS;
                if (gistId is object) {
                    code = await GistService.GetGistContentAsync(gistId);
                }
                else {
                    code = await JsRuntimeExt.Shared.GetOldCode();
                }
                Result = "等待编译……";
                await JsRuntimeExt.Shared.CreateMonacoEditorAsync(editorId,code);
                await JsRuntimeExt.Shared.CreateMonacoEditorAsync(resultId,Result);
                CompletionProvider.RunCode(code);
                await JsRuntimeExt.Shared.InvokeVoidAsync("monacoInterop.registerMonacoProviders",DotNetObjectReference.Create(this));
            }
        }

        [JSInvokable("FormatCode")]
        public async Task<string> FormatCode(string code) => await CompletionProvider.FormatCode(code);

        [JSInvokable("HoverInfoProvide")]
        public async Task<string> HoverInfoProvide(string code,int position) {
            IResponse respone = await Provider(code,position,RequestType.Hover);
            HoverInfoResult result = respone as HoverInfoResult;
            return JsonSerializer.Serialize(result);
        }
        [JSInvokable("ProvideCompletionItems")]
        public async Task<string> ProvideCompletionItems(string code,int position) {
            IResponse respone = await Provider(code,position,RequestType.Complete);
            CompletionResult result = respone as CompletionResult;
            return JsonSerializer.Serialize(result.Suggestions);
        }
        [JSInvokable("GetModelMarkers")]
        public async Task<string> GetModelMarkers(string code,int position) {
            IResponse respone = await Provider(code,position,RequestType.CodeCheck);
            CodeCheckResult result = respone as CodeCheckResult;
            return JsonSerializer.Serialize(result.codeChecks);
        }

        [JSInvokable("AutoRunCode")]
        public async Task<string> AutoRunCode(string code) {
            string inputValue = GetConsoleValue()??string.Empty;
            return await CompletionProvider.RunCode(code,inputValue);
        }

        protected async Task<IResponse> Provider(string code,int position,RequestType request) {
            SourceInfo sourceInfo = new SourceInfo(code,string.Empty,position);
            sourceInfo.Type = request;
            await CompletionProvider.CreateProviderAsync(sourceInfo);
            IResponse respone = await CompletionProvider.GetResultAsync();
            return respone;
        }

        protected async Task RunCode() {
            string inputValue = GetConsoleValue()??string.Empty;
            code = await JsRuntimeExt.Shared.GetValue(editorId);
            Result = await CompletionProvider.RunCode(code,inputValue);
            await JsRuntimeExt.Shared.SetValue(resultId,Result);
        }

       
        protected async Task CodeSharing() {
            code = await JsRuntimeExt.Shared.GetValue(editorId);
            if (string.IsNullOrEmpty(code)) return;
            await GistService.CreateGistAsync(code);
            shareId = $"{baseUri}codeshare/{GistService.GistId}";
            await JsRuntimeExt.Shared.CopyUrl(shareId);
            show = true;
        }

        private void OnMyParameterChanged() {
            JsRuntimeExt.Shared.CreateMonacoEditorAsync(resultId,Result);
        }

        private void OnDiagnosticsUpdated(object sender,List<Diagnostic> diagnostics) {
            Diagnostics = diagnostics;
            InvokeAsync(() => { this.StateHasChanged(); });
            //_ = JS.SetMonacoDiagnosticsAsync(_editorId, diagnostics);
        }

        //protected async Task AskGPT() {
        //    string ask =  await JsRuntimeExt.Shared.GetValue(editorId);
        //    Result = "思考中……请等待";
        //    askGpt = "正在思考，请勿重复点击";
        //    StateHasChanged();
        //    await JsRuntimeExt.Shared.SetValue(resultId,Result);
        //    Result = await new ChatGPT().Reply(ask);
        //    await JsRuntimeExt.Shared.SetValue(resultId,Result);
        //    askGpt = "问问ChatGPT?";
        //}
    }

}