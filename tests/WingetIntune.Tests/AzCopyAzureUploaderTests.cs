using Microsoft.Extensions.Logging.Abstractions;

namespace WingetIntune.Tests;

public class AzCopyAzureUploaderTests
{
    private static string azCopyPath = Path.Combine(Path.GetTempPath(), "intunewin", "azcopy.exe");

    [Fact]
    public async Task UploadAsync_CallsProcessManager()
    {
        var file = "C:\\test\\file.txt";
        var url = "https://localhost/test/uri";

        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock
            .Setup(x => x.FileExists(azCopyPath))
            .Returns(true)
            .Verifiable();

        var processManagerMock = new Mock<IProcessManager>();
        processManagerMock
            .Setup(x => x.RunProcessAsync(azCopyPath, $"copy \"{file}\" \"{url}\" --output-type \"json\"", It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(new ProcessResult(0, "", ""))
            .Verifiable();

        var azCopyAzureUploader = new AzCopyAzureUploader(new NullLogger<AzCopyAzureUploader>(), processManagerMock.Object, fileManagerMock.Object);
        await azCopyAzureUploader.UploadFileToAzureAsync(file, new Uri(url), CancellationToken.None);

        fileManagerMock.VerifyAll();
        processManagerMock.VerifyAll();
    }

    [Fact]
    public async Task UploadAsync_ThrowsOnNonZeroReturnCode()
    {
        var file = "C:\\test\\file.txt";
        var url = "https://localhost/test/uri";

        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock
            .Setup(x => x.FileExists(azCopyPath))
            .Returns(true)
            .Verifiable();

        var processManagerMock = new Mock<IProcessManager>();
        processManagerMock
            .Setup(x => x.RunProcessAsync(azCopyPath, $"copy \"{file}\" \"{url}\" --output-type \"json\"", It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(new ProcessResult(100, "Test Output", "Test Result"))
            .Verifiable();

        var azCopyAzureUploader = new AzCopyAzureUploader(new NullLogger<AzCopyAzureUploader>(), processManagerMock.Object, fileManagerMock.Object);
        await Assert.ThrowsAsync<Exception>(() => azCopyAzureUploader.UploadFileToAzureAsync(file, new Uri(url), CancellationToken.None));

        fileManagerMock.VerifyAll();
        processManagerMock.VerifyAll();
    }

    [Fact]
    public async Task UploadAsync_DownloadsAzCopy()
    {
        var file = "C:\\test\\file.txt";
        var url = "https://localhost/test/uri";

        var fileManagerMock = new Mock<IFileManager>(MockBehavior.Strict);
        fileManagerMock
            .Setup(x => x.FileExists(azCopyPath))
            .Returns(false)
            .Verifiable();

        fileManagerMock.Setup(fileManagerMock => fileManagerMock.DownloadFileAsync("https://aka.ms/downloadazcopy-v10-windows", It.IsAny<string>(), true, true, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        fileManagerMock.Setup(fileManagerMock => fileManagerMock.ExtractFileToFolder(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

        fileManagerMock.Setup(fileManagerMock => fileManagerMock.FindFile(It.IsAny<string>(), "azcopy.exe")).Returns(azCopyPath).Verifiable();

        fileManagerMock.Setup(fileManagerMock => fileManagerMock.CopyFile(azCopyPath, azCopyPath, false)).Verifiable();

        fileManagerMock.Setup(fileManagerMock => fileManagerMock.DeleteFileOrFolder(It.IsAny<string>())).Verifiable();

        var processManagerMock = new Mock<IProcessManager>();
        processManagerMock
            .Setup(x => x.RunProcessAsync(azCopyPath, $"copy \"{file}\" \"{url}\" --output-type \"json\"", It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(new ProcessResult(0, "", ""))
            .Verifiable();

        var azCopyAzureUploader = new AzCopyAzureUploader(new NullLogger<AzCopyAzureUploader>(), processManagerMock.Object, fileManagerMock.Object);
        await azCopyAzureUploader.UploadFileToAzureAsync(file, new Uri(url), CancellationToken.None);

        fileManagerMock.VerifyAll();
        processManagerMock.VerifyAll();
    }
}