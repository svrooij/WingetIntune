# WinTuner

[![GitHub issues](https://img.shields.io/github/issues/svrooij/wingetintune?style=for-the-badge)](https://github.com/svrooij/WingetIntune/issues)
[![Github sponsors](https://img.shields.io/github/sponsors/svrooij?style=for-the-badge&logo=github&logoColor=white)](https://github.com/sponsors/svrooij)

[Documentation](https://wintuner.app/)

Take any app from WinGet and upload it to Intune in minutes. This app is available as [PowerShell module](#wintuner-powershell-module) and as a [CLI](#wintuner-cli), both run mostly thee same code.

- Downloading the installer and the logo
- Creating an `intunewin` file automatically
- Generating the needed script information
- Publish the app to Intune.

[![LinkedIn Profile][badge_linkedin]][link_linkedin]
[![Link Mastodon][badge_mastodon]][link_mastodon]
[![Follow on Twitter][badge_twitter]][link_twitter]
[![Check my blog][badge_blog]][link_blog]

## WinTuner PowerShell Module

This is the PowerShell version of the WinTuner application, requiring PowerShell `7.4` (net8.0). Available in the [PowerShell Gallery](https://www.powershellgallery.com/packages/WinTuner/). Documentation can be found [here](https://wintuner.app/docs/category/wintuner-powershell).

```PowerShell
Install-Module -Name WinTuner
```

As of April 2024, the main development focus will be on the PowerShell module, since that is what most sysadmin use. The CLI will still be maintained, but will not get new features as fast as the PowerShell module.

## WinTuner CLI

This application ~~is Windows only and~~ requires **Dotnet 8** to be installed on your computer. It's a [beta application](#beta-application), so please report any issues you find.
Some commands run the `winget` in the background and are thus Windows-only, make sure you have the [App Installer](https://www.microsoft.com/p/app-installer/9nblggh4nns1) installed on your computer if you want to use these commands.

The `package` and `publish` commands are cross-platform, and should work on any platform that supports dotnet 8. These commands no longer use the WinGet executable, which also means any other sources than `winget` are no longer supported.
The `msi` command is still windows only, as it uses the `Microsoft.Deployment.WindowsInstaller` package.

Check out the [documentation](https://wintuner.app/docs/category/wintuner-cli) for more information.

### Installing

This package can be downloaded as a dotnet tool. Make sure you have Dotnet 8 installed on your computer.
I'm working to get a code signing certificate, but for now you might have to configure an exception on your computer to run unsigned code.

```Shell
# Install dotnet 8 sdk (or the way specific for your platform)
winget install --id Microsoft.DotNet.SDK.8 --source winget

# Add the nuget feed, if that is not already done
dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget.org

# This command will install the tool
dotnet tool install --global Svrooij.Winget-Intune.Cli

# or to update to the latest version
dotnet tool update --global SvRooij.Winget-Intune.Cli

```

## Beta application

This is a beta application, it's not yet ready for production use. I'm still working on it, and I'm looking for feedback.
If you found a bug please create an [issue](https://github.com/svrooij/WingetIntune/issues/new/choose), if you have questions or want to share your feedback, check out the [discussions](https://github.com/svrooij/WingetIntune/discussions) page.

## Library (soon)

I'm planning to release the actual intune specific code as a separate library, so you can use it in your own projects. This will be released as a separate package.

## Contributing

If you want to contribute to this project, please check out the [contributing](https://github.com/svrooij/WingetIntune/blob/main/CONTRIBUTING.md) page and the [Code of Conduct](https://github.com/svrooij/WingetIntune/blob/main/CODE_OF_CONDUCT.md).

## Usefull information

- [WinTuner website](https://wintuner.app/)
- [Blog articles on Intune](https://svrooij.io/tags/intune/)
- Open-source [winget index](https://wintuner.app/docs/related/winget-package-index)
- Open-source [PowerShell Content Prep](https://wintuner.app/docs/related/content-prep-tool)

[badge_blog]: https://img.shields.io/badge/blog-svrooij.io-blue?style=for-the-badge
[badge_linkedin]: https://img.shields.io/badge/LinkedIn-stephanvanrooij-blue?style=for-the-badge&logo=linkedin
[badge_mastodon]: https://img.shields.io/mastodon/follow/109502876771613420?domain=https%3A%2F%2Fdotnet.social&label=%40svrooij%40dotnet.social&logo=mastodon&logoColor=white&style=for-the-badge
[badge_twitter]: https://img.shields.io/twitter/follow/svrooij?logo=twitter&style=for-the-badge
[link_blog]: https://svrooij.io/
[link_linkedin]: https://www.linkedin.com/in/stephanvanrooij
[link_mastodon]: https://dotnet.social/@svrooij
[link_twitter]: https://twitter.com/svrooij