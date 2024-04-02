using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace Winget.CommunityRepository;
public sealed class WingetRepositoryWithEf : WingetRepository
{
    protected override async ValueTask<List<Models.WingetEntry>> LoadEntriesFromSqlLite(CancellationToken cancellationToken, string url = DefaultIndexUri)
    {
        var folder = Path.Combine(Path.GetTempPath(), "WingetCommunityRepo", Guid.NewGuid().ToString());
        Directory.CreateDirectory(folder);
        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
        zip.ExtractToDirectory(folder);
        var file = Path.Combine(folder, "Public", "index.db");
        var connectionString = $"Data Source='{file}';Pooling=false;";
        var results = new List<Models.WingetEntry>();
        using (var db = new DbModels.IndexContext(connectionString))
        {
            var ids = db.Ids.OrderBy(x => x.Id1).ToList();
            var totalCount = ids.Count;
            int counter = 0;
            foreach (var id in ids)
            {
                cancellationToken.ThrowIfCancellationRequested();
                counter++;
                if (counter % 50 == 0)
                {
                    LogProcessing(counter, totalCount);
                }

                try
                {
                    var entry = new Models.WingetEntry { PackageId = id.Id1 };
                    var versions = db.Manifests.Where(m => m.Id == id.Rowid).Select(x => x.VersionValue.Version1).ToList();
                    entry.Version = versions?.GetHighestVersion();
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


        await Task.Delay(2000, cancellationToken);

        Directory.Delete(folder, true);

        return results;
    }
}
