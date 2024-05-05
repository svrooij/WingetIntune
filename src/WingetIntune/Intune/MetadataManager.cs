using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WingetIntune.Models;

namespace WingetIntune.Intune;
public class MetadataManager
{
    private readonly ILogger<MetadataManager> logger;
    private readonly IFileManager fileManager;
    private readonly Mapper mapper = new();

    public MetadataManager(ILogger<MetadataManager> logger, IFileManager fileManager)
    {
        this.logger = logger;
        this.fileManager = fileManager;
    }

    public async Task<PackageInfo> LoadPackageInfoFromFolderAsync(string folder, CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(folder);
#endif
        var filename = Path.Combine(folder, "app.json");
        if (!fileManager.FileExists(filename))
        {
            throw new FileNotFoundException($"Could not find app.json in folder {folder}", filename);
        }

        logger.LogDebug("Loading package info from {filename}", filename);

        var data = await fileManager.ReadAllBytesAsync(filename, cancellationToken);
        return JsonSerializer.Deserialize<PackageInfo>(data, MyJsonContext.Default.PackageInfo)!;
    }

    public Task<PackageInfo> LoadPackageInfoFromFolderAsync(string rootFolder, string packageId, string version, CancellationToken cancellationToken) =>
        LoadPackageInfoFromFolderAsync(Path.Combine(rootFolder, packageId, version), cancellationToken);

    public Win32LobApp ConvertPackageInfoToWin32App(PackageInfo packageInfo)
    {
        logger.LogDebug("Converting package info to Win32App");
        var win32App = mapper.ToWin32LobApp(packageInfo);
        return win32App;
    }

    public string GetIntuneWinFileName(string packageFolder, PackageInfo packageInfo)
    {
        return Path.Combine(packageFolder, Path.GetFileNameWithoutExtension(packageInfo.InstallerFilename!) + ".intunewin");
    }
}
