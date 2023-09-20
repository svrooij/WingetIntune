using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class NormNamesMap
{
    public long Manifest { get; set; }

    public long NormName { get; set; }
}
