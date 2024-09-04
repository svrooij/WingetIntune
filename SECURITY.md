[# Security Policy WinTuner

## Supported Versions

Use this section to tell people about which versions of your project are
currently being supported with security updates.

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |
| 1.0.1   | :x:                |

## Reporting a Vulnerability

The WinTuner team (@svrooij for now) and community take security bugs in WinTuner seriously.
We appreciate your efforts to responsibly disclose your findings, and will make every effort to acknowledge your contributions.

To report a security issue, please use the GitHub Security Advisory ["Report a Vulnerability"](https://github.com/svrooij/WingetIntune/security/advisories/new) tab.

We will send a response indicating the next steps in handling your report. We will keep you update regurlarly throughout the process.

### Multi-tenant application

Yes, this PowerShell module has a ClientID included which is registered to be a multi-tenant application. Microsoft is pushing back on these multi-tenant apps, but for convenience it's still included.
And only used during interactive authentication.

## Regular issues

If your found a bug or an issue, please create an [issue](https://github.com/svrooij/WingetIntune/issues) through the regular issue tracker.
