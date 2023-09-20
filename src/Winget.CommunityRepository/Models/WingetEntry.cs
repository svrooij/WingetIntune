using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winget.CommunityRepository.Models;
public class WingetEntry
{
    internal string? Name { get; set; }
    public required string PackageId { get; set; }
    public string? Version { get; set; }
}
