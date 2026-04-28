using System.Text.Json.Serialization;
using eBRestarter.Core.Application.Models.Config;

namespace eBRestarter.Desktop.WinUI3;

[JsonSerializable(typeof(ExtensionConfigDto))]
internal partial class ExtensionConfigJsonContext : JsonSerializerContext
{
}
