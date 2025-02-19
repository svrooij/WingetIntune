using System.ComponentModel.DataAnnotations;

namespace Winget.CommunityRepository.DbModels;

public partial class Manifest
{
    [Key]
    public long Rowid { get; set; }

    public long Id { get; set; }

    public long Name { get; set; }

    public long Moniker { get; set; }

    public long Version { get; set; }

    public long Channel { get; set; }

    public long Pathpart { get; set; }

    public byte[]? Hash { get; set; }

    public long? ArpMinVersion { get; set; }

    public long? ArpMaxVersion { get; set; }
}
