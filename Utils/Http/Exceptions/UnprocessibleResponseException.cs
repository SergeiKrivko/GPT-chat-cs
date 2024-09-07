namespace Utils.Http.Exceptions;

public class UnprocessableResponseException : HttpServiceException
{
    public UnprocessableResponseException() : base()
    {
    }

    public UnprocessableResponseException(string message) : base(message)
    {
    }

    public UnprocessableResponseException(string message, Exception inner) : base(message, inner)
    {
    }
}