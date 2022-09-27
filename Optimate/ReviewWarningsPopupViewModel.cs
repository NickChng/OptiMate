using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PropertyChanged;
using System.IO;
using System.Xml.Serialization;

namespace Optimate
{
    public class ReviewWarningsViewModel : ObservableObject
    {

        public ObservableCollection<string> Warnings { get; set; } = new ObservableCollection<string>();
        public ReviewWarningsViewModel(List<string> warnings)
        {
            Warnings = new ObservableCollection<string>(warnings);
            RaisePropertyChangedEvent(nameof(Warnings));
        }

        public void SetWarnings(List<string> warnings)
        {
            Warnings = new ObservableCollection<string>(warnings);
            RaisePropertyChangedEvent(nameof(Warnings));
        }
        public ReviewWarningsViewModel()
        {
            var warnings = new List<string>() { "Design", "Time", "Debug" };
            Warnings = new ObservableCollection<string>(warnings);
        }

    }
}