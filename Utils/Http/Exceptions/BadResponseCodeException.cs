namespace Utils.Http.Exceptions;

public class BadResponseCodeException : HttpServiceException
{
    public BadResponseCodeException() : base()
    {
    }

    public BadResponseCodeException(string message) : base(message)
    {
    }

    public BadResponseCodeException(string message, Exception inner) : base(message, inner)
    {
    }
}