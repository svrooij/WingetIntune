using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace Winget.CommunityRepository;

public partial class WingetRepository
{
    public const string OpenSourceIndexUri = "https://raw.githubusercontent.com/svrooij/winget-pkgs-index/main/index.json";
    public const string DefaultIndexUri = "https://cdn.winget.microsoft.com/cache/source.msix";
    public bool UseRespository { get; set; }
    public Uri IndexUri { get; set; } = new(OpenSourceIndexUri);
    protected readonly HttpClient httpClient;
    protected readonly ILogger<WingetRepository> logger;

    private List<Models.WingetEntry>? Entries;

    private readonly string cacheFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WingetCommunityRepo", "index.json");

    public WingetRepository(HttpClient? httpClient = null, ILogger<WingetRepository>? logger = null)
    {
        UseRespository = false;
        this.httpClient = httpClient ?? new HttpClient();
        this.httpClient.DefaultRequestHeaders.Add("User-Agent", "WingetIntune");
        this.logger = logger ?? new NullLogger<WingetRepository>();
    }

    public async ValueTask<string?> GetLatestVersion(string packageId, CancellationToken cancellationToken = default)
    {
        var entry = await GetEntry(packageId, cancellationToken);
        return entry?.Version;
    }

    public async ValueTask<string?> GetPackageId(string packageId, CancellationToken cancellationToken = default)
    {
        var entry = await GetEntry(packageId, cancellationToken);
        return entry?.PackageId;
    }

    private async ValueTask<Models.WingetEntry?> GetEntry(string packageId, CancellationToken cancellationToken = default)
    {
        await LoadEntries(cancellationToken, false, cacheFile);

        var entry = Entries!.FirstOrDefault(e => e.PackageId!.Equals(packageId, StringComparison.OrdinalIgnoreCase));
        return entry;
    }

    public async ValueTask<IEnumerable<Models.WingetEntry>> SearchPackage(string query, CancellationToken cancellationToken = default)
    {
        await LoadEntries(cancellationToken, false, cacheFile);

        var results = Entries!.Where(e => e.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) == true || e.PackageId!.Contains(query, StringComparison.OrdinalIgnoreCase));
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
            LogLoadingPackageIndexFromCache(cacheFile);
            var info = new FileInfo(cacheFile);
            if (info.LastWriteTimeUtc > DateTime.UtcNow.AddHours(-3))
            {
                LogCacheStillValid();
                var cacheData = await File.ReadAllTextAsync(cacheFile, cancellationToken);
                Entries = JsonSerializer.Deserialize<List<Models.WingetEntry>>(cacheData);
                return Entries!;
            }
        }

        if (UseRespository)
        {
            LogLoadingPackageIndexFromRepository(DefaultIndexUri);
            Entries = await LoadEntriesFromSqlLite(cancellationToken, DefaultIndexUri);
        }
        else
        {
            LogLoadingPackageIndex(IndexUri);
            var response = await httpClient.GetAsync(IndexUri!, cancellationToken);
            response.EnsureSuccessStatusCode();
            Entries = await response.Content.ReadFromJsonAsync<List<Models.WingetEntry>>(cancellationToken: cancellationToken);
        }

        if (!string.IsNullOrEmpty(cacheFile))
        {
            LogSavingPackageIndex(cacheFile);
            var json = JsonSerializer.Serialize(Entries);
            var cachePath = Path.GetDirectoryName(cacheFile);
            Directory.CreateDirectory(cachePath!);
            await File.WriteAllTextAsync(cacheFile, json, cancellationToken);
        }

        return Entries!;
    }

    protected virtual ValueTask<List<Models.WingetEntry>> LoadEntriesFromSqlLite(CancellationToken cancellationToken, string url = DefaultIndexUri)
    {
        throw new NotImplementedException("Use the Winget.CommunityRepository.Ef package to use this method");
    }

    [LoggerMessage(EventId = 100, Level = LogLevel.Information, Message = "Processing {numberOfEntries} of {totalEntries}")]
    protected partial void LogProcessing(int numberOfEntries, int totalEntries);

    [LoggerMessage(EventId = 101, Level = LogLevel.Information, Message = "Saving package index to cache at {cacheFile}")]
    private partial void LogSavingPackageIndex(string cacheFile);

    [LoggerMessage(EventId = 102, Level = LogLevel.Information, Message = "Loading package index from {indexUri}")]
    private partial void LogLoadingPackageIndex(Uri indexUri);

    [LoggerMessage(EventId = 103, Level = LogLevel.Information, Message = "Loading package index from repository {indexUrl}")]
    private partial void LogLoadingPackageIndexFromRepository(string indexUrl);

    [LoggerMessage(EventId = 110, Level = LogLevel.Debug, Message = "Found {packageId} with {version}")]
    protected partial void LogPackage(string packageId, string? version);

    [LoggerMessage(EventId = 111, Level = LogLevel.Debug, Message = "Loading package index from cache at {cacheFile}")]
    private partial void LogLoadingPackageIndexFromCache(string cacheFile);

    [LoggerMessage(EventId = 112, Level = LogLevel.Debug, Message = "Cache is still valid, using cache")]
    private partial void LogCacheStillValid();
}
