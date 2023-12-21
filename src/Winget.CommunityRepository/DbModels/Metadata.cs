using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class Metadata
{
    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;
}
