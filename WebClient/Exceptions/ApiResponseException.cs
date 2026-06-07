using Domain.Exceptions;

namespace WebClient.Exceptions;

public class ApiResponseException : Exception
{
    public MessageError? Error { get; }

    public ApiResponseException(string message, MessageError? error = null)
        : base(message)
    {
        Error = error;
    }
}
