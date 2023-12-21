using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Pathpart
{
    public long Rowid { get; set; }

    public long? Parent { get; set; }

    public string Pathpart1 { get; set; } = null!;
}
