using eBRestarter.Core.Domain.Models.Records;

namespace eBRestarter.Core.Application.Interfaces;

public interface ILocalizationService
{
    string GetString(string key);
}
