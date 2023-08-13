using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetIntune;
public interface IAzureFileUploader
{
    Task UploadFileToAzureAsync(string filename, Uri sasUri, CancellationToken cancellationToken);
}
