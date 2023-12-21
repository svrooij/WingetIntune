using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class TagsMap
{
    public long Manifest { get; set; }

    public long Tag { get; set; }
}
