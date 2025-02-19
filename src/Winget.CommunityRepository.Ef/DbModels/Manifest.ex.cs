namespace Winget.CommunityRepository.DbModels;
public partial class Manifest
{
#nullable disable
    public virtual Id IdValue { get; set; }
    public virtual Name NameValue { get; set; }
    public virtual Version VersionValue { get; set; }
    public virtual ICollection<Tag> Tags { get; set; }
}

