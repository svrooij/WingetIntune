
namespace WingetIntune.Internal.MsStore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class MicrosoftStoreManifest
{

    public string type { get; set; }
    public MicrosoftStoreManifestData Data { get; set; }
}

public class MicrosoftStoreManifestData
{
    public string type { get; set; }
    public string PackageIdentifier { get; set; }
    public MicrosoftStoreManifestVersion[] Versions { get; set; }
}

public class MicrosoftStoreManifestVersion
{
    public string type { get; set; }
    public string PackageVersion { get; set; }
    public MicrosoftStoreManifestDefaultlocale DefaultLocale { get; set; }
    public MicrosoftStoreManifestInstaller[] Installers { get; set; }
}

public class MicrosoftStoreManifestDefaultlocale
{
    public string type { get; set; }
    public string PackageLocale { get; set; }
    public string Publisher { get; set; }
    public string PublisherUrl { get; set; }
    public string PrivacyUrl { get; set; }
    public string PublisherSupportUrl { get; set; }
    public string PackageName { get; set; }
    public string License { get; set; }
    public string Copyright { get; set; }
    public string ShortDescription { get; set; }
    public string Description { get; set; }
    public object[] Tags { get; set; }
    public MicrosoftStoreManifestAgreement[] Agreements { get; set; }
}

public class MicrosoftStoreManifestAgreement
{
    public string type { get; set; }
    public string AgreementLabel { get; set; }
    public string Agreement { get; set; }
    public string AgreementUrl { get; set; }
}

public class MicrosoftStoreManifestInstaller
{
    public string type { get; set; }
    public string MSStoreProductIdentifier { get; set; }
    public string Architecture { get; set; }
    public string InstallerType { get; set; }
    public MicrosoftStoreManifestMarkets Markets { get; set; }
    public string PackageFamilyName { get; set; }
    public string Scope { get; set; }
}

public class MicrosoftStoreManifestMarkets
{
    public string type { get; set; }
    public string[] AllowedMarkets { get; set; }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
