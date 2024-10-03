namespace eBRestarter.Classes.Enums
{
    public enum ResponseCode
    {
        Error = -1,
        None = 0,
        Success = 1,
        RequestLimit = 10,
        NoConnectionToServer = 404,
        HttpRE401 = 401,
        HttpRE429 = 429,
        HTTPTimeout = 408,
        InternalServerError = 500,
        GeneralExceptionError = -3
    }
}
