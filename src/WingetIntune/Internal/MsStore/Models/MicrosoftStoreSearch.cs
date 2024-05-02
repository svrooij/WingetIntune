namespace WingetIntune.Internal.MsStore.Models;
public class MicrosoftStoreSearchRequest
{
    public required MicrosoftStoreSearchQuery Query { get; set; }
}

public class MicrosoftStoreSearchQuery
{
    public required string KeyWord { get; set; }
    public string MatchType { get; set; } = "Substring";
}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class MicrosoftStoreSearchResult
{
    public string type { get; set; }
    public MicrosoftStoreSearchData[] Data { get; set; }

    public override string ToString()
    {
        return $"[{nameof(MicrosoftStoreSearchResult)}] {Data.Length} results";
    }
}

public class MicrosoftStoreSearchData
{
    public string type { get; set; }
    public string PackageIdentifier { get; set; }
    public string PackageName { get; set; }
    public string Publisher { get; set; }
    public MicrosoftStoreManifestVersion[] Versions { get; set; }
}

public class MicrosoftStoreSearchVersion
{
    public string type { get; set; }
    public string PackageVersion { get; set; }
    public string[] PackageFamilyNames { get; set; }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
