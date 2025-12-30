using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.Browser
{
    public interface IBrowserDownloadService
    {
        Task DownloadFileAsync(
            string url,
            string destinationPath,
            IProgress<DownloadProgressStatus> progress,
            CancellationToken cancellationToken);
    }
}
