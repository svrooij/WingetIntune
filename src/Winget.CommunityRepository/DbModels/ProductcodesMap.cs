using System;
using System.Collections.Generic;

namespace Winget.CommunityRepository.DbModels;

public partial class ProductcodesMap
{
    public long Manifest { get; set; }

    public long Productcode { get; set; }
}
