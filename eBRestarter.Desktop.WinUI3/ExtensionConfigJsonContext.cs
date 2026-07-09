using System.Text.Json.Serialization;
using eBRestarter.Infrastructure.ObjectArchetypes.DTOs;

namespace eBRestarter.Desktop.WinUI3;

[JsonSerializable(typeof(ExtensionConfig))]
internal partial class ExtensionConfigJsonContext : JsonSerializerContext;
