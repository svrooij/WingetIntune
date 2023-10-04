# Winget Intune packager CLI

[![GitHub issues](https://img.shields.io/github/issues/svrooij/wingetintune?style=for-the-badge)](https://github.com/svrooij/WingetIntune/issues)
[![Github sponsors](https://img.shields.io/github/sponsors/svrooij?style=for-the-badge&logo=github&logoColor=white)](https://github.com/sponsors/svrooij)

Take any (just msi installers for now) app from winget and upload it to Intune in minutes.

- Downloading the installer and the logo
- Creating an `intunewin` file automatically
- Generating the needed script information
- Publish the app to Intune.

This application is **Windows only** and requires **Dotnet 7** to be installed on your computer. It's also a [beta application](#beta-application), so please report any issues you find.
A lot of commands run the `winget` command, so make sure you have the [App Installer](https://www.microsoft.com/en-us/p/app-installer/9nblggh4nns1) installed on your computer as well.

[![LinkedIn Profile][badge_linkedin]][link_linkedin]
[![Link Mastodon][badge_mastodon]][link_mastodon]
[![Follow on Twitter][badge_twitter]][link_twitter]
[![Check my blog][badge_blog]][link_blog]

## Installing

This package can be downloaded as a dotnet tool. Make sure you have Dotnet 7 installed on your computer.
I'm working to get a code signing certificate, but for now you might have to configure an exception on your computer to run unsigned code.

```Shell
# Install dotnet 7 sdk
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
  Enhanced Winget CLI for automations

Usage:
  winget-intune [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  install <packageId>  Installs or upgrades a package
  check <packageId>    Check if a specific version in installed
  info <packageId>     Show package info as json
  package <packageId>  Package an app for Intune
  publish <packageId>  Publish an packaged app to Intune
  msi <msiFile>        Extract info from MSI file
  about                Information about this package and it's author
```

### Package

You should definitely try the `package` command. as it's the most important one. Package an app from winget as an `intunewin` file, ready for uploading to intune.
You can also expect a `detection.ps1` file, that you should configure to be used as detection script in Intune, provided it's not a MSI installer, in that case you can find the detection information in the readme.
It will also write a `app.json` file with all the information about the app, for automation purposes.

```Shell
winget-intune package {PackageId} [--version {version}] [--source winget] --package-folder {PackageFolder}
```

> The `packageId` is case sensitive, so make sure you use the correct casing. Tip: Copy it from the result of the `winget search {name}` command.

This command will download the [content-prep-tool](https://github.com/Microsoft/Microsoft-Win32-Content-Prep-Tool) automatically, and use it to create the `intunewin` file.
In a future version this might be replaced with a custom implementation, but for now this works. The SHA265 hash of the installer is checked and compared to the one in the `winget` manifest, to make sure you won't package a tampered installer.

### Publish

The `publish` command will upload the `intunewin` file to Intune. You'll need to run the [package](#package) command first.
Not all packages will work for publishing, you can always try to manually upload the `intunewin` file to [Intune](https://endpoint.microsoft.com/#view/Microsoft_Intune_DeviceSettings/AppsWindowsMenu/~/windowsApps).

```Shell
# This app uses the built-in windows authentication, this will trigger a login prompt (or do sso).
winget-intune publish {PackageId} --package-folder {PackageFolder} --tenant {TenantId} --username {Username}

# You can also provide a token, this is useful for automation.
winget-intune publish {PackageId} --package-folder {PackageFolder} --token {Token}
```

## Library (soon)

I'm planning to release the actual intune specific code as a separate library, so you can use it in your own projects. This will be released as a separate package.

## Contributing

If you want to contribute to this project, please check out the [contributing](https://github.com/svrooij/WingetIntune/blob/main/CONTRIBUTING.md) page and the [Code of Conduct](https://github.com/svrooij/WingetIntune/blob/main/CODE_OF_CONDUCT.md).

## Usefull information

- [Microsoft-Win32-Content-Prep-Tool](https://github.com/microsoft/Microsoft-Win32-Content-Prep-Tool)
- [Blog articles on Intune](https://svrooij.io/tags/intune/)

[badge_blog]: https://img.shields.io/badge/blog-svrooij.io-blue?style=for-the-badge
[badge_linkedin]: https://img.shields.io/badge/LinkedIn-stephanvanrooij-blue?style=for-the-badge&logo=linkedin
[badge_mastodon]: https://img.shields.io/mastodon/follow/109502876771613420?domain=https%3A%2F%2Fdotnet.social&label=%40svrooij%40dotnet.social&logo=mastodon&logoColor=white&style=for-the-badge
[badge_twitter]: https://img.shields.io/twitter/follow/svrooij?logo=twitter&style=for-the-badge
[link_blog]: https://svrooij.io/
[link_linkedin]: https://www.linkedin.com/in/stephanvanrooij
[link_mastodon]: https://dotnet.social/@svrooij
[link_twitter]: https://twitter.com/svrooij