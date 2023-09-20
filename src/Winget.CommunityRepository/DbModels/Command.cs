using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Command
{
    public long Rowid { get; set; }

    public string Command1 { get; set; } = null!;
}
