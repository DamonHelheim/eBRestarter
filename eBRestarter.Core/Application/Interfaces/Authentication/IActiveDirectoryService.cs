using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Net.Mime;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.Authentication
{
    public interface IActiveDirectoryService
    {
        bool ValidateCredentials(ContextType contextType, string domain, string username, string password);
    }
}
