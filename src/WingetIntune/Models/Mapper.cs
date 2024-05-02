﻿using Microsoft.Graph.Beta.Models;
using Riok.Mapperly.Abstractions;
using System.Text.RegularExpressions;
using WingetIntune.Internal.MsStore;
using WingetIntune.Intune;

namespace WingetIntune.Models;

[Mapper]
internal partial class Mapper
{
    public Win32LobApp ToWin32LobApp(PackageInfo packageInfo)
    {
        if (packageInfo is null)
        {
            throw new ArgumentNullException(nameof(packageInfo));
        }

        if (packageInfo.Source != PackageSource.Winget)
        {
            throw new NotSupportedException($"Package source {packageInfo.Source} is not supported");
        }

        var app = _ToWin32LobApp(packageInfo);
        app.DisplayVersion = packageInfo.Version;
        app.InstallExperience = new Win32LobAppInstallExperience()
        {
            RunAsAccount = packageInfo.InstallerContext == InstallerContext.User ? RunAsAccountType.User : RunAsAccountType.System,
            DeviceRestartBehavior = Win32LobAppRestartBehavior.BasedOnReturnCode
        };
        app.AllowAvailableUninstall = true;

        // This version of windows has the ability to install winget packages, if I'm not mistaken
        app.MinimumSupportedWindowsRelease = "2004";
        app.MinimumSupportedOperatingSystem = new WindowsMinimumOperatingSystem
        {
            V102004 = true
        };

        app.ReturnCodes = new List<Win32LobAppReturnCode>
            {
                new Win32LobAppReturnCode { Type = Win32LobAppReturnCodeType.Success, ReturnCode = 0 },
                new Win32LobAppReturnCode { Type = Win32LobAppReturnCodeType.Success, ReturnCode = 1707 },
                new Win32LobAppReturnCode { Type = Win32LobAppReturnCodeType.SoftReboot, ReturnCode = 3010 },
                new Win32LobAppReturnCode { Type = Win32LobAppReturnCodeType.HardReboot, ReturnCode = 1641 },
                new Win32LobAppReturnCode { Type = Win32LobAppReturnCodeType.Retry, ReturnCode = 1618 }
        };

        app.ApplicableArchitectures = ToGraphArchitecture(packageInfo.Architecture);

        if (packageInfo.MsiProductCode is not null && packageInfo.MsiVersion is not null) // packageInfo.InstallerType.IsMsi() 
        {
            // Not sure if this is correct, should this information always be set if available?
            if (packageInfo.InstallerType.IsMsi())
            {
                app.MsiInformation = new Win32LobAppMsiInformation
                {
                    ProductCode = packageInfo.MsiProductCode,
                    ProductVersion = packageInfo.MsiVersion,
                    Publisher = packageInfo.Publisher,
                    ProductName = packageInfo.DisplayName
                };
            }

            // Using ProductCode detection, is much faster then the detection script
            // if we have this information, we should use it.
            app.DetectionRules = new List<Win32LobAppDetection>
            {
                new Win32LobAppProductCodeDetection
                {
                    ProductCode = packageInfo.MsiProductCode,
                    ProductVersion = packageInfo.MsiVersion,
                    ProductVersionOperator = Win32LobAppDetectionOperator.GreaterThanOrEqual
                }
            };
        }
        else
        {
            app.DetectionRules = new List<Win32LobAppDetection>
            {
                new Win32LobAppPowerShellScriptDetection
                {
                    ScriptContent = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(packageInfo.DetectionScript!)),
                    EnforceSignatureCheck = false,
                    // Not sure if winget is even available on 32 bit systems
                    RunAs32Bit = packageInfo.Architecture == Architecture.X86
                }
            };
        }

