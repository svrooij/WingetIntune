namespace WingetIntune.Extensions;

internal static class StringArrayExtensions
{
    internal static bool ContainsIgnoreCase(this IEnumerable<string> source, string value)
    {
        return source.Any(x => x.Equals(value, StringComparison.InvariantCultureIgnoreCase));
    }

    internal static string? GetValue(this IEnumerable<string> source, string key)
    {
        if (key is null) return null;
        return source.FirstOrDefault(x => x.StartsWith($"{key}:", StringComparison.InvariantCultureIgnoreCase))?.Substring(key.Length + 1).Trim();
    }

    internal static Uri? GetUri(this IEnumerable<string> source, string key)
    {
        var value = source.GetValue(key);
        if (value is null) { return null; }
        return new Uri(value);
    }

    internal static string? GetValueContains(this IEnumerable<string> source, string key)
    {
        return source.FirstOrDefault(x => x.Contains(key, StringComparison.InvariantCultureIgnoreCase))?.TrimStart().Substring(key.Length + 1).Trim();
    }

    internal static string? GetMultiLineValue(this string[] source, string key)
    {
        var index = Array.FindIndex(source, l => l.StartsWith(key));
        if (index == -1) { return null; }
        var value = source[index].Substring(key.Length + 1).Trim();
        var lines = source.Skip(index + 1).TakeWhile(l => l.StartsWith("  ") || l == "");
        if (lines.Any())
        {
            value += $"{Environment.NewLine}" + string.Join(Environment.NewLine, lines.Select(l => l.Trim()));
        }
        return value;
    }
}
