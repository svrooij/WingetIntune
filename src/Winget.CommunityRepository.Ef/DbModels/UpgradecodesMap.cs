using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class UpgradecodesMap
{
    public long Manifest { get; set; }

    public long Upgradecode { get; set; }
}
