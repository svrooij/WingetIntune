using Microsoft.Extensions.Logging.Abstractions;
using WingetIntune.Intune;

namespace WingetIntune.Tests.Intune;

public class ProcessIntunePackagerTests
{
    [Fact]
    public async Task CreatePackage_DownloadsTool()
    {
        var processManager = Substitute.For<IProcessManager>();
        var fileManager = Substitute.For<IFileManager>();
        string toolPath = Path.Combine(Path.GetTempPath(), "Intune", ProcessIntunePackager.IntuneWinAppUtil);

        var packager = new ProcessIntunePackager(processManager, fileManager, new NullLogger<ProcessIntunePackager>());

        var inputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package");
        var outputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package", "output");

        var installerFilename = "installer.exe";

        var cancellationToken = new CancellationToken();

        fileManager.FileExists(toolPath).Returns(false);
        fileManager.DownloadFileAsync(ProcessIntunePackager.IntuneWinAppUtilUrl, toolPath, null, true, false, cancellationToken)
            .Returns(Task.CompletedTask);

        processManager.RunProcessAsync(toolPath, Arg.Any<string>(), cancellationToken, false)
            .Returns(Task.FromResult(new ProcessResult(0, "", "")));

        await packager.CreatePackage(inputFolder, outputFolder, installerFilename, cancellationToken);

        await fileManager.Received().DownloadFileAsync(ProcessIntunePackager.IntuneWinAppUtilUrl, toolPath, null, true, false, cancellationToken);

    }

    [Fact]
    public async Task CreatePackage_DoesNotDownloadToolIfExists()
    {
        var processManager = Substitute.For<IProcessManager>();
        var fileManager = Substitute.For<IFileManager>();
        string toolPath = Path.Combine(Path.GetTempPath(), "Intune", ProcessIntunePackager.IntuneWinAppUtil);

        var packager = new ProcessIntunePackager(processManager, fileManager, new NullLogger<ProcessIntunePackager>());

        var inputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package");
        var outputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package", "output");

        var installerFilename = "installer.exe";

        var cancellationToken = new CancellationToken();

        fileManager.FileExists(toolPath).Returns(true);

        processManager.RunProcessAsync(toolPath, Arg.Any<string>(), cancellationToken, false)
            .Returns(Task.FromResult(new ProcessResult(0, "", "")));

        await packager.CreatePackage(inputFolder, outputFolder, installerFilename, cancellationToken);

        fileManager.Received().FileExists(toolPath);
        await processManager.Received().RunProcessAsync(toolPath, Arg.Any<string>(), cancellationToken, false);

    }

    [Fact]
    public async Task CreatePackage_CallsCorrectProcess()
    {
        var processManager = Substitute.For<IProcessManager>();
        var fileManager = Substitute.For<IFileManager>();
        string toolPath = Path.Combine(Path.GetTempPath(), "Intune", ProcessIntunePackager.IntuneWinAppUtil);

        var packager = new ProcessIntunePackager(processManager, fileManager, new NullLogger<ProcessIntunePackager>());

        var inputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package");
        var outputFolder = Path.Combine(Path.GetTempPath(), "packages", "some-package", "output");

        var installerFilename = "installer.exe";

        var cancellationToken = new CancellationToken();

        fileManager.FileExists(toolPath).Returns(true);

        var arguments = $"-c {inputFolder} -s {installerFilename} -o {outputFolder} -q";

        processManager.RunProcessAsync(toolPath, arguments, cancellationToken, false)
            .Returns(Task.FromResult(new ProcessResult(0, "", "")));

        await packager.CreatePackage(inputFolder, outputFolder, installerFilename, cancellationToken);

        fileManager.Received().FileExists(toolPath);
        await processManager.Received().RunProcessAsync(toolPath, arguments, cancellationToken, false);

    }
}
