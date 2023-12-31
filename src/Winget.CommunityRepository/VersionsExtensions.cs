﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winget.CommunityRepository;
internal static class VersionsExtensions
{
    internal static string? GetHighestVersion(this IEnumerable<string> versions)
    {
        if (versions is null || !versions.Any()) { return string.Empty; }
        return versions.Max(new VersionComparer());
    }
}

internal class VersionComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x is null && y is null) { return 0; }
        if (x is null) { return -1; }
        if (y is null) { return 1; }
        Version? xVersion = Version.TryParse(x, out var xVersionResult) ? xVersionResult : null;
        Version? yVersion = Version.TryParse(y, out var yVersionResult) ? yVersionResult : null;
        if (xVersion is null || yVersion is null) { return x.CompareTo(y); }
        return xVersion.CompareTo(yVersion);


    }
}
