using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows.Forms;

namespace Library.Files.Resources
{
    public static class Resource
    {
        public static void ReadRessourceFile(string filename)
        {
            //Requires Assembly System.Windows.Forms '
            ResXResourceReader rsxr = new ResXResourceReader(filename);

            // Iterate through the resources and display the contents to the console. '                
            IDictionaryEnumerator dict = rsxr.GetEnumerator();
            while (dict.MoveNext())
            {
                string val = null;
                try
                {
                    val = (dict.Value as ResXDataNode)?.GetValue((ITypeResolutionService)null).ToString();
                }
                catch { }
                Console.WriteLine(dict.Key.ToString() + ":" + val?.Substring(0, val.Length > 1000 ? 100 : val.Length));
            }            

            //Close the reader. '
            rsxr.Close();
        }

        public static IEnumerable<Tuple<string, string>> GetKeysResources(string filename)
        {
            //Requires Assembly System.Windows.Forms '
            ResXResourceReader rsxr = new ResXResourceReader(filename) { UseResXDataNodes = true };
                                      
            IDictionaryEnumerator dict = rsxr.GetEnumerator();
            while (dict.MoveNext())
            {
                string val= null;
                try
                {
                    val = (dict.Value as ResXDataNode)?.GetValue((ITypeResolutionService)null).ToString();
                }
                catch { }                
                yield return Tuple.Create(dict.Key.ToString(), val?.Substring(0, val.Length > 1000 ? 100: val.Length));
            }

            //Close the reader. '
            rsxr.Close();
        } 
        
        public static void DeleteKey(string filename, IEnumerable<string> keys)
        {
            //Requires Assembly System.Windows.Forms '
            using (ResXResourceReader rsxr = new ResXResourceReader(filename) { UseResXDataNodes = true })
            {                
                var logs = new List<string>();

                using (var rsxw = new ResXResourceWriter(filename+".new"))
                {
                    try
                    {
                        foreach (DictionaryEntry resourceEntry in rsxr)
                        {
                            string currentKey = (string)resourceEntry.Key;
                            if (!keys.Contains(currentKey))
                                rsxw.AddResource(currentKey, resourceEntry.Value);
                            else
                                logs.Add(currentKey);
                        }                            
                    }
                    finally
                    {
                        //Close the writer. '
                        rsxw.Close();

                        //Close the reader. '
                        rsxr.Close();
                    }
                }

                if(MessageBox.Show("\n\t- "+ string.Join("\n\t- ", logs), "Voici les Keys supprimé", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (File.Exists(filename + ".old"))
                        File.Delete(filename + ".old");
                    File.Move(filename, filename + ".old");
                    File.Move(filename + ".new", filename);

                    UpdateResourceDesign(filename);                    
                }
            }
        }

        public static void UpdateResourceDesign(string filename)
        {            
            var codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-EN");

            var Namespace = GetNameSpace(filename);
            
            var currentPath = Directory.GetCurrentDirectory();

            // pour qu'il puissent trouver les fichiers contenu dans les resources (exemple icône)
            Directory.SetCurrentDirectory(Path.GetDirectoryName(filename));

            // Needs System.Design.dll
            System.CodeDom.CodeCompileUnit code = System.Resources.Tools.StronglyTypedResourceBuilder.Create( Path.GetFileName(filename),
                                                                                                              Path.GetFileNameWithoutExtension(filename),
                                                                                                              Namespace, 
                                                                                                              codeProvider,
                                                                                                              false, 
                                                                                                              out string[] unmatchedElements);

            var fileinfos = new FileInfo(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".Designer.cs"));
            if (fileinfos.Exists && fileinfos.Length == 0)
                return;

            using (StreamWriter writer = new StreamWriter(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".Designer.cs"), false, Encoding.UTF8))
            {
                codeProvider.GenerateCodeFromCompileUnit(code, writer, new System.CodeDom.Compiler.CodeGeneratorOptions());
            }

            Directory.SetCurrentDirectory(currentPath);
        }

        private static string GetNameSpace(string filename)
        {
            var path = Path.GetDirectoryName(filename);            
            return path.Substring(Directory.GetParent(Directory.GetParent(path).FullName).FullName.Length+1).Replace('\\','.');
        }
    }
}
