using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Library.Files.Resources.Cleaner
{
    public static class HelperCleaner
    {
        public static bool AnyKeyUsedInFile(KeyModel key, string filename)
        {
            if (!ResultsCleaner.Keys.ContainsKey(key.Key))
                ResultsCleaner.Keys.Add(key.Key, new ResultKey(key.Key));
            if(!ResultsCleaner.Keys[key.Key].Values.ContainsKey(filename))
                ResultsCleaner.Keys[key.Key].Values.Add(filename, new List<ResultLine>());

            if (!ResultsCleaner.Files.ContainsKey(filename))
                ResultsCleaner.Files.Add(filename, new ResultFile(filename));
            if (!ResultsCleaner.Files[filename].Values.ContainsKey(key.Key))
                ResultsCleaner.Files[filename].Values.Add(key.Key, new List<ResultLine>());

            if (!File.Exists(filename))
                return false;

            using (var sr = new StreamReader(filename))
            {
                var regex = new Regex($@"\b{key.Key}\b");

                var regexResManage = new Regex($"ResourceManager");
                var ResourceLg = Path.GetFileNameWithoutExtension(key.File).LastIndexOf('.');
                var regexRes = new Regex($"{(ResourceLg>0? Path.GetFileNameWithoutExtension(key.File).Substring(0, ResourceLg): Path.GetFileNameWithoutExtension(key.File))}.{key.Key}");
                string line = sr.ReadLine();
                int indexline = 1;
                //bool comment = false;
                while(!sr.EndOfStream)
                {
                    //if (/*line.Contains(@"/*") ||*/ line.Contains(@"<!--"))
                    //    comment = true;                     

                    //if (/*line.Contains(@"/") ||*/ line.Contains(@"-->"))                    
                    //    comment = false;

                    if (line == null || line.TrimStart().StartsWith(@"//") /*|| comment*/)
                    {
                        line = sr.ReadLine();
                        indexline++;
                        continue;
                    }

                    if (regex.IsMatch(line) && (regexResManage.IsMatch(line) || regexRes.IsMatch(line)))
                    {
                        ResultsCleaner.Keys[key.Key].Values[filename].Add(new ResultLine { Num = indexline, Line = line });
                        ResultsCleaner.Files[filename].Values[key.Key].Add(new ResultLine { Num = indexline, Line = line });
                        return true;
                    }

                    line = sr.ReadLine();
                    indexline++;
                }
            }

            return false;
        }

        public static bool AnyKeyUsedInPath(KeyModel key, string path)
        {
            foreach (var file in HelperFiles.GetFileInfos(new[] { "*.*"/*, "*.cs", ".xaml"*/ }, path))
                if (AnyKeyUsedInFile(key, file.FullName))
                    return true;

            return false;
        }

        public static IEnumerable<string> GetKeyUsedInFile(string key, Stream stream)
        {            
            using (var sr = new StreamReader(stream))
            {
                var regex = new Regex($@"\b{key}\b");

                var regexResManage = new Regex($"ResourceManager");
                var regexRes = new Regex($"Resources.{key}");
                string line = sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    if (line == null || line.TrimStart().StartsWith("\\")) continue;

                    if (regex.IsMatch(line) && (regexResManage.IsMatch(line) || regexRes.IsMatch(line)))
                        yield return line;

                    line = sr.ReadLine();
                }
            }
        }

        public static IEnumerable<IEnumerable<string>> GetKeyUsedInPath(string key, string path)
        {
            foreach (var file in HelperFiles.GetFileInfos(new[] { "*.cs", ".xaml" }, path))
                yield return GetKeyUsedInFile(key, File.Open(file.FullName, FileMode.Open));                                
        }        
    }
}
