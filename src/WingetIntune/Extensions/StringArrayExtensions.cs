namespace WingetIntune.Extensions;

internal static class StringArrayExtensions
{
    public static bool ContainsIgnoreCase(this IEnumerable<string> source, string value)
    {
        return source.Any(x => x.Equals(value, StringComparison.InvariantCultureIgnoreCase));
    }
}
