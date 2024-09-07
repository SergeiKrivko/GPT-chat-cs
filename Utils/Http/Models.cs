using System.Runtime.InteropServices.Marshalling;

namespace Utils.Http;

class ResponseBody<T>
{
    public string detail { get; set; }
    public T data { get; set; }
}