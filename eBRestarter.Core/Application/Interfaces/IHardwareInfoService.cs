using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces
{
    public interface IHardwareInfoService
    {
        Task<HardwareInfo> GetHardwareInfoAsync();
    }
}
