using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Version
{
    public long Rowid { get; set; }

    public string Version1 { get; set; } = null!;
}
