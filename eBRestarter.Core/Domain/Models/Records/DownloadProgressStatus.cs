using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records
{
    public record DownloadProgressStatus(long BytesReceived, long TotalBytes, double Percentage);
}
