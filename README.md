# Winget Intune packager CLI

Take any (just msi installers for now) app from winget and upload it to Intune in minutes.

- Downloading the installer and the logo
- Creating an `intunewin` file automatically
- Generating the needed script information
- Publish the app to Intune.

## Installing

This package can be downloaded as a dotnet tool. Make sure you have Dotnet 7 installed on your computer.

```Shell
# This command will install the tool
dotnet tool install --global Svrooij.Winget-Intune.Cli

# or to update to the latest version
dotnet tool update --global SvRooij.Winget-Intune.Cli

```

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
```

## Package

You should definitely try the `package` command. as it's the most important one. Package an app from winget as an `intunewin` file, ready for uploading to intune.

```Shell
winget-intune package
Required argument missing for command: 'package'.

Description:
  Package an app for Intune

Usage:
  WingetIntune.Cli package [<packageId>] [command] [options]

Arguments:
  <packageId>  Package identifier

Options:
  -v, --version <version>                 Package Version
  -s, --source <source>                   Package source
  --temp-folder <temp-folder> (REQUIRED)  Folder to store temporaty files [default:
                                          C:\Users\stephan\AppData\Local\Temp\intunewin]
  --package-folder <package-folder>       Folder for the packaged apps
  -?, -h, --help                          Show help and usage information


Commands:
  image <image-path>  Convert an image to base64, to upload to Intune.
```

## Library

You can also check-out the library if you want in integrate this into your own app. (soon)
