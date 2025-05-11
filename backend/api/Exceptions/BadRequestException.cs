using System.Net;

namespace api.Exceptions;

public class BadRequestException(string message) : ApiException(message, HttpStatusCode.BadRequest)
{
}