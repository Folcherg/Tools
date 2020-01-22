using Library.Files;
using Library.Files.Resources;
using Library.Files.Resources.Cleaner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanerRessources
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG            
            var pathMaster = @"D:\Projets\Test";            
#else
            var pathMaster = args[0];
#endif
            foreach (var path in Directory.GetDirectories(pathMaster))
            {
                Console.WriteLine("*****************************");
                Console.WriteLine(path);
                Console.WriteLine("*****************************");

                // Load Keys 
                var filesRes = HelperFiles.GetFileInfos(new[] { "*.resx" }, Path.Combine(path,"Properties"));
                var keysnotused = new List<KeyModel>();
                var count = filesRes.Sum(f => Resource.GetKeysResources(f.FullName).Count());
                var index = 0;
                var pourcent = 0;
                Console.WriteLine($"Chargement des clés: 0 %");
                foreach (var file in filesRes)
                    foreach (var key in Resource.GetKeysResources(file.FullName))
                    {
                        keysnotused.Add(new KeyModel(key.Item1, key.Item2, file.FullName) { NotUsed = true });

                        index++;
                        var newpourcent = index * 100 / count;
                        if (pourcent < newpourcent)
                        {
                            pourcent = newpourcent;
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Console.WriteLine($"Chargement des clés: {pourcent} %");
                        }
                    }


                // search Keys not used
                var files = HelperFiles.GetFileInfos(new[] { "*.cs", "*.xaml" }, path).Where( f => !f.FullName.TrimEnd().EndsWith(".Designer.cs"));
                count = files.Count();
                index = 0;
                pourcent = 0;
                Console.WriteLine($"Recherche des clés inutilisées: 0 %");
                foreach (var file in files)
                {
                    foreach (var key in keysnotused.Where(k => k.NotUsed).ToArray())
                        if (HelperCleaner.AnyKeyUsedInFile(key, file.FullName))
                            key.NotUsed = false;

                    index++;
                    var newpourcent = index * 100 / count;
                    if (pourcent < newpourcent)
                    {
                        pourcent = newpourcent;
                        //var pos = Console.CursorTop;
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.WriteLine($"Recherche des clés inutilisées: {pourcent} %");                        
                    }
                }

                // display results
                Console.WriteLine("clé non utilisé: " + keysnotused.Count(k => k.NotUsed));
                Console.WriteLine("clé valide: " + keysnotused.Count(k => !k.NotUsed));
                foreach (var key in keysnotused.Where(k => k.NotUsed).GroupBy(k => k.File))
                {
                    Console.WriteLine(key.Key);
                    foreach (var item in key)
                    {
                        ResultsCleaner.KeysNotUsed.Add(item);
                        Console.WriteLine($"\t - {item.Key} => {item.Label}");
                    }
                }                
            }

            // display results
            Console.WriteLine("clé non utilisé: " + ResultsCleaner.KeysNotUsed.Count());
            Console.WriteLine("clé valide: " + ResultsCleaner.Keys.Count());

            using (var sw = new StreamWriter(File.Open(Path.Combine(pathMaster, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "KeyError.csv"), FileMode.CreateNew), Encoding.Unicode))
            {
                try
                {
                    sw.WriteLine($"Fichier ; Clé ; Libellé");
                    foreach (var key in ResultsCleaner.KeysNotUsed.OrderBy(k => k.File))
                        sw.WriteLine($"{key.File}; {key.Key} ; {key.Label}");
                }
                finally
                {
                    sw.Close();
                }
            }

            using (var sw = new StreamWriter(File.Open(Path.Combine(pathMaster, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "KeyValid.csv"), FileMode.CreateNew), Encoding.Unicode))
            {
                try
                {
                    sw.WriteLine($" Clé ; Fichier ; numéro ligne ; ligne");
                    foreach (var file in ResultsCleaner.Files.OrderBy(f => f.Key))
                        foreach (var key in file.Value.Values)
                            foreach (var item in key.Value)
                                sw.WriteLine($"{key.Key}; {file.Key} ; {item.Num} ; {item.Line}");

                    sw.Close();
                }
                finally
                {
                    sw.Close();
                }
            }

            Console.WriteLine("Press Key for finish...");
            Console.ReadKey();
        }
    }
}
