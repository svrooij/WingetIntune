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
    private const string name = "generate-index";
    private const string description = "(hidden) Generates the index.json file for the repository (cross platform)";
    public GenerateIndexCommand() : base(name, description)
    {
        IsHidden = true;
        AddOption(new Option<string>(["--output-path", "-o"], "The path to the output file") { IsRequired = true });
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

        logging.SetLogLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        logger.LogInformation("Loading packages from {sourceUri}", options.SourceUri);
        var repo = host.Services.GetRequiredService<Winget.CommunityRepository.WingetRepository>();
        repo.UseRespository = true;
        var packages = await repo.RefreshPackages(false, combinedCancellation.Token);
        if (File.Exists(options.OutputPath) && options.DetectChanges)
        {
            await HandleChanges(logger, options, packages, combinedCancellation.Token);
        }
        var json = JsonSerializer.Serialize(packages);
        await File.WriteAllTextAsync(Path.GetFullPath(options.OutputPath), json, combinedCancellation.Token);
        logger.LogInformation("Generated index.json file at {outputPath}", options.OutputPath);
        return 0;
    }

    private static async Task HandleChanges(ILogger logger, GenerateIndexCommandOptions options, IEnumerable<Winget.CommunityRepository.Models.WingetEntry> packages, CancellationToken cancellationToken)
    {
        logger.LogInformation("Detecting changes from existing index.json file at {outputPath}", options.OutputPath);
        var existingJson = await File.ReadAllTextAsync(Path.GetFullPath(options.OutputPath), cancellationToken);
        var existingPackages = JsonSerializer.Deserialize<IEnumerable<Winget.CommunityRepository.Models.WingetEntry>>(existingJson);
        if (existingPackages is not null)
        {
            var updates = packages
                .Where(p => !existingPackages.Any(ep => ep.PackageId == p.PackageId && ep.Version == p.Version))
                .OrderBy(p => p.PackageId);
            if (updates.Any())
            {
                var lastWriteTime = File.GetLastWriteTimeUtc(Path.GetFullPath(options.OutputPath));
                logger.LogInformation("Detected {count} updates since {lastWriteTime:yyyy-MM-dd HH:mm} UTC", updates.Count(), lastWriteTime);
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

                if (options.UpdateGithub == true)
                {
                    // Write markdown table with update summary to environment variable GITHUB_STEP_SUMMARY
                    // get last file write date from the existing file

                    var markdown = new StringBuilder();
                    markdown.AppendLine($"Detected **{updates.Count()}** updates since `{lastWriteTime:yyyy-MM-dd HH:mm:ss} UTC`");
                    markdown.AppendLine("");
                    markdown.AppendLine("| PackageId | Version |");
                    markdown.AppendLine("| --- | --- |");
                    foreach (var update in updates)
                    {
                        markdown.AppendLine($"| {update.PackageId} | {update.Version} |");
                    }
                    Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", markdown.ToString(), EnvironmentVariableTarget.Process);
                    logger.LogInformation("Generated GitHub Action step summary");
                }

                if (options.UpdateUri is not null && options.UpdateUri.IsAbsoluteUri)
                {
                    await PostUpdatesToUri(logger, options.UpdateUri, updates, cancellationToken);
                }
            }
            else
            {
                logger.LogInformation("No updates detected");
            }
        }
    }

    private static async Task PostUpdatesToUri(ILogger logger, Uri uri, IEnumerable<Winget.CommunityRepository.Models.WingetEntry> updates, CancellationToken cancellationToken)
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
        public string OutputPath { get; set; }
        public Uri? SourceUri { get; set; }
        public string? UpdateCsv { get; set; }
        public string? UpdateJson { get; set; }
        public bool? UpdateGithub { get; set; }
        public Uri? UpdateUri { get; set; }

        internal bool DetectChanges => !string.IsNullOrEmpty(UpdateJson) || !string.IsNullOrEmpty(UpdateCsv) || UpdateUri?.IsAbsoluteUri == true || UpdateGithub == true;
    }
}
