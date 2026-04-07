namespace eBRestarter.Core.Domain.Enums;

public enum ResponseCode
{
    GeneralExceptionError = -3,
    Error = -1,
    None = 0,
    Success = 1,
    RequestLimit = 10,
    HttpRE401 = 401,
    NoConnectionToServer = 404,
    HTTPTimeout = 408,
    HttpRE429 = 429,
    InternalServerError = 500,
    HttpRE500 = 501,
    HttpRE200 = 502
}
