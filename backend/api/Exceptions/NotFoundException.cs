using System.Net;

namespace api.Exceptions
{
    public class NotFoundException(string message) : ApiException(message, HttpStatusCode.NotFound)
    {
    }
}