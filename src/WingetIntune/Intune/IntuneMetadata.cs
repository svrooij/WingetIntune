using System.Text;

namespace WingetIntune.Intune;

public class IntuneMetadata
{
    private const string MainFolder = "IntuneWinPackage";
    private const string MetadataFolder = "Metadata";
    private const string MetadataFilename = "Detection.xml";
    private const string ContentsFolder = "Contents";
    private const string ContentsFilename = "IntunePackage.intunewin";

    public static ApplicationInfo? GetApplicationInfo(byte[] data)
    {
        var xml = Encoding.UTF8.GetString(data);
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ApplicationInfo));
        using var reader = new System.IO.StringReader(xml);
        return (ApplicationInfo)serializer.Deserialize(reader);
    }

    public static string GetMetadataPath(string extractedFolderPath) => System.IO.Path.Combine(extractedFolderPath, MainFolder, MetadataFolder, MetadataFilename);

    public static string GetContentsPath(string extractedFolderPath) => System.IO.Path.Combine(extractedFolderPath, MainFolder, ContentsFolder, ContentsFilename);
}

// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class ApplicationInfo
{
    private string nameField;

    private uint unencryptedContentSizeField;

    private string fileNameField;

    private string setupFileField;

    private ApplicationInfoEncryptionInfo encryptionInfoField;

    private ApplicationInfoMsiInfo msiInfoField;

    private string toolVersionField;

    /// <remarks/>
    public string Name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    public uint UnencryptedContentSize
    {
        get
        {
            return this.unencryptedContentSizeField;
        }
        set
        {
            this.unencryptedContentSizeField = value;
        }
    }

    /// <remarks/>
    public string FileName
    {
        get
        {
            return this.fileNameField;
        }
        set
        {
            this.fileNameField = value;
        }
    }

    /// <remarks/>
    public string SetupFile
    {
        get
        {
            return this.setupFileField;
        }
        set
        {
            this.setupFileField = value;
        }
    }

    /// <remarks/>
    public ApplicationInfoEncryptionInfo EncryptionInfo
    {
        get
        {
            return this.encryptionInfoField;
        }
        set
        {
            this.encryptionInfoField = value;
        }
    }

    /// <remarks/>
    public ApplicationInfoMsiInfo MsiInfo
    {
        get
        {
            return this.msiInfoField;
        }
        set
        {
            this.msiInfoField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ToolVersion
    {
        get
        {
            return this.toolVersionField;
        }
        set
        {
            this.toolVersionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ApplicationInfoEncryptionInfo
{
    private string encryptionKeyField;

    private string macKeyField;

    private string initializationVectorField;

    private string macField;

    private string profileIdentifierField;

    private string fileDigestField;

    private string fileDigestAlgorithmField;

    /// <remarks/>
    public string EncryptionKey
    {
        get
        {
            return this.encryptionKeyField;
        }
        set
        {
            this.encryptionKeyField = value;
        }
    }

    /// <remarks/>
    public string MacKey
    {
        get
        {
            return this.macKeyField;
        }
        set
        {
            this.macKeyField = value;
        }
    }

    /// <remarks/>
    public string InitializationVector
    {
        get
        {
            return this.initializationVectorField;
        }
        set
        {
            this.initializationVectorField = value;
        }
    }

    /// <remarks/>
    public string Mac
    {
        get
        {
            return this.macField;
        }
        set
        {
            this.macField = value;
        }
    }

    /// <remarks/>
    public string ProfileIdentifier
    {
        get
        {
            return this.profileIdentifierField;
        }
        set
        {
            this.profileIdentifierField = value;
        }
    }

    /// <remarks/>
    public string FileDigest
    {
        get
        {
            return this.fileDigestField;
        }
        set
        {
            this.fileDigestField = value;
        }
    }

    /// <remarks/>
    public string FileDigestAlgorithm
    {
        get
        {
            return this.fileDigestAlgorithmField;
        }
        set
        {
            this.fileDigestAlgorithmField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ApplicationInfoMsiInfo
{
    private string msiProductCodeField;

    private string msiProductVersionField;

    private string msiPackageCodeField;

    private string msiUpgradeCodeField;

    private string msiExecutionContextField;

    private bool msiRequiresLogonField;

    private bool msiRequiresRebootField;

    private bool msiIsMachineInstallField;

    private bool msiIsUserInstallField;

    private bool msiIncludesServicesField;

    private bool msiIncludesODBCDataSourceField;

    private bool msiContainsSystemRegistryKeysField;

    private bool msiContainsSystemFoldersField;

    private string msiPublisherField;

    /// <remarks/>
    public string MsiProductCode
    {
        get
        {
            return this.msiProductCodeField;
        }
        set
        {
            this.msiProductCodeField = value;
        }
    }

    /// <remarks/>
    public string MsiProductVersion
    {
        get
        {
            return this.msiProductVersionField;
        }
        set
        {
            this.msiProductVersionField = value;
        }
    }

    /// <remarks/>
    public string MsiPackageCode
    {
        get
        {
            return this.msiPackageCodeField;
        }
        set
        {
            this.msiPackageCodeField = value;
        }
    }

    /// <remarks/>
    public string MsiUpgradeCode
    {
        get
        {
            return this.msiUpgradeCodeField;
        }
        set
        {
            this.msiUpgradeCodeField = value;
        }
    }

    /// <remarks/>
    public string MsiExecutionContext
    {
        get
        {
            return this.msiExecutionContextField;
        }
        set
        {
            this.msiExecutionContextField = value;
        }
    }

    /// <remarks/>
    public bool MsiRequiresLogon
    {
        get
        {
            return this.msiRequiresLogonField;
        }
        set
        {
            this.msiRequiresLogonField = value;
        }
    }

    /// <remarks/>
    public bool MsiRequiresReboot
    {
        get
        {
            return this.msiRequiresRebootField;
        }
        set
        {
            this.msiRequiresRebootField = value;
        }
    }

    /// <remarks/>
    public bool MsiIsMachineInstall
    {
        get
        {
            return this.msiIsMachineInstallField;
        }
        set
        {
            this.msiIsMachineInstallField = value;
        }
    }

    /// <remarks/>
    public bool MsiIsUserInstall
    {
        get
        {
            return this.msiIsUserInstallField;
        }
        set
        {
            this.msiIsUserInstallField = value;
        }
    }

    /// <remarks/>
    public bool MsiIncludesServices
    {
        get
        {
            return this.msiIncludesServicesField;
        }
        set
        {
            this.msiIncludesServicesField = value;
        }
    }

    /// <remarks/>
    public bool MsiIncludesODBCDataSource
    {
        get
        {
            return this.msiIncludesODBCDataSourceField;
        }
        set
        {
            this.msiIncludesODBCDataSourceField = value;
        }
    }

    /// <remarks/>
    public bool MsiContainsSystemRegistryKeys
    {
        get
        {
            return this.msiContainsSystemRegistryKeysField;
        }
        set
        {
            this.msiContainsSystemRegistryKeysField = value;
        }
    }

    /// <remarks/>
    public bool MsiContainsSystemFolders
    {
        get
        {
            return this.msiContainsSystemFoldersField;
        }
        set
        {
            this.msiContainsSystemFoldersField = value;
        }
    }

    /// <remarks/>
    public string MsiPublisher
    {
        get
        {
            return this.msiPublisherField;
        }
        set
        {
            this.msiPublisherField = value;
        }
    }
}