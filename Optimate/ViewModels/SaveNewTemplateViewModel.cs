using OptiMate.Controls;
using OptiMate.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace OptiMate.ViewModels
{
    public class SaveNewTemplateViewModel : ObservableObject
    {
        private IEventAggregator _ea;
        private MainModel _model;

        

        public SaveNewTemplateViewModel(IEventAggregator ea, MainModel model, string defaultNewProtocolName)
        {
            _ea = ea;
            _model = model;
            NewTemplateDisplayName = defaultNewProtocolName;
        }
        public string NewTemplateDisplayName { get; set; }

        public ICommand SaveCommand
        {
            get
            {
                return new DelegateCommand(Save);
            }
        }

        private void Save(object obj)
        {
            _model.SaveTemplateToPersonal(NewTemplateDisplayName);
            (obj as Popup).IsOpen = false;
            
//SaveToPersonalResult result = _model.SaveTemplateToPersonal();
            //if (result == SaveToPersonalResult.Success)
            //{
            //    StatusMessage = "Template successfully saved to personal templates.";
            //}
            //else if (result == SaveToPersonalResult.Failure)
            //{
            //    MessageBox.Show("Failed to save current template.");
            //}
        }
    }
}
