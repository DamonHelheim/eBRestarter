using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Enums
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
