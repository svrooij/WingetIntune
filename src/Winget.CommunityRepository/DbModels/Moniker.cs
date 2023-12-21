using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Moniker
{
    public long Rowid { get; set; }

    public string Moniker1 { get; set; } = null!;
}
