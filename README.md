# Winget Intune packager CLI

Take any (just msi installers for now) app from winget and upload it to Intune in minutes.

- Downloading the installer and the logo
- Creating an `intunewin` file automatically
- Generating the needed script information

## Installing

This package can be downloaded as a dotnet tool. Will show the correct install information here.

## Commands

The CLI has several commands, try them out yourself.

## Package

You should definitely try the `package` command. as it's the most important one. Package an app from winget as an `intunewin` file, ready for uploading to intune.

```bash
Description:
  Package an app for Intune

Usage:
  WingetIntune.Cli package [<packageId>] [options]

Arguments:
  <packageId>  Package identifier

Options:
  -v, --version <version>                 Package Version
  -s, --source <source>                   Package source
  --temp-folder <temp-folder> (REQUIRED)  Folder to store temporaty files [default:
                                          ...\Temp\intunewin]
  --output-folder <output-folder>         Output folder for the package
  -?, -h, --help                          Show help and usage information
```

## Library

You can also check-out the library if you want in integrate this into your own app.
