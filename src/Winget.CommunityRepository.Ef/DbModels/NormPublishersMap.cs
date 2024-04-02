using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class NormPublishersMap
{
    public long Manifest { get; set; }

    public long NormPublisher { get; set; }
}
