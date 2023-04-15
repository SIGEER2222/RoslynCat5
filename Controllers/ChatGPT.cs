using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3;

namespace RoslynCat.Controllers
{
    public class ChatGPT
    {
        public async Task<string> Reply(string userText) {
            string result = string.Empty;

            if (string.IsNullOrWhiteSpace(userText)) {
                return result;
            }
            string OPENAPI_TOKEN = new GetConfig().OpenAI;
            var openAiService = new OpenAIService(new OpenAiOptions(){
                ApiKey =  OPENAPI_TOKEN
            });
            var completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest{
                Messages = new List<ChatMessage>{
                    ChatMessage.FromUser("我是一个C#初学者，在话题中尽量告诉我关于C#的信息，回复中尽量带着C#代码和讲解"),
                    ChatMessage.FromSystem("很高兴为你服务，我将在话题中尽量告诉你关于C#的信息"),
                    ChatMessage.FromAssistant("我会努力生成正确的C#代码并讲解"),
                    ChatMessage.FromUser(userText),
                },
                Model = Models.ChatGpt3_5Turbo,
                MaxTokens = 1500//optional
            });
            if (completionResult.Successful) {
               result = (completionResult.Choices.First().Message.Content);
            }
            else {
                await Console.Out.WriteLineAsync("请求失败" + completionResult);
            }
            await Console.Out.WriteLineAsync(result);
            return result;
        }
    }
}
