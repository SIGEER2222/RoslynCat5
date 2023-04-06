

using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.JSInterop;
using RoslynCat.Controllers;
using RoslynCat.Roslyn;

namespace RoslynCat.Pages
{
    public partial class Index
    {
        public List<Diagnostic> Diagnostics { get; set; }
        [Inject] Compiler CompliterService { get; set; }
        //[Inject] CompletionDocument doc { get; set; }
        [Inject] IJSRuntime JS { get; set; }
        private string result = "等待编译...";
        private string shareId = string.Empty;
        protected override void OnParametersSet() {
            //this.MonacoService.DiagnosticsUpdated += this.OnDiagnosticsUpdated;
            base.OnParametersSet();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                Console.WriteLine("第一次加载");
                JsRuntimeExt.Shared = JS;
                await JsRuntimeExt.Shared.CreateMonacoEditorAsync(editorId,code);
                await CompliterService.CreatCompilation(code);
                //doc.Test();
            }
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