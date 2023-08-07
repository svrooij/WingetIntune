using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WingetIntune.Models
{
    [JsonSerializable(typeof(PackageInfo))]
    internal partial class MyJsonContext : JsonSerializerContext
    {
    }
}
