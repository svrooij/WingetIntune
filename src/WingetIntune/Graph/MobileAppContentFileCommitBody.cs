namespace WingetIntune.Graph;

public class MobileAppContentFileCommitBody
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FileEncryptionInfo FileEncryptionInfo { get; set; }
}

public class FileEncryptionInfo
{
    public string EncryptionKey { get; set; }
    public string InitializationVector { get; set; }
    public string Mac { get; set; }
    public string MacKey { get; set; }
    public string ProfileIdentifier { get; set; }
    public string FileDigest { get; set; }
    public string FileDigestAlgorithm { get; set; }
}
