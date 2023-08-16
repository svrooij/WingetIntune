using WingetIntune.Models;

namespace WingetIntune;

public interface IWingetRepository
{
    Task<IsInstalledResult> CheckInstalled(string id, string? version, CancellationToken cancellationToken = default);

    Task<PackageInfo> GetPackageInfoAsync(string id, string? version, string? source, CancellationToken cancellationToken = default);

    Task<ProcessResult> Install(string id, string? version, string? source, bool force, CancellationToken cancellationToken = default);

    Task<ProcessResult> Upgrade(string id, string? version, string? source, bool force, CancellationToken cancellationToken = default);
}
