using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Upgradecode
{
    public long Rowid { get; set; }

    public string Upgradecode1 { get; set; } = null!;
}
