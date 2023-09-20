using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Winget.CommunityRepository;
public partial class WingetRepository
{
    public const string OpenSourceIndexUri = "https://github.com/svrooij/winget-pkgs-index/raw/main/index.json";
    public const string DefaultIndexUri = "https://winget.azureedge.net/cache/source.msix";
    public bool UseRespository { get; set; }
    public Uri IndexUri { get; set; } = new(OpenSourceIndexUri);
    private readonly HttpClient httpClient;
    private readonly ILogger<WingetRepository> logger;

    private List<Models.WingetEntry>? Entries;

    private readonly string cacheFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WingetCommunityRepo", "index.json");

    public WingetRepository(HttpClient? httpClient = null, ILogger<WingetRepository>? logger = null)
    {
        UseRespository = false;
        this.httpClient = httpClient ?? new();
        this.logger = logger ?? new NullLogger<WingetRepository>();
    }

    public async ValueTask<string> GetLatestVersion(string packageId, CancellationToken cancellationToken = default)
    {
        await LoadEntries(cancellationToken, false, cacheFile);

        var entry = Entries!.FirstOrDefault(e => e.PackageId == packageId);
        return entry!.Version;
    }

    public async ValueTask<IEnumerable<Models.WingetEntry>> SearchPackage(string query, CancellationToken cancellationToken = default)
    {
        await LoadEntries(cancellationToken, false, cacheFile);

        var results = Entries!.Where(e => e.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) == true || e.PackageId.Contains(query, StringComparison.OrdinalIgnoreCase));
        return results;
    }

    public async ValueTask<IEnumerable<Models.WingetEntry>> RefreshPackages(bool saveToCache = false, CancellationToken cancellationToken = default)
    {
        await LoadEntries(cancellationToken, true, saveToCache ? cacheFile : null);
        return Entries!;
    }

    private async ValueTask<List<Models.WingetEntry>> LoadEntries(CancellationToken cancellationToken, bool refresh = false, string? cacheFile = null)
    {
        if (Entries is not null && !refresh) { return Entries; }

        if (!string.IsNullOrEmpty(cacheFile) && File.Exists(cacheFile) && !refresh)
        {
            var info = new FileInfo(cacheFile);
            if (info.LastWriteTimeUtc > DateTime.UtcNow.AddDays(-1))
            {
                var cacheData = await File.ReadAllTextAsync(cacheFile, cancellationToken);
                Entries = JsonSerializer.Deserialize<List<Models.WingetEntry>>(cacheData);
                return Entries!;
            }
        }

        if (UseRespository)
        {
            Entries = await LoadEntriesFromSqlLite(cancellationToken, DefaultIndexUri);
        } else
        {
            var response = await httpClient.GetAsync(IndexUri!, cancellationToken);
            response.EnsureSuccessStatusCode();
            Entries = await response.Content.ReadFromJsonAsync<List<Models.WingetEntry>>(cancellationToken: cancellationToken);
        }

        if (!string.IsNullOrEmpty(cacheFile))
        {
            var json = JsonSerializer.Serialize(Entries);
            var cachePath = Path.GetDirectoryName(cacheFile);
            Directory.CreateDirectory(cachePath!);
            await File.WriteAllTextAsync(cacheFile, json, cancellationToken);
        }

        return Entries!;

    }

    private async ValueTask<List<Models.WingetEntry>> LoadEntriesFromSqlLite(CancellationToken cancellationToken, string url = DefaultIndexUri)
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


        await Task.Delay(2000);

        Directory.Delete(folder, true);

        return results;

    }

    [LoggerMessage(EventId = 100, Level = LogLevel.Information, Message = "Processing {numberOfEntries} of {totalEntries}")]
    private partial void LogProcessing(int numberOfEntries, int totalEntries);

    [LoggerMessage(EventId = 110, Level = LogLevel.Debug, Message = "Found {packageId} with {version}")]
    private partial void LogPackage(string packageId, string? version);
}
