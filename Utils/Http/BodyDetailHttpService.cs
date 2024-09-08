using System.Net.Http.Json;
using Newtonsoft.Json;
using Utils.Http.Exceptions;

namespace Utils.Http;

public class BodyDetailHttpService: HttpService
{
    protected override async Task<T> ProcessResponseBody<T>(HttpResponseMessage response)
    {
        var jsonString = await response.Content.ReadAsStringAsync();
        var body = JsonConvert.DeserializeObject<ResponseBody<T>>(jsonString);
        // var body = await response.Content.ReadFromJsonAsync<ResponseBody<T>>();
        if (body == null)
            throw new UnprocessableResponseException();
        return body.data;
    }
}