using System.Net.Http.Json;

namespace RoslynCat.Controllers;

public abstract class BaseHttpClientService
{
    protected readonly HttpClient Client;

    protected BaseHttpClientService(HttpClient client)
    {
        Client = client;
    }

    protected virtual void OnRequesting()
    {

    }

    protected virtual void OnRequested<TEntity>(TEntity entity)
    {

    }

    protected virtual void OnException(Exception ex)
    {
        throw new Exception(ex.Message, ex);
    }

    public async Task<TResult> Get<TResult>(string url)
    {
        OnRequesting();

        var result = await Client.GetFromJsonAsync<TResult>(url);
        if (result is null)
        {
            throw new NullReferenceException(nameof(result));
        }

        OnRequested(result);
        return result;
    }

    public async Task<TResult> Post<TResult, TBody>(string url, TBody body)
    {
        OnRequesting();

        var response = await Client.PostAsJsonAsync(url, body);
        var result = await response.Content.ReadFromJsonAsync<TResult>();

        if (result is null)
        {
            throw new NullReferenceException(nameof(result));
        }

        OnRequested(result);

        return result;
    }
}