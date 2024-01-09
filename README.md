# WinTuner CLI

[![GitHub issues](https://img.shields.io/github/issues/svrooij/wingetintune?style=for-the-badge)](https://github.com/svrooij/WingetIntune/issues)
[![Github sponsors](https://img.shields.io/github/sponsors/svrooij?style=for-the-badge&logo=github&logoColor=white)](https://github.com/sponsors/svrooij)

[Documentation](https://wintuner.app/)

Take any (just msi installers for now) app from winget and upload it to Intune in minutes.

- Downloading the installer and the logo
- Creating an `intunewin` file automatically
- Generating the needed script information
- Publish the app to Intune.

This application ~~is **Windows only** and~~ requires **Dotnet 7** to be installed on your computer. It's a [beta application](#beta-application), so please report any issues you find.
Some commands run the `winget` in the background and are thus Windows-only, make sure you have the [App Installer](https://www.microsoft.com/p/app-installer/9nblggh4nns1) installed on your computer if you want to use these commands.

The `package` and `publish` commands are cross-platform, and should work on any platform that supports dotnet 7. These commands no longer use the winget executable, which also means any other sources than `winget` are no longer supported.
The `msi` command is still windows only, as it uses the `Microsoft.Deployment.WindowsInstaller` package.

[![LinkedIn Profile][badge_linkedin]][link_linkedin]
[![Link Mastodon][badge_mastodon]][link_mastodon]
[![Follow on Twitter][badge_twitter]][link_twitter]
[![Check my blog][badge_blog]][link_blog]

## Installing

This package can be downloaded as a dotnet tool. Make sure you have Dotnet 7 installed on your computer.
I'm working to get a code signing certificate, but for now you might have to configure an exception on your computer to run unsigned code.

```Shell
# Install dotnet 7 sdk (or the way specific for your platform)
winget install --id Microsoft.DotNet.SDK.7 --source winget

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

## Commands

The CLI has several commands, try them out yourself.

```Shell
Description:
  wintuner by @svrooij allows you to package any winget app for Intune

Usage:
  wintuner [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  package <packageId>  Package an app for Intune (cross platform)
  publish <packageId>  Publish a packaged app to Intune (cross platform)
  about                Information about this package and it's author (cross platform)
  install <packageId>  Installs or upgrades a package (Windows-only)
  check <packageId>    Check if a specific version in installed (Windows-only)
  info <packageId>     Show package info as json (Windows-only)
  msi <msiFile>        Extract info from MSI file (Windows-only)

```

### Package

You should definitely try the `package` command. as it's the most important one. Package an app from winget as an `intunewin` file, ready for uploading to intune.
You can also expect a `detection.ps1` file, that you should configure to be used as detection script in Intune, provided it's not a MSI installer, in that case you can find the detection information in the readme.
It will also write a `app.json` file with all the information about the app, for automation purposes.

```Shell
wintuner package {PackageId} [--version {version}] [--source winget] --package-folder {PackageFolder}
```

> The `packageId` ~~is case sensitive, so make sure you use the correct casing~~ will be matches against any package in the open source [index](https://wintuner.app/docs/related/winget-package-index). Tip: Copy it from the result of the `winget search {name}` command.

Upon downloading the installer, the SHA256 hash is checked against the one in the `winget` manifest, to make sure you won't package a tampered installer.

The packaging command uses an open-source & cross-platform [implementation](https://wintuner.app/docs/related/content-prep-tool) of the Windows-only & closed source [content-prep-tool](https://github.com/Microsoft/Microsoft-Win32-Content-Prep-Tool), to allow cross-platform building of the packages.
This new implementation is available as a dotnet library and a PowerShell module, so if you're into Intune packaging, check it out.

### Publish

The `publish` command will upload the `intunewin` file to Intune. You'll need to run the [package](#package) command first.
Not all packages will work for publishing, you can always try to manually upload the `intunewin` file to [Intune](https://endpoint.microsoft.com/#view/Microsoft_Intune_DeviceSettings/AppsWindowsMenu/~/windowsApps).

```Shell
# This app uses the built-in windows authentication, this will trigger a login prompt (or do sso).
wintuner publish {PackageId} --package-folder {PackageFolder} --tenant {TenantId} --username {Username}

# You can also provide a token, this is useful for automation.
wintuner publish {PackageId} --package-folder {PackageFolder} --token {Token}
```

#### Assignement and categories

You can also assign the app to a group, and set the categories.

```Shell
# Add --category "Productivity" --category "Utilities" to the command to set the categories (use the exact name!)
wintuner publish {PackageId} ... --category "Productivity" --category "Utilities"

# Add --available "group-guid" to make the app available to a group (use the guid of the group)
# Add --available "allusers" to make the app available to all users
# Instead of --available you can also use --required to make the app required for the group
# Or if you want to remove the app for that group, use --uninstall
wintuner publish {PackageId}... --required "3bac8336-623f-46bf-bcab-b5c61e3e5b7a" --required "allusers"
wintuner publish {PackageId}... --uninstall "3bac8336-623f-46bf-bcab-b5c61e3e5b7a" --uninstall "allusers"
```

#### Auto-package

You can also combine the `package` and `publish` command into one command, this will package the app and publish it to Intune. But this makes debugging harder, so when submitting issues, please don't use this option.

```Shell
wintuner publish {PackageId}... --auto-package
```

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