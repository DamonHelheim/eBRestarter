using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models
{
    public class ApiRequest
    {
        public string Url { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public Enums.HttpMethod Method { get; set; }
        public int TimeoutSeconds { get; set; } = 60;

    }
}

