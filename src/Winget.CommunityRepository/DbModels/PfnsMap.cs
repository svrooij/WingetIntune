using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class PfnsMap
{
    public long Manifest { get; set; }

    public long Pfn { get; set; }
}
