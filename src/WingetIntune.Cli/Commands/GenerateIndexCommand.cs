using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Text;
using System.Text.Json;
using WingetIntune.Cli.Configuration;

namespace WingetIntune.Commands;
internal class GenerateIndexCommand : Command
{
    private const string GithubStepSummary = "GITHUB_STEP_SUMMARY";
    private const string name = "generate-index";
    private const string description = "(hidden) Generates index files for the repository (cross platform)";
    public GenerateIndexCommand() : base(name, description)
    {
        IsHidden = true;
        AddOption(new Option<string>(["--output-path", "-o"], "The path to the v1 index.json") { IsRequired = false });
        AddOption(new Option<string>(["--output-folder"], "Write all index files to this folder (jsonv2 is always written here)") { IsHidden = true });
        AddOption(new Option<bool>(["--csv"], "Write the index as CSV") { IsHidden = true });
        AddOption(new Option<bool>(["--json"], "Write the index as JSON") { IsHidden = true });
        AddOption(new Option<bool>(["--csvv2"], "Write the index as CSV") { IsHidden = true });
        AddOption(new Option<Uri>(["--source-uri", "-s"], () => new Uri(Winget.CommunityRepository.WingetRepository.DefaultIndexUri), "The source URI to use for the index.json file"));
        AddOption(new Option<int>(["--timeout", "-t"], () => 600000, "The timeout for the operation in milliseconds"));
        AddOption(new Option<string>(["--update-json"], "Create JSON file with only the updates") { IsHidden = true });
        AddOption(new Option<string>(["--update-csv"], "Create CSV file with only the updates") { IsHidden = true });
        AddOption(new Option<bool>(["--update-github"], "Create GitHub Action step summary") { IsHidden = true });
        AddOption(new Option<Uri?>(["--update-uri"], () =>
        {
            var uri = Environment.GetEnvironmentVariable("UPDATE_URI");
            return string.IsNullOrEmpty(uri) ? null : new Uri(uri);
        }, "Post updates to this url")
        { IsHidden = true });

        this.Handler = CommandHandler.Create<GenerateIndexCommandOptions, InvocationContext>(HandleCommand);
    }

