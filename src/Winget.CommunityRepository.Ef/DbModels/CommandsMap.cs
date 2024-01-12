using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class CommandsMap
{
    public long Manifest { get; set; }

    public long Command { get; set; }
}
