namespace Utils.Http.Exceptions;

public class ConnectionException : HttpServiceException
{
    public ConnectionException() : base()
    {
    }

    public ConnectionException(string message) : base(message)
    {
    }

    public ConnectionException(string message, Exception inner) : base(message, inner)
    {
    }
}