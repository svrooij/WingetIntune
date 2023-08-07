using System.Text.Json.Serialization;

namespace WingetIntune.Models;

[JsonSerializable(typeof(PackageInfo))]
public partial class MyJsonContext : JsonSerializerContext
{
}