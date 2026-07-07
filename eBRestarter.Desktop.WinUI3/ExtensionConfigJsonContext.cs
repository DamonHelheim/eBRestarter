using System.Text.Json.Serialization;
using eBRestarter.Infrastructure.Models.Config;

namespace eBRestarter.Desktop.WinUI3;

[JsonSerializable(typeof(ExtensionConfig))]
internal partial class ExtensionConfigJsonContext : JsonSerializerContext
{
}

