using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Name
{
    public long Rowid { get; set; }

    public string Name1 { get; set; } = null!;
}
