using Microsoft.Extensions.Logging.Abstractions;
using WingetIntune.Intune;

namespace WingetIntune.Tests.Intune;

public class ProcessIntunePackagerTests
{
    [Fact]
    public async Task CreatePackage_DownloadsTool()
    {
        var processManagerMock = new Mock<IProcessManager>(MockBehavior.Loose);
        var fileManagerMock = new Mock<IFileManager>(MockBehavior.Strict);
        string toolPath = Path.Combine(Path.GetTempPath(), "Intune", ProcessIntunePackager.IntuneWinAppUtil);

        var packager = new ProcessIntunePackager(processManagerMock.Object, fileManagerMock.Object, new NullLogger<ProcessIntunePackager>());

        var inputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package");
        var outputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package", "output");

        var installerFilename = "installer.exe";

        var cancellationToken = new CancellationToken();

        fileManagerMock.Setup(x => x.FileExists(toolPath)).Returns(false);
        fileManagerMock.Setup(x => x.DownloadFileAsync(ProcessIntunePackager.IntuneWinAppUtilUrl, toolPath, true, false, cancellationToken))
            .Returns(Task.CompletedTask).Verifiable();

        processManagerMock.Setup(x => x.RunProcessAsync(toolPath, It.IsAny<string>(), cancellationToken, false))
            .ReturnsAsync(new ProcessResult(0, "", ""));

        await packager.CreatePackage(inputFolder, outputFolder, installerFilename, cancellationToken);
        fileManagerMock.VerifyAll();
    }

    [Fact]
    public async Task CreatePackage_DoesNotDownloadToolIfExists()
    {
        var processManagerMock = new Mock<IProcessManager>(MockBehavior.Loose);
        var fileManagerMock = new Mock<IFileManager>(MockBehavior.Strict);
        string toolPath = Path.Combine(Path.GetTempPath(), "Intune", ProcessIntunePackager.IntuneWinAppUtil);

        var packager = new ProcessIntunePackager(processManagerMock.Object, fileManagerMock.Object, new NullLogger<ProcessIntunePackager>());

        var inputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package");
        var outputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package", "output");

        var installerFilename = "installer.exe";

        var cancellationToken = new CancellationToken();

        fileManagerMock.Setup(x => x.FileExists(toolPath)).Returns(true).Verifiable();

        processManagerMock.Setup(x => x.RunProcessAsync(toolPath, It.IsAny<string>(), cancellationToken, false))
            .ReturnsAsync(new ProcessResult(0, "", ""))
            .Verifiable();

        await packager.CreatePackage(inputFolder, outputFolder, installerFilename, cancellationToken);
        fileManagerMock.VerifyAll();
    }

    [Fact]
    public async Task CreatePackage_CallsCorrectProcess()
    {
        var processManagerMock = new Mock<IProcessManager>(MockBehavior.Strict);
        var fileManagerMock = new Mock<IFileManager>(MockBehavior.Strict);
        string toolPath = Path.Combine(Path.GetTempPath(), "Intune", ProcessIntunePackager.IntuneWinAppUtil);

        var packager = new ProcessIntunePackager(processManagerMock.Object, fileManagerMock.Object, new NullLogger<ProcessIntunePackager>());

        var inputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package");
        var outputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package", "output");

        var installerFilename = "installer.exe";

        var cancellationToken = new CancellationToken();

        fileManagerMock.Setup(x => x.FileExists(toolPath))
            .Returns(true)
            .Verifiable();

        var arguments = $"-c {inputFolder} -s {installerFilename} -o {outputFolder} -q";

        processManagerMock.Setup(x => x.RunProcessAsync(toolPath, arguments, cancellationToken, false))
            .ReturnsAsync(new ProcessResult(0, "", ""))
            .Verifiable();

        await packager.CreatePackage(inputFolder, outputFolder, installerFilename, cancellationToken);
        fileManagerMock.VerifyAll();
        processManagerMock.VerifyAll();
    }
}
