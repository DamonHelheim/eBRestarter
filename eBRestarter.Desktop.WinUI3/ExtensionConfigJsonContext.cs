using System.Text.Json.Serialization;
using eBRestarter.Core.Domain.Entities;

namespace eBRestarter.Desktop.WinUI3;

[JsonSerializable(typeof(ExtensionConfigDto))]
internal partial class ExtensionConfigJsonContext : JsonSerializerContext
{
}
