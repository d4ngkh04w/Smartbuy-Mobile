using System.Net;

namespace api.Exceptions
{
    public class ServerException(string message = "An unexpected error occurred") : ApiException(message, HttpStatusCode.InternalServerError)
    {
    }
}