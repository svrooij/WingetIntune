using WingetIntune.Models;

namespace WingetIntune.Implementations;

public static class WingetHelper
{
    internal const string ArgDisableInteractivity = "--disable-interactivity";
    internal const string ArgExact = "--exact";
    internal const string ArgForce = "--force";
    internal const string ArgId = "--id";
    internal const string ArgPackageAgreements = "--accept-package-agreements";
    internal const string ArgScope = "--scope";
    internal const string ArgScopeSystem = "system";
    internal const string ArgScopeUser = "user";
    internal const string ArgSilent = "--silent";
    internal const string ArgSource = "--source";
    internal const string ArgSourceAgreements = "--accept-source-agreements";
    internal const string ArgVersion = "--version";
    internal const string ArgWinget = "winget";

    internal const string CmdInstall = "install";
    internal const string CmdShow = "show";
    internal const string CmdUninstall = "uninstall";
    internal const string CmdUpgrade = "upgrade";

    public static string GetInstallArgumentsForPackage(string packageId, string? version, string? source = ArgWinget, bool force = false, InstallerContext installerContext = InstallerContext.Unknown)
    {
        ArgumentNullException.ThrowIfNull(packageId);
        var args = new List<string>
        {
            CmdInstall,
            ArgId,
            packageId
        };

        if (!string.IsNullOrEmpty(version))
        {
            args.Add(ArgVersion);
            args.Add(version);
        }

        if (!string.IsNullOrEmpty(source))
        {
            args.Add(ArgSource);
            args.Add(source);
        }

        if (force)
        {
            args.Add(ArgForce);
        }

        args.Add(ArgSilent);
        args.Add(ArgPackageAgreements);
        args.Add(ArgSourceAgreements);
        args.Add(ArgDisableInteractivity);

        if (installerContext == InstallerContext.User)
        {
            args.Add(ArgScope);
            args.Add(ArgScopeUser);
        }
        else if (installerContext == InstallerContext.System)
        {
            args.Add(ArgScope);
            args.Add(ArgScopeSystem);
        }

        return string.Join(" ", args);
    }

    public static string GetShowArgumentsForPackage(string packageId, string? version, string? source = ArgWinget)
    {
        ArgumentNullException.ThrowIfNull(packageId);
        var args = new List<string>
        {
            CmdShow,
            ArgId,
            packageId
        };
        if (!string.IsNullOrEmpty(version))
        {
            args.Add(ArgVersion);
            args.Add(version);
        }
        if (!string.IsNullOrEmpty(source))
        {
            args.Add(ArgSource);
            args.Add(source);
        }
        args.Add(ArgExact);
        args.Add(ArgSourceAgreements);
        args.Add(ArgDisableInteractivity);
        return string.Join(" ", args);
    }

    public static string GetUninstallArgumentsForPackage(string packageId, string? source = ArgWinget, bool force = false, InstallerContext installerContext = InstallerContext.Unknown)
    {
        ArgumentNullException.ThrowIfNull(packageId);
        var args = new List<string>
        {
            CmdUninstall,
            ArgId,
            packageId
        };

        if (!string.IsNullOrEmpty(source))
        {
            args.Add(ArgSource);
            args.Add(source);
        }

        if (force)
        {
            args.Add(ArgForce);
        }

        args.Add(ArgSilent);
        args.Add(ArgSourceAgreements);
        args.Add(ArgDisableInteractivity);

        if (installerContext == InstallerContext.User)
        {
            args.Add(ArgScope);
            args.Add(ArgScopeUser);
        }
        else if (installerContext == InstallerContext.System)
        {
            args.Add(ArgScope);
            args.Add(ArgScopeSystem);
        }
        return string.Join(" ", args);
    }

    public static string GetUpgradeArgumentsForPackage(string packageId, string? version, string? source = ArgWinget, bool force = false, InstallerContext installerContext = InstallerContext.Unknown)
    {
        ArgumentNullException.ThrowIfNull(packageId);
        var args = new List<string>
        {
            CmdUpgrade,
            ArgId,
            packageId
        };

        if (!string.IsNullOrEmpty(version))
        {
            args.Add(ArgVersion);
            args.Add(version);
        }

        if (!string.IsNullOrEmpty(source))
        {
            args.Add(ArgSource);
            args.Add(source);
        }

        if (force)
        {
            args.Add(ArgForce);
        }

        args.Add(ArgSilent);
        args.Add(ArgPackageAgreements);
        args.Add(ArgSourceAgreements);
        args.Add(ArgDisableInteractivity);

        if (installerContext == InstallerContext.User)
        {
            args.Add(ArgScope);
            args.Add(ArgScopeUser);
        }
        else if (installerContext == InstallerContext.System)
        {
            args.Add(ArgScope);
            args.Add(ArgScopeSystem);
        }

        return string.Join(" ", args);
    }
}
