using Microsoft.Extensions.Logging;
using System.IO.Compression;
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
        var file = Path.Combine(folder, "Public", "index.db");
        var connectionString = $"Data Source='{file}';Pooling=false;";
        var results = new List<Models.WingetEntryExtended>();
        using (var db = new DbModels.IndexContext(connectionString))
        {
            var ids = db.Ids.OrderBy(x => x.Id1).ToList();
            var totalCount = ids.Count;
            int counter = 0;
            foreach (var id in ids)
            {
                if(cancellationToken.IsCancellationRequested) { break; }
                counter++;
                if (counter % 50 == 0)
                {
                    LogProcessing(counter, totalCount);
                }

                try
                {
                    var entry = new Models.WingetEntryExtended { PackageId = id.Id1 };
                    var versions = db.Manifests.Where(m => m.Id == id.Rowid).Select(x => x.VersionValue.Version1).ToList();
                    var nameId = db.Manifests.First(m => m.Id == id.Rowid).Name;
                    entry.Name = db.Names.FirstOrDefault(n => n.Rowid == nameId)?.Name1;
                    entry.Version = versions?.GetHighestVersion();

                    var allManifestIds = db.Manifests.Where(db => db.Id == id.Rowid).Select(m => m.Rowid).ToList();
                    var tagIds = db.TagsMaps.Where(tm => allManifestIds.Contains(tm.Manifest)).Select(tm => tm.Tag).Distinct().ToList();

                    entry.Tags = db.Tags.Where(t => tagIds.Contains(t.Rowid)).Select(t => t.Tag1).ToArray();

                    LogPackage(entry.PackageId, entry.Version);
                    //entry.Name = db.Manifests.First(m => m.Id == id.Rowid).NameValue.Name1;
                    results.Add(entry);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error parsing {id}", id.Id1);
                }

            }
        }


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
