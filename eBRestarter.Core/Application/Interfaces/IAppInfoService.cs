using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces
{
    public interface IAppInfoService
    {
        string GetAppVersion();
        List<IconCredit> GetIconCredits();
    }
}
