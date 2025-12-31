using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records
{
    public record AutoLogonCredentials(string Username, string Domain, string Password);
}
