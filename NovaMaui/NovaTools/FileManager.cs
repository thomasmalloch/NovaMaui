using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaTools
{
    public class FileManager
    {
        public static async Task<Stream> OpenFile(string filename) 
        {
#if WINDOWS
            return await FileSystem.OpenAppPackageFileAsync("Assets/" + filename);
#else
            return await FileSystem.OpenAppPackageFileAsync(filename);
#endif
        }
    }
}
