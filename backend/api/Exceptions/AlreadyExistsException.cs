using System.Net;

namespace api.Exceptions;

public class AlreadyExistsException(string message) : ApiException(message, HttpStatusCode.Conflict)
{
}