namespace SmartMetroService.Application.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class AlreadyExistsException : ApiException
{
    public AlreadyExistsException(string message) : base(message, 409)
    {
    }
}

public class ValidationException : ApiException
{
    public ValidationException(string message) : base(message, 400)
    {
    }
}

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message) : base(message, 401)
    {
    }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string message) : base(message, 404)
    {
    }
}

public class OtpException : ApiException
{
    public OtpException(string message) : base(message, 400)
    {
    }
}
