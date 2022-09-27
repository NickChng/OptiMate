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
using System.ComponentModel;
using System.Windows;

namespace Optimate
{
    public class ReviewWarningsViewModel : ObservableObject
    {

        public List<string> Warnings { get; set; } = new List<string>();
       
        public void SetWarnings(List<string> warnings)
        {
            Warnings = new List<string>(warnings);
            RaisePropertyChangedEvent(nameof(Warnings));
        }
        public ReviewWarningsViewModel()
        {
            try
            {
                var warnings = new List<string>() { "Design", "Time", "Debug" };
                foreach (string warning in warnings)
                {
                    Warnings.Add(warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}