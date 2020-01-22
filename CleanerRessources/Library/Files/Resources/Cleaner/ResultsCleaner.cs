using Library.MVVM.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Library.Files.Resources.Cleaner
{
    public static class ResultsCleaner
    {
        public static Dictionary<string, ResultKey> Keys { get; } = new Dictionary<string, ResultKey>();
        public static Dictionary<string, ResultFile> Files { get; } = new Dictionary<string, ResultFile>();
        public static List<KeyModel> KeysNotUsed { get; } = new List<KeyModel>();
    }

    public class ResultKey
    {
        public ResultKey(string key) => Key = key;        
        public string Key { get; private set; }
        public Dictionary<string, List<ResultLine>> Values { get; } = new Dictionary<string, List<ResultLine>>();
    }

    public class ResultFile
    {
        public ResultFile(string file) => File = file;
        public string File { get; private set; }
        public Dictionary<string, List<ResultLine>> Values { get; } = new Dictionary<string, List<ResultLine>>();
    }

    public class KeyModel : BaseViewModel
    {
        public KeyModel(string key, string label, string file)
        {
            Key = key;
            Label = label;
            File = file;
        } 
        public string Key { get; private set; }
        public string Label { get; private set; }
        public string File { get; private set; }
        public bool Selected { get => Get<bool>(); set => Set(value); }
        public bool NotUsed { get => Get<bool>(); set => Set(value); }
    }

    public class ResultLine
    {
        public int Num { get; set; }
        public string Line { get; set; }
    }
}
