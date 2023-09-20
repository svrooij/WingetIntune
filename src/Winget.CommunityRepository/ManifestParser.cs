using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Winget.CommunityRepository;

public class ManifestParser
{
    private readonly IDeserializer deserializer;

    public ManifestParser() : this(BuildSerializer())
    {
    }

    public ManifestParser(IDeserializer deserializer)
    {
        this.deserializer = deserializer;
    }

    public Models.WingetMainManifest ParseMainManifest(string yaml)
    {
        var manifest = deserializer.Deserialize<Models.WingetMainManifest>(yaml);
        return manifest;
    }

    public Models.WingetInstallerManifest ParseInstallerManifest(string yaml)
    {
        var manifest = deserializer.Deserialize<Models.WingetInstallerManifest>(yaml);
        return manifest;
    }

    public Models.WingetLocalizedManifest ParseLocalizedManifest(string yaml)
    {
        var manifest = deserializer.Deserialize<Models.WingetLocalizedManifest>(yaml);
        return manifest;
    }

    private static IDeserializer BuildSerializer() => new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
}
