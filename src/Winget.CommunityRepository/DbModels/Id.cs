using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Id
{
    public long Rowid { get; set; }

    public string Id1 { get; set; } = null!;
}
