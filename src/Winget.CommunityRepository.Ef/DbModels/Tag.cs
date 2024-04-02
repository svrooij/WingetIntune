using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Tag
{
    public long Rowid { get; set; }

    public string Tag1 { get; set; } = null!;
}
