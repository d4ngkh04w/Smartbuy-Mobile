using System.Net;

namespace api.Exceptions
{
    public class UnauthorizedException(string message = "You are not authorized") : ApiException(message, HttpStatusCode.Unauthorized)
    {
    }
}