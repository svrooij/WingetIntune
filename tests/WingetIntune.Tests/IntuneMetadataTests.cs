using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingetIntune.Intune;

namespace WingetIntune.Tests;
public class IntuneMetadataTests
{

    [Fact]
    public void IntuneMetadata_Parse_ReturnsResult()
    {

        var result = IntuneMetadata.GetApplicationInfo(System.Text.Encoding.UTF8.GetBytes(DetectionsXml));
        Assert.NotNull(result);
        Assert.Equal("Microsoft Azure CLI (32-bit)", result.Name);
        Assert.Equal("azure-cli-2.51.0.msi", result.SetupFile);
        Assert.Equal("IntunePackage.intunewin", result.FileName);

        Assert.Equal("{89E4C65D-96DD-435B-9BBB-EF1EAEF5B738}", result.MsiInfo.MsiProductCode);
        Assert.Equal("2.51.0", result.MsiInfo.MsiProductVersion);
        Assert.Equal("{A721702D-9A23-499F-B48C-5166F83333C6}", result.MsiInfo.MsiPackageCode);
        Assert.Equal("{DFF82AF0-3F95-4AC9-8EFD-948604FDB028}", result.MsiInfo.MsiUpgradeCode);

        Assert.Equal("zItIfv4tKhpfVFEAHQRJBNPHWe1y0HbHIksD5VNwscg=", result.EncryptionInfo.EncryptionKey);
        Assert.Equal("H9DU/pt/cRPhDaGrriFTUYhQtSSomBXkRYGNY3zhwRQ=", result.EncryptionInfo.MacKey);
    }

    const string DetectionsXml = @"<ApplicationInfo xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" ToolVersion=""1.8.4.0"">
  <Name>Microsoft Azure CLI (32-bit)</Name>
  <UnencryptedContentSize>49718336</UnencryptedContentSize>
  <FileName>IntunePackage.intunewin</FileName>
  <SetupFile>azure-cli-2.51.0.msi</SetupFile>
  <EncryptionInfo>
    <EncryptionKey>zItIfv4tKhpfVFEAHQRJBNPHWe1y0HbHIksD5VNwscg=</EncryptionKey>
    <MacKey>H9DU/pt/cRPhDaGrriFTUYhQtSSomBXkRYGNY3zhwRQ=</MacKey>
    <InitializationVector>mPxbxFv1PeGxMHmsUYTLJw==</InitializationVector>
    <Mac>61FnckpV9Hla6CXLa2xUyzd2F5IaTO7KFln6RfOSSQ8=</Mac>
    <ProfileIdentifier>ProfileVersion1</ProfileIdentifier>
    <FileDigest>/O1hIjB5L/V2CqbNczKhHsUALm54xW6+jWRnIuKhuIM=</FileDigest>
    <FileDigestAlgorithm>SHA256</FileDigestAlgorithm>
  </EncryptionInfo>
  <MsiInfo>
    <MsiProductCode>{89E4C65D-96DD-435B-9BBB-EF1EAEF5B738}</MsiProductCode>
    <MsiProductVersion>2.51.0</MsiProductVersion>
    <MsiPackageCode>{A721702D-9A23-499F-B48C-5166F83333C6}</MsiPackageCode>
    <MsiUpgradeCode>{DFF82AF0-3F95-4AC9-8EFD-948604FDB028}</MsiUpgradeCode>
    <MsiExecutionContext>System</MsiExecutionContext>
    <MsiRequiresLogon>false</MsiRequiresLogon>
    <MsiRequiresReboot>false</MsiRequiresReboot>
    <MsiIsMachineInstall>true</MsiIsMachineInstall>
    <MsiIsUserInstall>false</MsiIsUserInstall>
    <MsiIncludesServices>false</MsiIncludesServices>
    <MsiIncludesODBCDataSource>false</MsiIncludesODBCDataSource>
    <MsiContainsSystemRegistryKeys>true</MsiContainsSystemRegistryKeys>
    <MsiContainsSystemFolders>false</MsiContainsSystemFolders>
    <MsiPublisher>Microsoft Corporation</MsiPublisher>
  </MsiInfo>
</ApplicationInfo>";

}
