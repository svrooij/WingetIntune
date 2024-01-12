using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Channel
{
    public long Rowid { get; set; }

    public string Channel1 { get; set; } = null!;
}
