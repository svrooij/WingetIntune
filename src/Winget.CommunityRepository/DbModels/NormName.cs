using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class NormName
{
    public long Rowid { get; set; }

    public string NormName1 { get; set; } = null!;
}
