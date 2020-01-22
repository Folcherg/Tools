using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Library.MVVM.ViewModel;
using Library.MVVM.Command;
using Library.Files.Resources;
using Library.Files;
using Library.Files.Resources.Cleaner;

namespace InterfaceCleanerRessource
{
    public class MainModel : BaseViewModel
    {
        public bool ProcessRuning { get => Get<bool>(); set => Set(value); }
        public int Pourcent { get => Get<int>(); set => Set(value); }
        public string PathSolution { get => Get<string>(); set => Set(value); }
        public string ProjectCurrent { get => Get<string>(); set => Set(value); }        
        public ObservableCollection<KeyModel> ListKeyNotUsed { get; } = new ObservableCollection<KeyModel>();
        public IEnumerable<KeyModel> SelectKeys { get; } = new ObservableCollection<KeyModel>();
        private List<KeyModel> AllKeys { get; } = new List<KeyModel>();
        public int CountAllKeys => AllKeys.Count;
        public int CountKeysNotUsers => ListKeyNotUsed.Count;

        #region Cmd
        public ICommand SelectDirectoryCmd { get; } = new RelayCommand(SelectDirectory, o => o is MainModel model);
        private static void SelectDirectory(object obj)
        {
            var model = (MainModel)obj;
            using (var folderBrowserDialog = new FolderBrowserDialog())                                         
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    model.PathSolution = folderBrowserDialog.SelectedPath;            
        }

        public ICommand DeleteKeyCmd => new RelayCommand(DeleteKey, o => o is MainModel);
        private async void DeleteKey(object obj)
        {
            var model = (MainModel)obj;

            var grp = model.ListKeyNotUsed.Where(i => i.Selected).GroupBy(g => g.File).ToArray();
            foreach (var g in grp)           
                Resource.DeleteKey(g.Key, g.Select(r => r.Key));           
            

            foreach (var key in model.ListKeyNotUsed.Where(i => i.Selected).ToArray())
            {
                await Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate () { ListKeyNotUsed.Remove(key); });
            }
        }

        public ICommand SearchKeyNotUsedCmd { get; } = new RelayCommand(ProcessSearch, o => o is MainModel model ? Directory.Exists(model.PathSolution) : false);
        private async static void ProcessSearch(object obj)
        {
            var model = (MainModel)obj;
            model.ProcessRuning = true;
            //var path = @"D:\Projets\WallpaperRubric\Tilboard";            
            var pathMaster = model.PathSolution;            
            foreach (var path in Directory.GetDirectories(pathMaster))
            {
                model.ProjectCurrent = path.Substring(path.LastIndexOf('\\')+1);
                // Load Keys                 
                var filesRes = HelperFiles.GetFileInfos(new[] { "*.resx" }, Path.Combine(path, "Properties"));
                var keysnotused = new List<KeyModel>();
                foreach (var file in filesRes)
                    foreach (var key in Resource.GetKeysResources(file.FullName))
                        keysnotused.Add(new KeyModel(key.Item1, key.Item2, file.FullName) { NotUsed = true });                

                // search Keys not used
                var files = HelperFiles.GetFileInfos(new[] { "*.cs", "*.xaml" }, path).Where(f => !f.FullName.TrimEnd().EndsWith(".Designer.cs"));
                var count = files.Count();
                var index = 0;
                model.Pourcent = 0;
                foreach (var file in files)
                {
                    foreach (var key in keysnotused.Where(k => k.NotUsed).ToArray())
                        if (HelperCleaner.AnyKeyUsedInFile(key, file.FullName))
                            key.NotUsed = false;

                    index++;
                    var newpourcent = index * 100 / count;
                    if (model.Pourcent < newpourcent)
                        await Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate () { model.Pourcent = newpourcent; }, DispatcherPriority.Background);                                                                   
                }

                foreach (var key in keysnotused.Where(k => k.NotUsed).GroupBy(k => k.File))
                    foreach (var item in key)
                    {
                        var keyNotUsed = item;
                        ResultsCleaner.KeysNotUsed.Add(keyNotUsed);
                        await Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate () { model.AddKeyNotUsed(keyNotUsed); }, DispatcherPriority.Background);
                    }

                model.AddKeys(keysnotused);                
            }

            model.ProcessRuning = false;
        }

        private void AddKeyNotUsed(KeyModel keyNotUsed)
        {
            ListKeyNotUsed.Add(keyNotUsed);
            ChangedProperty(nameof(CountKeysNotUsers));
        }

        private void AddKeys(List<KeyModel> keysnotused)
        {
            AllKeys.AddRange(keysnotused);
            ChangedProperty(nameof(CountAllKeys));
        }
        #endregion

        public MainModel()
        {
#if DEBUG
            PathSolution = @"D:\Projets\WallpaperRubric";
            //PathSolution = @"C:\Users\gfr\source\repos\tilboard\Tilbury.Common.Wpf";
#endif
        }
    }
}
