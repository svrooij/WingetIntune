using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winget.CommunityRepository.DbModels;
public partial class Manifest
{
    public virtual Name NameValue { get; set; }
    public virtual Version VersionValue { get; set; }
    //    public virtual Id IdValue { get; set; }
}

