using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Files
{
    public static class HelperFiles
    {
        public static IEnumerable<FileInfo> GetFileInfos(IEnumerable<string> searchpatterns, string path)
        {            
            var listFiles = new List<FileInfo>();
            if (!Directory.Exists(path))
                return listFiles;
            foreach (var search in searchpatterns)
                listFiles.AddRange(Directory.GetFiles(path, search).Select(f => new FileInfo(f)));
            foreach (var dir in Directory.GetDirectories(path))            
                listFiles.AddRange(GetFileInfos(searchpatterns, dir));
            
            return listFiles;
        }

        
    }
}
