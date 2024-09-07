using System.Net;
using System.Net.Http.Json;
using Utils.Http.Exceptions;

namespace Utils.Http;

public class HttpService
{
    protected HttpClient Client = new HttpClient();

    public delegate void UnauthorizedRequestHandler();
    public event UnauthorizedRequestHandler? Unauthorized;

    private void ProcessStatusCode(HttpResponseMessage resp)
    {
        if (resp.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            Unauthorized?.Invoke();
            throw new UnauthorizedException();
        }
        if (!resp.IsSuccessStatusCode)
            throw new BadResponseCodeException();
    }

    private async Task<T> ProcessResponseBody<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<ResponseBody<T>>();
        if (body == null)
            throw new UnprocessableResponseException();
        return body.data;
    }

    private async Task<T> ProcessResponse<T>(HttpResponseMessage response)
    {
        ProcessStatusCode(response);
        return await ProcessResponseBody<T>(response);
    }

    private async Task ProcessResponse(HttpResponseMessage response)
    {
        ProcessStatusCode(response);
        await ProcessResponseBody<int?>(response);
    }

    protected async Task<T> Post<T>(string url, BaseRequestBody? body)
    {
        var response = await Client.PostAsync(url, body == null ? null : JsonContent.Create(body));
        return await ProcessResponse<T>(response);
    }

    protected async Task Post(string url, BaseRequestBody? body)
    {
        var response = await Client.PostAsync(url, body == null ? null : JsonContent.Create(body));
        await ProcessResponse(response);
    }

    protected async Task<T> Get<T>(string url)
    {
        var response = await Client.GetAsync(url);
        return await ProcessResponse<T>(response);
    }

    protected async Task<T> Put<T>(string url, BaseRequestBody? body)
    {
        var response = await Client.PutAsync(url, body == null ? null : JsonContent.Create(body));
        return await ProcessResponse<T>(response);
    }

    protected async Task Put(string url, BaseRequestBody? body)
    {
        var response = await Client.PutAsync(url, body == null ? null : JsonContent.Create(body));
        await ProcessResponse(response);
    }

    protected async Task<T> Delete<T>(string url)
    {
        var response = await Client.DeleteAsync(url);
        return await ProcessResponse<T>(response);
    }

    protected async Task Delete(string url)
    {
        var response = await Client.DeleteAsync(url);
        await ProcessResponse(response);
    }
}