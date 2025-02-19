using System.ComponentModel.DataAnnotations;

namespace Winget.CommunityRepository.DbModels;

public partial class Tag
{
    [Key]
    public long Rowid { get; set; }

    public string Tag1 { get; set; } = null!;
    public virtual ICollection<Manifest> Manifests { get; set; }
}
