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
/// <summary>
/// Makes it easier to work with wintuner metadata files.
/// </summary>
public class MetadataManager
{
    private readonly ILogger<MetadataManager> logger;
    private readonly IFileManager fileManager;
    private readonly Mapper mapper = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="fileManager"></param>
    public MetadataManager(ILogger<MetadataManager> logger, IFileManager fileManager)
    {
        this.logger = logger;
        this.fileManager = fileManager;
    }

    /// <summary>
    /// Loads the package info from a folder.
    /// </summary>
    /// <param name="folder">Folder where WinTuner placed a file</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<PackageInfo> LoadPackageInfoFromFolderAsync(string folder, CancellationToken cancellationToken)
    {
        logger.LogDebug("Loading package info from {folder}", folder);
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(folder);
#endif
        var filename = Path.Combine(folder, "app.json");
        if (!fileManager.FileExists(filename))
        {
            var ex = new FileNotFoundException($"Could not find app.json in folder {folder}", filename);
            logger.LogWarning(ex, "Could not find app.json in folder {folder}", folder);
            throw ex;
        }

        logger.LogDebug("Loading package info from {filename}", filename);

        var data = await fileManager.ReadAllBytesAsync(filename, cancellationToken);
        var result = JsonSerializer.Deserialize<PackageInfo>(data, MyJsonContext.Default.PackageInfo);
        if (result == null)
        {
            var ex = new InvalidOperationException($"Could not deserialize app.json");
            logger.LogWarning(ex, "Could not deserialize app.json in folder {folder}", folder);
            throw ex;
        }
        return result;
    }

    /// <summary>
    /// Loads the package info from a folder, with packageId and version
    /// </summary>
    /// <param name="rootFolder">The Root package filer</param>
    /// <param name="packageId">Package ID of previously packaged app</param>
    /// <param name="version">Version of the app</param>
    /// <param name="cancellationToken"></param>
    /// <remarks>Combines <paramref name="rootFolder"/>/<paramref name="packageId"/>/<paramref name="version"/> to a path to get the metadata from</remarks>
    public Task<PackageInfo> LoadPackageInfoFromFolderAsync(string rootFolder, string packageId, string version, CancellationToken cancellationToken) =>
        LoadPackageInfoFromFolderAsync(Path.Combine(rootFolder, packageId, version), cancellationToken);

    /// <summary>
    /// Converts a package info to a Win32App to upload to Graph
    /// </summary>
    /// <param name="packageInfo"></param>
    /// <returns></returns>
    public Win32LobApp ConvertPackageInfoToWin32App(PackageInfo packageInfo)
    {
        logger.LogDebug("Converting package info to Win32App");
        var win32App = mapper.ToWin32LobApp(packageInfo);
        return win32App;
    }

    /// <summary>
    /// Gets the IntuneWin file name from a package folder
    /// </summary>
    /// <param name="packageFolder"></param>
    /// <param name="packageInfo"></param>
    /// <returns></returns>
    public string GetIntuneWinFileName(string packageFolder, PackageInfo packageInfo)
    {
        return Path.Combine(packageFolder, Path.GetFileNameWithoutExtension(packageInfo.InstallerFilename!) + ".intunewin");
    }
}
