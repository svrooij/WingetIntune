using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Productcode
{
    public long Rowid { get; set; }

    public string Productcode1 { get; set; } = null!;
}