        app.Developer = packageInfo.Publisher;
        app.FileName = Path.GetFileNameWithoutExtension(packageInfo.InstallerFilename) + ".intunewin";
        app.SetupFilePath = packageInfo.InstallerFilename ?? "install.ps1";
        app.Notes = $"Generated by {nameof(WingetIntune)} at {DateTimeOffset.UtcNow} [WinTuner|{packageInfo.Source}|{packageInfo.PackageIdentifier}]";
        return app;
    }

    private partial Win32LobApp _ToWin32LobApp(PackageInfo packageInfo);

    private static WindowsArchitecture ToGraphArchitecture(Architecture? architecture) => architecture switch
    {
        Architecture.Arm64 => WindowsArchitecture.Arm64,
        Architecture.X86 => WindowsArchitecture.X86 | WindowsArchitecture.X64,
        Architecture.X64 => WindowsArchitecture.X64,
        Architecture.Neutral => WindowsArchitecture.Neutral | WindowsArchitecture.X86 | WindowsArchitecture.X64 | WindowsArchitecture.Arm64,
        _ => WindowsArchitecture.None
    };

    public WinGetApp ToWinGetApp(MicrosoftStoreManifest storeManifest)
    {
        var locale = storeManifest.Data.Versions.LastOrDefault()?.DefaultLocale!;
        var app = _ToWinGetApp(locale);
        app.DisplayName = locale.PackageName;
        app.PackageIdentifier = storeManifest.Data.PackageIdentifier;
        app.InformationUrl = locale.PublisherSupportUrl?.ValidUriOrNull();
        app.PrivacyInformationUrl = locale.PrivacyUrl?.ValidUriOrNull();
        app.AdditionalData.Add("repositoryType", "microsoftstore");
        app.InstallExperience = new WinGetAppInstallExperience()
        {
            RunAsAccount = storeManifest.Data.Versions?.LastOrDefault()?.Installers?.LastOrDefault()?.Scope == "user" ? RunAsAccountType.User : RunAsAccountType.System,
        };
        app.Developer = app.Publisher;
        app.Description ??= locale.ShortDescription;
        app.Notes = $"Generated by WinTuner at {DateTimeOffset.UtcNow} [WinTuner|store|{storeManifest.Data.PackageIdentifier}]";
        return app;
    }

    public WinGetApp ToWinGetApp(DisplayCatalogResponse displayCatalogResponse)
    {
        var product = displayCatalogResponse.Products.FirstOrDefault();
        var displaySku = product?.DisplaySkuAvailabilities?.FirstOrDefault()?.Sku.LocalizedProperties.FirstOrDefault();
        var productProperties = product?.LocalizedProperties?.FirstOrDefault();
        var app = new WinGetApp();
        app.DisplayName = displaySku!.SkuTitle;
        app.PackageIdentifier = product!.ProductId;
        app.InformationUrl = productProperties?.SupportUri;
        app.PrivacyInformationUrl = productProperties?.PublisherWebsiteUri;
        app.Description = productProperties!.ProductDescription;
        app.AdditionalData.Add("repositoryType", "microsoftstore");
        app.InstallExperience = new WinGetAppInstallExperience()
        {
            RunAsAccount = RunAsAccountType.System,
        };
        app.Developer = app.Publisher = productProperties.PublisherName;
        app.Notes = $"Generated by WinTuner at {DateTimeOffset.UtcNow} [WinTuner|store|{product!.ProductId}]";
        return app;
    }

    private partial WinGetApp _ToWinGetApp(MicrosoftStoreManifestDefaultlocale locale);

    internal partial WingetIntune.Graph.FileEncryptionInfo ToFileEncryptionInfo(ApplicationInfoEncryptionInfo packageInfo);

    //internal partial Microsoft.Graph.Beta.Models.FileEncryptionInfo ToGraphEncryptionInfo(ApplicationInfoEncryptionInfo packageInfo);

    internal static IntuneApp ToIntuneApp(Win32LobApp? win32LobApp)
    {
        ArgumentNullException.ThrowIfNull(win32LobApp, nameof(win32LobApp));

        var (packageId, source) = win32LobApp.Notes.ExtractPackageIdAndSourceFromNotes();
        return new IntuneApp
        {
            PackageId = packageId!,
            Name = win32LobApp.DisplayName!,
            CurrentVersion = win32LobApp.DisplayVersion!,
            GraphId = win32LobApp.Id!,
            SupersededAppCount = win32LobApp.SupersededAppCount,
            SupersedingAppCount = win32LobApp.SupersedingAppCount,
            InstallerContext = win32LobApp.InstallExperience?.RunAsAccount == RunAsAccountType.User ? InstallerContext.User : InstallerContext.System,
            Architecture = win32LobApp.ApplicableArchitectures switch
            {
                WindowsArchitecture.Arm64 => Architecture.Arm64,
                WindowsArchitecture.X64 => Architecture.X64,
                WindowsArchitecture.X86 => Architecture.X86,
                _ => Architecture.Neutral
            }
        };
    }
}

internal static class MapperExtensions
{
    public static string? ValidUriOrNull(this string? input)
        => Uri.TryCreate(input, UriKind.Absolute, out var uri) && uri.Scheme.StartsWith("http") ? uri.ToString() : null;
}

internal static class StringExtensions
{
    internal static (string?, string?) ExtractPackageIdAndSourceFromNotes(this string? notes)
    {
        if (notes is null || !notes.Contains("[WingetIntune|"))
        {
            return (null, null);
        }

        var match = Regex.Match(notes, @"\[WingetIntune\|(?<source>[^\|]+)\|(?<packageId>[^\]]+)\]");
        if (match.Success)
        {
            return (match.Groups["packageId"].Value, match.Groups["source"].Value);
        }
        else
        {
            return (null, null);
        }
    }
}
