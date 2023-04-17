using System.Collections.Concurrent;

namespace RoslynCat.Rules
{
    public class RequestRateLimit
    {
        private readonly RequestDelegate _nextMiddleware;
        private readonly int _limit;
        private readonly IDictionary<string, int> _requestCounts;

        public RequestRateLimit(RequestDelegate nextMiddleware,int limit) {
            _nextMiddleware = nextMiddleware;
            _limit = limit;
            _requestCounts = new ConcurrentDictionary<string,int>();
        }

        public async Task InvokeAsync(HttpContext context) {
            var ipAddress = context.Connection.RemoteIpAddress.ToString();

            if (_requestCounts.TryGetValue(ipAddress,out var count)) {
                if (count >= _limit) {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Too many requests. Please try again later.");
                    return;
                }

                _requestCounts[ipAddress] = count + 1;
            }
            else {
                _requestCounts[ipAddress] = 1;
            }

            await _nextMiddleware(context);
        }
    }

    public static class RequestRateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestRateLimit(this IApplicationBuilder app,int limit) {
            return app.UseMiddleware<RequestRateLimit>(limit);
        }
    }
}
