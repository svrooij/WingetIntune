using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using Winget.CommunityRepository.Models;

namespace Winget.CommunityRepository;
public sealed class WingetRepositoryWithEf : WingetRepository
{
    protected override async ValueTask<List<Models.WingetEntryExtended>> LoadEntriesFromSqlLite(CancellationToken cancellationToken, string url = DefaultIndexUri)
    {
        var folder = Path.Combine(Path.GetTempPath(), "WingetCommunityRepo", Guid.NewGuid().ToString());
        logger.LogInformation("Downloading the index to {folder}", folder);
        Directory.CreateDirectory(folder);
        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
        ExtractToDirectory(zip, folder);
        logger.LogDebug("Index downloaded to {folder}", folder);
        //var folder = "C:\\Users\\stephan\\AppData\\Local\\Temp\\WingetCommunityRepo\\2b73b4c2-e89d-49d0-a68b-133f36f3e65f";
        var file = Path.Combine(folder, "Public", "index.db");
        var connectionString = $"Data Source='{file}';Pooling=true;";
        var results = new List<Models.WingetEntryExtended>();
        //var stopWatch = new Stopwatch();
        //stopWatch.Restart();
        using (var db = new DbModels.IndexContext(connectionString))
        {
            var groupedManifests = await db.Manifests
                .Include(m => m.NameValue)
                .Include(m => m.VersionValue)
                .Include(m => m.IdValue)
                .GroupBy(m => m.Id)
                .ToListAsync(cancellationToken);
            var totalCount = groupedManifests.Count;
            int counter = 0;
            var tagMaps = await db.TagsMaps.Include(tm => tm.TagValue).ToListAsync(cancellationToken);
            foreach (var manifests in groupedManifests)
            {
                if (cancellationToken.IsCancellationRequested) { break; }
                counter++;
                if (counter % 50 == 0)
                {
                    LogProcessing(counter, totalCount);
                }

                var id = manifests.First().IdValue.Id1;

                try
                {
                    var entry = new Models.WingetEntryExtended { PackageId = id };

                    entry.Version = manifests.Select(x => x.VersionValue.Version1)?.GetHighestVersion();
                    var manifest = manifests.First(m => m.VersionValue.Version1 == entry.Version);

                    entry.Name = manifest.NameValue.Name1;
                    entry.Tags = tagMaps.Where(tm => tm.Manifest == manifest.Rowid).Select(tm => tm.TagValue.Tag1).ToArray();

                    LogPackage(entry.PackageId, entry.Version);
                    results.Add(entry);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error parsing {id}", id);
                }

            }
        }
        //stopWatch.Stop();
        //logger.LogInformation("Loaded {count} entries in {time}s", results.Count, stopWatch.Elapsed.TotalSeconds);


        await Task.Delay(2000, CancellationToken.None);

        Directory.Delete(folder, true);

        cancellationToken.ThrowIfCancellationRequested();

        return results.OrderBy(p => p.PackageId).ToList();
    }

    private void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName)
    {
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string filePath = Path.GetFullPath(Path.Combine(destinationDirectoryName, entry.FullName));

            if (!filePath.StartsWith(destinationDirectoryName, StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException("Extracting Zip entry would have resulted in a file outside the specified destination directory.");
            }

            var directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (string.IsNullOrEmpty(entry.Name))
            {
                //Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                continue;
            }

            entry.ExtractToFile(filePath, true);
        }
    }
}
