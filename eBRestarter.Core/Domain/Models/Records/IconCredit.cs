using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records
{
    public record IconCredit
    {
        public string IconCreator { get; set; } = string.Empty;
        public string IconImageSource { get; init; } = string.Empty;
        public string IconHyperLink { get; init; } = string.Empty;
        public string IconHyperLinkContent { get; init; } = string.Empty;
    }
}
