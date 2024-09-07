using System.Net.Http.Json;
using Utils.Http.Exceptions;

namespace Utils.Http;

public class BodyDetailHttpService: HttpService
{
    private async Task<T> ProcessResponseBody<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<ResponseBody<T>>();
        if (body == null)
            throw new UnprocessableResponseException();
        return body.data;
    }
}