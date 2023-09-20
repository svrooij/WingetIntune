using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Pfn
{
    public long Rowid { get; set; }

    public string Pfn1 { get; set; } = null!;
}
