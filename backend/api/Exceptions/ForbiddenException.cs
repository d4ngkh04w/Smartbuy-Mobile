using System.Net;

namespace api.Exceptions;

public class ForbiddenException(string message = "You do not have permission to access this resource") : ApiException(message, HttpStatusCode.Forbidden)
{
}