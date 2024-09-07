namespace Utils.Http.Exceptions;

public class HttpServiceException : Exception
{
    public HttpServiceException() : base()
    {
    }

    public HttpServiceException(string message) : base(message)
    {
    }

    public HttpServiceException(string message, Exception inner) : base(message, inner)
    {
    }
}