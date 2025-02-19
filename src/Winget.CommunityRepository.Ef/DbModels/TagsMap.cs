using System.ComponentModel.DataAnnotations.Schema;

namespace Winget.CommunityRepository.DbModels;

public partial class TagsMap
{
    public long Manifest { get; set; }

    public long Tag { get; set; }

    [ForeignKey(nameof(Manifest))]
    public Manifest ManifestValue { get; set; }

    [ForeignKey(nameof(Tag))]
    public Tag TagValue { get; set; }
}
