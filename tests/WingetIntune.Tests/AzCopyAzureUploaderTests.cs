using Microsoft.Extensions.Logging.Abstractions;
using WingetIntune.Implementations;
namespace WingetIntune.Tests;

public class AzCopyAzureUploaderTests
{
    private static string azCopyPath = Path.Combine(Path.GetTempPath(), "intunewin", "azcopy.exe");

    [Fact]
    public async Task UploadAsync_CallsProcessManager()
    {
        var file = "C:\\test\\file.txt";
        var url = "https://localhost/test/uri";

        var fileManager = Substitute.For<IFileManager>();
        fileManager.FileExists(azCopyPath).Returns(true);

        var processManager = Substitute.For<IProcessManager>();
        processManager.RunProcessAsync(azCopyPath, $"copy \"{file}\" \"{url}\" --output-type \"json\"", Arg.Any<CancellationToken>(), false)
            .Returns(Task.FromResult(new ProcessResult(0, "", "")));

        var azCopyAzureUploader = new AzCopyAzureUploader(new NullLogger<AzCopyAzureUploader>(), processManager, fileManager);
        await azCopyAzureUploader.UploadFileToAzureAsync(file, new Uri(url), CancellationToken.None);

        fileManager.Received().FileExists(azCopyPath);

        await processManager.Received().RunProcessAsync(azCopyPath, $"copy \"{file}\" \"{url}\" --output-type \"json\"", Arg.Any<CancellationToken>(), false);
    }

    [Fact]
    public async Task UploadAsync_ThrowsOnNonZeroReturnCode()
    {
        var file = "C:\\test\\file.txt";
        var url = "https://localhost/test/uri";

        var fileManager = Substitute.For<IFileManager>();
        fileManager.FileExists(azCopyPath).Returns(true);

        var processManager = Substitute.For<IProcessManager>();
        processManager.RunProcessAsync(azCopyPath, $"copy \"{file}\" \"{url}\" --output-type \"json\"", Arg.Any<CancellationToken>(), false)
            .Returns(Task.FromResult(new ProcessResult(100, "Test Output", "Test Result")));

        var azCopyAzureUploader = new AzCopyAzureUploader(new NullLogger<AzCopyAzureUploader>(), processManager, fileManager);
        await Assert.ThrowsAsync<Exception>(() => azCopyAzureUploader.UploadFileToAzureAsync(file, new Uri(url), CancellationToken.None));

        fileManager.Received().FileExists(azCopyPath);
        await processManager.Received().RunProcessAsync(azCopyPath, $"copy \"{file}\" \"{url}\" --output-type \"json\"", Arg.Any<CancellationToken>(), false);

    }

    [Fact]
    public async Task UploadAsync_DownloadsAzCopy()
    {
        var file = "C:\\test\\file.txt";
        var url = "https://localhost/test/uri";

        var fileManager = Substitute.For<IFileManager>();
        fileManager.FileExists(azCopyPath).Returns(false);

        fileManager.DownloadFileAsync("https://aka.ms/downloadazcopy-v10-windows", Arg.Any<string>(), null, true, true, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        fileManager.ExtractFileToFolder(Arg.Any<string>(), Arg.Any<string>());

        fileManager.FindFile(Arg.Any<string>(), "azcopy.exe").Returns(azCopyPath);

        fileManager.CopyFile(azCopyPath, azCopyPath, false);

        fileManager.DeleteFileOrFolder(Arg.Any<string>());

        var processManager = Substitute.For<IProcessManager>();
        processManager.RunProcessAsync(azCopyPath, $"copy \"{file}\" \"{url}\" --output-type \"json\"", Arg.Any<CancellationToken>(), false)
            .Returns(Task.FromResult(new ProcessResult(0, "", "")));



        var azCopyAzureUploader = new AzCopyAzureUploader(new NullLogger<AzCopyAzureUploader>(), processManager, fileManager);
        await azCopyAzureUploader.UploadFileToAzureAsync(file, new Uri(url), default);


        fileManager.Received().FileExists(azCopyPath);

        await fileManager.Received().DownloadFileAsync("https://aka.ms/downloadazcopy-v10-windows", Arg.Any<string>(), null, true, true, Arg.Any<CancellationToken>());

        fileManager.Received().ExtractFileToFolder(Arg.Any<string>(), Arg.Any<string>());

        fileManager.Received().FindFile(Arg.Any<string>(), "azcopy.exe");

        fileManager.Received().CopyFile(azCopyPath, azCopyPath, false);

        fileManager.Received().DeleteFileOrFolder(Arg.Any<string>());

        await processManager.Received().RunProcessAsync(azCopyPath, $"copy \"{file}\" \"{url}\" --output-type \"json\"", Arg.Any<CancellationToken>(), false);

    }
}
