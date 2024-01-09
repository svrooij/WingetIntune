namespace WingetIntune.Models;
public class IntuneApp {
  public required string PackageId { get; set; }
  public required string Name { get; set; }
  public required string Version { get; set; }
  public required string GraphId { get; set; }

}