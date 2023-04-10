using Microsoft.AspNetCore.Mvc;
using RoslynCat.Interface;
using RoslynCat.Roslyn;

namespace RoslynCat.Controllers
{

    [Route("completion")]
    [ApiController]
    public class CompletionController : ControllerBase
    {
        private IWorkSpaceService _workSpace;
        private CompletionProvider _completionProvider;
        public CompletionController(IWorkSpaceService workSpace,CompletionProvider completionProvider) {
            _workSpace = workSpace;
            _completionProvider = completionProvider;
            //Console.WriteLine(11222);
            //if (!ModelState.IsValid) {
            //    // 如果模型状态无效，那么打印错误信息
            //    var errors = ModelState.Values.SelectMany(v => v.Errors);
            //    foreach (var error in errors) {
            //        Console.WriteLine(error.ErrorMessage);
            //    }
            //}
            //else {
            //    Console.Out.WriteLineAsync("11111");
            //}
        }


        private async Task<IResponse> Handle(Request request,RequestType type) {
            SourceInfo sourceInfo = new SourceInfo(request.SourceCode,request.Assemblies,request.Position);
            sourceInfo.Type = type;
            await _completionProvider.CreateProviderAsync(_workSpace,sourceInfo);
            IResponse respone = await _completionProvider.GetResultAsync();
            Console.WriteLine(respone?.ToString());
            return respone;
        }

        [HttpPost("complete")]
        public async Task<ActionResult> Complete([FromBody] Request request) {
            // 处理完成端点的逻辑
            IResponse response = await Handle(request,RequestType.Complete);
            CompletionResult result = response as CompletionResult;
            return Ok(result);
        }

        [HttpPost("signature")]
        public async Task<ActionResult> Signature([FromBody] Request request) {
            // 处理签名端点的逻辑
            IResponse response = await Handle(request,RequestType.Signature);
            SignatureHelpResult result = response as SignatureHelpResult;
            return Ok(result);
        }

        [HttpPost("hover")]
        public async Task<ActionResult> Hover([FromBody] Request request) {
            // 处理悬停端点的逻辑
            await Console.Out.WriteLineAsync("hover");
            IResponse response = await Handle(request,RequestType.Hover);
            HoverInfoResult result = response as HoverInfoResult;
            return Ok(result);
        }

        [HttpPost("codeCheck")]
        public async Task<ActionResult> CodeCheck([FromBody] Request request) {
            // 处理代码检查端点的逻辑
            IResponse response = await Handle(request,RequestType.CodeCheck);
            CodeCheckResult result = response as CodeCheckResult;
            return Ok(result);
        }

        public class Request
        {
            public string SourceCode { get; set; }
            public int Position { get; set; }
            public string? Assemblies { get; set; }
        }
    }
}
