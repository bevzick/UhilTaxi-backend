namespace UhilTaxi.Application.Exceptions;
public sealed class AuthException(int status, string code, string message) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
}
