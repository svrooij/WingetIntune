using System.ComponentModel.DataAnnotations;

namespace Winget.CommunityRepository.DbModels;

public partial class Id
{
    [Key]
    public long Rowid { get; set; }

    public string Id1 { get; set; } = null!;
}
