using System.Text.Json.Serialization;

namespace WingetIntune.Models;

[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PackageInfo))]
public partial class MyJsonContext : JsonSerializerContext
{
}