    private async Task<int> HandleCommand(GenerateIndexCommandOptions options, InvocationContext context)
    {
        using var timeoutCancellation = new CancellationTokenSource(options.Timeout);
        using var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), timeoutCancellation.Token);
        var host = context.GetHost();
        var logging = host.Services.GetRequiredService<ControlableLoggingProvider>();
        var logger = host.Services.GetRequiredService<ILogger<GenerateIndexCommand>>();

        logger.LogInformation("Loading packages from {sourceUri}", options.SourceUri);
        var repo = host.Services.GetRequiredService<Winget.CommunityRepository.WingetRepository>();
        repo.UseRespository = true;
        var updatedAt = DateTimeOffset.UtcNow;
        var packages = await repo.RefreshPackages(false, combinedCancellation.Token);
        logger.LogInformation("Loaded {count} packages", packages.Count());

        if (options.OutputPath is not null)
        {
            await WriteV1Json(logger, options.OutputPath, packages, combinedCancellation.Token);
        }


        if (options.OutputFolder is not null)
        {
            if (options.Json == true)
            {
                await WriteV1Json(logger, options.GetPath("index.json"), packages, combinedCancellation.Token);
            }

            if (options.Csv == true)
            {
                var csv = new StringBuilder();
                csv.AppendLine("\"PackageId\",\"Version\"");
                foreach (var package in packages)
                {
                    csv.AppendLine($"\"{package.PackageId}\",\"{package.Version}\"");
                }
                await File.WriteAllTextAsync(options.GetPath("index.csv"), csv.ToString(), combinedCancellation.Token);
                logger.LogInformation("Generated CSV file at {outputPath}", options.GetPath("index.csv"));
            }

            var jsonV2Path = options.GetPath("index.v2.json");
            //DateTimeOffset? lastWrite = File.Exists(jsonV2Path) ? File.GetLastWriteTimeUtc(jsonV2Path) : null;
            packages = await WriteV2Json(logger, options, packages.ToList(), updatedAt, combinedCancellation.Token);
            if (options.CsvV2 == true)
            {
                var csv = new StringBuilder();
                csv.AppendLine("\"PackageId\",\"Version\",\"Name\",\"LastUpdate\"");
                foreach (var package in packages)
                {
                    csv.AppendLine($"\"{package.PackageId}\",\"{package.Version}\",\"{package.Name?.Replace('"', '\'')}\",\"{package.LastUpdate:u}\"");
                }
                await File.WriteAllTextAsync(options.GetPath("index.v2.csv"), csv.ToString(), combinedCancellation.Token);
                logger.LogInformation("Generated CSV file at {outputPath}", options.GetPath("index.v2.csv"));
            }
            if (options.DetectChanges)
            {
                DateTimeOffset? previousUpdateStamp = packages.Where(p => p.LastUpdate < updatedAt).Max(p => p.LastUpdate);
                await HandleChanges(logger, options, packages, previousUpdateStamp, combinedCancellation.Token);
            }
        }

        return 0;
    }

    private static async Task WriteV1Json(ILogger logger, string outputPath, IEnumerable<Winget.CommunityRepository.Models.WingetEntryExtended> packages, CancellationToken cancellationToken)
    {
        var packagesV1 = packages
            .Select(p => new Winget.CommunityRepository.Models.WingetEntry { Name = p.Name, PackageId = p.PackageId, Version = p.Version })
            .ToList();
        var json = JsonSerializer.Serialize(packagesV1);
        await File.WriteAllTextAsync(Path.GetFullPath(outputPath), json, cancellationToken);
        logger.LogInformation("Generated v1 index.json file at {outputPath}", outputPath);
    }

    private static async Task<IEnumerable<Winget.CommunityRepository.Models.WingetEntryExtended>> WriteV2Json(ILogger logger, GenerateIndexCommandOptions options, List<Winget.CommunityRepository.Models.WingetEntryExtended> packages, DateTimeOffset updateStamp, CancellationToken cancellationToken)
    {
        var v2Json = options.GetPath("index.v2.json");
        if (File.Exists(v2Json))
        {
            // Load existing v2 file
            // Update the existing packages with the new version and the last update time
            // Add new packages
            logger.LogInformation("Index.v2.json file exists, using it to track updates");
            var existingJson = await File.ReadAllTextAsync(Path.GetFullPath(v2Json), cancellationToken);
            var existingPackages = JsonSerializer.Deserialize<IEnumerable<Winget.CommunityRepository.Models.WingetEntryExtended>>(existingJson);
            if (existingPackages is not null)
            {
                packages.ForEach(p =>
                {
                    p.LastUpdate = existingPackages.FirstOrDefault(ep => ep.PackageId == p.PackageId && ep.Version == p.Version)?.LastUpdate ?? updateStamp;
                });
            }
        }
        else // File does not exist, set last update to the current time
        {
            logger.LogInformation("Index.v2.json does not exists, setting update time to now");
            packages.ForEach(p =>
            {
                if (!p.LastUpdate.HasValue)
                {
                    p.LastUpdate = updateStamp;
                }
            });
        }

        var json = JsonSerializer.Serialize(packages);
        await File.WriteAllTextAsync(Path.GetFullPath(v2Json), json, cancellationToken);

        return packages;
    }

    private static async Task HandleChanges(ILogger logger, GenerateIndexCommandOptions options, IEnumerable<Winget.CommunityRepository.Models.WingetEntryExtended> packages, DateTimeOffset? lastWrite, CancellationToken cancellationToken)
    {
        logger.LogInformation("Detecting changes since {LastUpdate} from existing index in {OutputFolder}", lastWrite, options.OutputFolder);
        var updates = packages
            .Where(p => !lastWrite.HasValue || !p.LastUpdate.HasValue || p.LastUpdate > lastWrite)
            .OrderBy(p => p.PackageId);
        if (lastWrite.HasValue && updates.Any())
        {
            logger.LogInformation("Detected {count} updates since {lastWriteTime:yyyy-MM-dd HH:mm} UTC", updates.Count(), lastWrite);
            if (!string.IsNullOrEmpty(options.UpdateJson))
            {
                var updatesJson = JsonSerializer.Serialize(updates);
                await File.WriteAllTextAsync(Path.GetFullPath(options.UpdateJson), updatesJson, cancellationToken);
                logger.LogInformation("Generated updates.json file at {outputPath}", options.UpdateJson);
            }

            if (!string.IsNullOrEmpty(options.UpdateCsv))
            {
                var csv = new StringBuilder();
                csv.AppendLine("\"PackageId\",\"Version\"");
                foreach (var update in updates)
                {
                    csv.AppendLine($"\"{update.PackageId}\",\"{update.Version}\"");
                }
                await File.WriteAllTextAsync(Path.GetFullPath(options.UpdateCsv), csv.ToString(), cancellationToken);
                logger.LogInformation("Generated updates.csv file at {outputPath}", options.UpdateCsv);
            }

            if (options.UpdateGithub == true || Environment.GetEnvironmentVariable(GithubStepSummary) is not null)
            {
                // Write markdown table with update summary to environment variable GITHUB_STEP_SUMMARY
                var markdown = new StringBuilder();
                markdown.AppendLine("## Winget crawl results");
                markdown.AppendLine("");
                markdown.AppendLine($"Detected **{updates.Count()}** updates since `{lastWrite:yyyy-MM-dd HH:mm:ss} UTC`");
                markdown.AppendLine("");
                markdown.AppendLine("### Changed packages");
                markdown.AppendLine("");
                markdown.AppendLine("| PackageId | Version |");
                markdown.AppendLine("| --- | --- |");
                foreach (var update in updates)
                {
                    markdown.AppendLine($"| {update.PackageId} | {update.Version} |");
                }
                markdown.AppendLine("");
                var summary = markdown.ToString();

                var summaryVariable = Environment.GetEnvironmentVariable(GithubStepSummary);
                if (summaryVariable is not null)
                {
                    try
                    {
                        logger.LogInformation("Writing GitHub step summary to {summaryVariable}", summaryVariable);
                        await File.AppendAllTextAsync(summaryVariable, summary, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to set GitHub step summary");
                    }
                }

                await File.WriteAllTextAsync(options.GetPath("github-step-summary.md"), summary, cancellationToken);
                logger.LogInformation("Generated GitHub Action step summary");
            }

            if (options.UpdateUri is not null && options.UpdateUri.IsAbsoluteUri)
            {
                await PostUpdatesToUri(logger, options.UpdateUri, updates, cancellationToken);
            }

        }
    }

    private static async Task PostUpdatesToUri(ILogger logger, Uri uri, IEnumerable<Winget.CommunityRepository.Models.WingetEntryExtended> updates, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(updates);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            await client.PostAsync(uri, content, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to post updates to {host}", uri.Host);
        }
    }

    internal class GenerateIndexCommandOptions
    {
        public int Timeout { get; set; }
        public string? OutputPath { get; set; }
        public string? OutputFolder { get; set; }
        public Uri? SourceUri { get; set; }
        public string? UpdateCsv { get; set; }
        public string? UpdateJson { get; set; }
        public bool? UpdateGithub { get; set; }
        public Uri? UpdateUri { get; set; } = Environment.GetEnvironmentVariable("WINGET_UPDATE_URI") is { } uri ? new Uri(uri) : null;
        public bool? Csv { get; set; }
        public bool? CsvV2 { get; set; }
        public bool? Json { get; set; }

        internal string GetPath(string file) => Path.Combine(OutputFolder ?? Environment.CurrentDirectory, file);

        internal bool DetectChanges => !string.IsNullOrEmpty(UpdateJson) || !string.IsNullOrEmpty(UpdateCsv) || UpdateUri?.IsAbsoluteUri == true || UpdateGithub == true;
    }
}
