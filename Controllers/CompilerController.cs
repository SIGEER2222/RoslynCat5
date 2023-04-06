using Microsoft.AspNetCore.Mvc;

namespace RoslynCat.Controllers
{
    [Route("completion")]
    public class CompilerController : Controller
    {
        //case 'complete': endPoint = '/completion/complete'; break;
        //case 'signature': endPoint = '/completion/signature'; break;
        //case 'hover': endPoint = '/completion/hover'; break;
        //case 'codeCheck': endPoint = '/completion/codeCheck'; break;

        [HttpGet("get")]
        public dynamic Get() {
            return "Get";
        }

        [HttpPost("resolve")]
        public dynamic Resolve([FromBody] SourceInfo source) {
            try {
                return "resolve";
            }
            catch (Exception ex) {
                return ex.Message;
                //throw;
            }
        } [HttpPost("complete")]
        public dynamic Complete([FromBody] SourceInfo source) {
            try {
                return "resolve";
            }
            catch (Exception ex) {
                return ex.Message;
                //throw;
            }
        } [HttpPost("signature")]
        public dynamic Signature([FromBody] SourceInfo source) {
            try {
                return "resolve";
            }
            catch (Exception ex) {
                return ex.Message;
                //throw;
            }
        } [HttpPost("hover")]
        public dynamic Hover([FromBody] SourceInfo source) {
            try {
                return "resolve";
            }
            catch (Exception ex) {
                return ex.Message;
                //throw;
            }
        } [HttpPost("codeCheck")]
        public dynamic CodeCheck([FromBody] SourceInfo source) {
            try {
                return "resolve";
            }
            catch (Exception ex) {
                return ex.Message;
                //throw;
            }
        }

        [HttpPost("compile")]
        public dynamic Compile([FromBody] SourceInfo source) {

            try {
                return "compile";
            }
            catch (Exception ex) {
                return ex.Message;
                //throw;
            }
        }

        [HttpPost("formatcode")]
        public dynamic formatcode([FromBody] SourceInfo source) {

            try {
                return "formatecode";
            }
            catch (Exception ex) {
                return ex.Message;
                //throw;
            }
        }
    }
}
