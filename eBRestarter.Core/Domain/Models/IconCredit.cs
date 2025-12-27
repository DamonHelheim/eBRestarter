using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models
{
    public class IconCredit
    {
        public string IconCreator { get; set; } = string.Empty;
        public string IconImageSource { get; set; } = string.Empty;
        public string IconHyperLink { get; set; } = string.Empty;
        public string IconHyperLinkContent { get; set; } = string.Empty;
    }
}
