# WinTuner

[![Documentation badge](https://img.shields.io/badge/Show_Documentation-darkblue?style=for-the-badge)](https://wintuner.app/)
[![PowerShell gallery version][badge_powershell]][link_powershell]
[![License][badge_license]][link_license]
[![GitHub issues](https://img.shields.io/github/issues/svrooij/wingetintune?style=for-the-badge)](https://github.com/svrooij/WingetIntune/issues)
[![Github sponsors](https://img.shields.io/github/sponsors/svrooij?style=for-the-badge&logo=github&logoColor=white)](https://github.com/sponsors/svrooij)

[![WinTuner Mascot](https://wintuner.app/img/wintuner-mascotte-two_100.png)](https://wintuner.app/)

Take any app from WinGet and upload it to Intune in minutes. This app is available as [PowerShell module](#wintuner-powershell-module)

- Downloading the installer and the logo
- Creating an `intunewin` file automatically
- Generating the needed script information
- Publish the app to Intune.

[![LinkedIn Profile][badge_linkedin]][link_linkedin]
[![Follow on BlueSky][badge_bsky]][link_bsky]
[![Check my blog][badge_blog]][link_blog]

> [!NOTE]
> This application is depreceated, please move to the [PowerShell module](https://wintuner.app/docs/category/wintuner-powershell).

## WinTuner PowerShell Module

[![PowerShell gallery version][badge_powershell]][link_powershell]
[![PowerShell gallery downloads][badge_powershell_downloads]][link_powershell]

This is the PowerShell version of the WinTuner application, requiring PowerShell `7.4` (net8.0). Available in the [PowerShell Gallery](https://www.powershellgallery.com/packages/WinTuner/). Documentation can be found [here](https://wintuner.app/docs/category/wintuner-powershell).

```PowerShell
Install-Module -Name WinTuner
```

## Beta application

This is a beta application, it's not yet ready for production use. I'm still working on it, and I'm looking for feedback.
If you found a bug please create an [issue](https://github.com/svrooij/WingetIntune/issues/new/choose), if you have questions or want to share your feedback, check out the [discussions](https://github.com/svrooij/WingetIntune/discussions) page.

## Contributing

If you want to contribute to this project, please check out the [contributing](https://github.com/svrooij/WingetIntune/blob/main/CONTRIBUTING.md) page and the [Code of Conduct](https://github.com/svrooij/WingetIntune/blob/main/CODE_OF_CONDUCT.md).

## Useful information

- [WinTuner website](https://wintuner.app/)
- [Blog articles on Intune](https://svrooij.io/tags/intune/)
- Open-source [winget index](https://wintuner.app/docs/related/winget-package-index)
- Open-source [PowerShell Content Prep](https://wintuner.app/docs/related/content-prep-tool)

[badge_blog]: https://img.shields.io/badge/blog-svrooij.io-blue?style=for-the-badge
[badge_linkedin]: https://img.shields.io/badge/LinkedIn-stephanvanrooij-blue?style=for-the-badge&logo=linkedin
[badge_mastodon]: https://img.shields.io/mastodon/follow/109502876771613420?domain=https%3A%2F%2Fdotnet.social&label=%40svrooij%40dotnet.social&logo=mastodon&logoColor=white&style=for-the-badge
[badge_bsky]: https://img.shields.io/badge/Bluesky-svrooij.io-0285FF?style=for-the-badge&logo=bluesky&logoColor=white
[link_blog]: https://svrooij.io/
[link_linkedin]: https://www.linkedin.com/in/stephanvanrooij
[link_mastodon]: https://dotnet.social/@svrooij
[link_bsky]: https://bsky.app/profile/svrooij.io

[badge_license]: https://img.shields.io/github/license/svrooij/WingetIntune?style=for-the-badge
[link_license]: https://github.com/svrooij/WingetIntune/blob/main/LICENSE.txt
[badge_powershell]: https://img.shields.io/powershellgallery/v/WinTuner?style=for-the-badge&logo=powershell&logoColor=white
[badge_powershell_downloads]: https://img.shields.io/powershellgallery/dt/WinTuner?style=for-the-badge&logo=powershell&logoColor=white
[link_powershell]: https://www.powershellgallery.com/packages/WinTuner/
