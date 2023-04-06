using RoslynCat.Interface;
using RoslynCat.Roslyn;
using System.Numerics;
using System.Text.Json;
using static RoslynCat.Data.SourceInfo;

namespace RoslynCat.Controllers
{
    public static class Map
    {
        //case 'complete': endPoint = '/completion/complete'; break;
        //case 'signature': endPoint = '/completion/signature'; break;
        //case 'hover': endPoint = '/completion/hover'; break;
        //case 'codeCheck': endPoint = '/completion/codeCheck'; break;
        public static RouteGroupBuilder MapTodosApi(this RouteGroupBuilder group) {
            group.MapPost("/complete",Completion);
            group.MapPost("/signature",Completion);
            group.MapPost("/hover",Completion);
            group.MapPost("/codeCheck",Completion);
            return group;
        }

        private static async Task Completion(HttpContext http) {

            if (http.Request.Body is null) {
                http.Response.StatusCode = 405;
                return;
            }

            using StreamReader stream = new StreamReader(http.Request.Body);
            string text = await stream.ReadToEndAsync();
            string end = http.Request.Path.Value.Split('/').Last();
            SourceInfo sourceInfo = JsonSerializer.Deserialize<SourceInfo>(text);
            sourceInfo.Type = end switch {
                "complete" => RequestType.Complete,
                "signature" => RequestType.Signature,
                "hover" => RequestType.Hover,
                "codeCheck" => RequestType.CodeCheck,
                _ => RequestType.None
            };
            IResponse respone = await CompletitionRequestHandler.Handle(sourceInfo);
            await JsonSerializer.SerializeAsync(http.Response.Body,respone);
        }
    }
}
