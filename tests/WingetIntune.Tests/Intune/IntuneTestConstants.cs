using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingetIntune.Models;

namespace WingetIntune.Tests.Intune;
internal class IntuneTestConstants
{
    internal const string azureCliOutput = @"Found Microsoft Azure CLI [Microsoft.AzureCLI]
Version: 2.51.0
Publisher: Microsoft Corporation
Publisher Url: https://github.com/Azure/azure-cli
Publisher Support Url: https://github.com/Azure/azure-cli/issues
Description: The Azure command-line interface (Azure CLI) is a set of commands used to create and manage Azure resources. The Azure CLI is available across Azure services and is designed to get you working quickly with Azure, with an emphasis on automation.
Homepage: https://docs.microsoft.com/en-us/cli/azure/
License: MIT License
License Url: https://github.com/Azure/azure-cli/blob/release/LICENSE
Copyright: Copyright (C) Microsoft Corporation.
Copyright Url: https://github.com/Azure/azure-cli/blob/release/LICENSE
Release Notes Url: https://learn.microsoft.com/en-us/cli/azure/release-notes-azure-cli
Tags:
  azure
  cli
  Command-Line
Installer:
  Installer Type: msi
  Installer Url: https://azcliprod.azureedge.net/msi/azure-cli-2.51.0.msi
  Installer SHA256: 00f8f78b0e85ca0ba3966fef43fbe9d456ccc3cf7058a0cacc5d39399ccff905";

    internal static readonly PackageInfo azureCliPackageInfo = new PackageInfo
    {
        DisplayName = "Microsoft Azure CLI",
        PackageIdentifier = "Microsoft.AzureCLI",
        Description = "The Azure command-line interface (Azure CLI) is a set of commands used to create and manage Azure resources. The Azure CLI is available across Azure services and is designed to get you working quickly with Azure, with an emphasis on automation.",
        Version = "2.51.0",
        Source = PackageSource.Winget,
        Hash = "00f8f78b0e85ca0ba3966fef43fbe9d456ccc3cf7058a0cacc5d39399ccff905",
        Publisher = "Microsoft Corporation",
        PublisherUrl = new Uri("https://github.com/Azure/azure-cli"),
        SupportUrl = new Uri("https://github.com/Azure/azure-cli/issues"),
        Installers = new List<Models.Manifest.WingetInstaller>
        {
            new Models.Manifest.WingetInstaller
            {
                Architecture = "x64",
                Scope = "system",
                InstallerType = "msi",
                InstallerUrl = "https://azcliprod.azureedge.net/msi/azure-cli-2.51.0-x64.msi",
            }
        },
        MsiProductCode = "{89E4C65D-96DD-435B-9BBB-EF1EAEF5B738}",
        MsiVersion = "2.51.0",
    };
}
