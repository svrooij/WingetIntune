using WingetIntune.Implementations;
using WingetIntune.Models;

namespace WingetIntune.Tests;

public class WingetHelperTests
{
    [Theory]
    [InlineData(null, true, InstallerContext.System, "install --id my-fake-id --source winget --force --silent --accept-package-agreements --accept-source-agreements --scope machine")]
    [InlineData(null, false, InstallerContext.System, "install --id my-fake-id --source winget --silent --accept-package-agreements --accept-source-agreements --scope machine")]
    [InlineData(null, true, InstallerContext.User, "install --id my-fake-id --source winget --force --silent --accept-package-agreements --accept-source-agreements --scope user")]
    [InlineData(null, false, InstallerContext.User, "install --id my-fake-id --source winget --silent --accept-package-agreements --accept-source-agreements --scope user")]
    [InlineData(null, true, InstallerContext.Unknown, "install --id my-fake-id --source winget --force --silent --accept-package-agreements --accept-source-agreements")]
    [InlineData(null, false, InstallerContext.Unknown, "install --id my-fake-id --source winget --silent --accept-package-agreements --accept-source-agreements")]
    [InlineData("42.0.0", true, InstallerContext.System, "install --id my-fake-id --version 42.0.0 --source winget --force --silent --accept-package-agreements --accept-source-agreements --scope machine")]
    [InlineData("42.0.0", false, InstallerContext.System, "install --id my-fake-id --version 42.0.0 --source winget --silent --accept-package-agreements --accept-source-agreements --scope machine")]
    [InlineData("42.0.0", true, InstallerContext.User, "install --id my-fake-id --version 42.0.0 --source winget --force --silent --accept-package-agreements --accept-source-agreements --scope user")]
    [InlineData("42.0.0", false, InstallerContext.User, "install --id my-fake-id --version 42.0.0 --source winget --silent --accept-package-agreements --accept-source-agreements --scope user")]
    [InlineData("42.0.0", true, InstallerContext.Unknown, "install --id my-fake-id --version 42.0.0 --source winget --force --silent --accept-package-agreements --accept-source-agreements")]
    [InlineData("42.0.0", false, InstallerContext.Unknown, "install --id my-fake-id --version 42.0.0 --source winget --silent --accept-package-agreements --accept-source-agreements")]
    public void GetInstallArgumentsForPackage_ReturnsCorrectString(string? version, bool force, InstallerContext installerContext, string expected)
    {
        var packageId = "my-fake-id";

        var result = WingetHelper.GetInstallArgumentsForPackage(packageId, version, force: force, installerContext: installerContext);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetShowArgumentsForPackage_ReturnsCorrectString()
    {
        var packageId = "my-fake-id";
        var version = "42.0.0";

        var result = WingetHelper.GetShowArgumentsForPackage(packageId, version);

        Assert.Equal("show --id my-fake-id --version 42.0.0 --source winget --exact --accept-source-agreements", result);
    }

    [Theory]
    [InlineData(true, InstallerContext.System, "uninstall --id my-fake-id --source winget --force --silent --accept-source-agreements --scope machine")]
    [InlineData(false, InstallerContext.System, "uninstall --id my-fake-id --source winget --silent --accept-source-agreements --scope machine")]
    [InlineData(true, InstallerContext.User, "uninstall --id my-fake-id --source winget --force --silent --accept-source-agreements --scope user")]
    [InlineData(false, InstallerContext.User, "uninstall --id my-fake-id --source winget --silent --accept-source-agreements --scope user")]
    [InlineData(true, InstallerContext.Unknown, "uninstall --id my-fake-id --source winget --force --silent --accept-source-agreements")]
    [InlineData(false, InstallerContext.Unknown, "uninstall --id my-fake-id --source winget --silent --accept-source-agreements")]
    public void GetUninstallArgumentsForPackage_ReturnsCorrectString(bool force, InstallerContext installerContext, string expected)
    {
        var packageId = "my-fake-id";

        var result = WingetHelper.GetUninstallArgumentsForPackage(packageId, force: force, installerContext: installerContext);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, true, InstallerContext.System, "upgrade --id my-fake-id --source winget --force --silent --accept-package-agreements --accept-source-agreements --scope machine")]
    [InlineData(null, false, InstallerContext.System, "upgrade --id my-fake-id --source winget --silent --accept-package-agreements --accept-source-agreements --scope machine")]
    [InlineData(null, true, InstallerContext.User, "upgrade --id my-fake-id --source winget --force --silent --accept-package-agreements --accept-source-agreements --scope user")]
    [InlineData(null, false, InstallerContext.User, "upgrade --id my-fake-id --source winget --silent --accept-package-agreements --accept-source-agreements --scope user")]
    [InlineData(null, true, InstallerContext.Unknown, "upgrade --id my-fake-id --source winget --force --silent --accept-package-agreements --accept-source-agreements")]
    [InlineData(null, false, InstallerContext.Unknown, "upgrade --id my-fake-id --source winget --silent --accept-package-agreements --accept-source-agreements")]
    [InlineData("42.0.0", true, InstallerContext.System, "upgrade --id my-fake-id --version 42.0.0 --source winget --force --silent --accept-package-agreements --accept-source-agreements --scope machine")]
    [InlineData("42.0.0", false, InstallerContext.System, "upgrade --id my-fake-id --version 42.0.0 --source winget --silent --accept-package-agreements --accept-source-agreements --scope machine")]
    [InlineData("42.0.0", true, InstallerContext.User, "upgrade --id my-fake-id --version 42.0.0 --source winget --force --silent --accept-package-agreements --accept-source-agreements --scope user")]
    [InlineData("42.0.0", false, InstallerContext.User, "upgrade --id my-fake-id --version 42.0.0 --source winget --silent --accept-package-agreements --accept-source-agreements --scope user")]
    [InlineData("42.0.0", true, InstallerContext.Unknown, "upgrade --id my-fake-id --version 42.0.0 --source winget --force --silent --accept-package-agreements --accept-source-agreements")]
    [InlineData("42.0.0", false, InstallerContext.Unknown, "upgrade --id my-fake-id --version 42.0.0 --source winget --silent --accept-package-agreements --accept-source-agreements")]
    public void GetUpgradeArgumentsForPackage_ReturnsCorrectString(string? version, bool force, InstallerContext installerContext, string expected)
    {
        var packageId = "my-fake-id";

        var result = WingetHelper.GetUpgradeArgumentsForPackage(packageId, version, force: force, installerContext: installerContext);

        Assert.Equal(expected, result);
    }
}
