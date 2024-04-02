using OptiMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace OptiMate.ViewModels
{
    public class ConfirmationViewModel : ObservableObject
    {
        public string ConfirmationText { get; set; }
        public bool UserConfirmed { get; private set; } = false;

        private DelegateCommand _runOnConfirmation;
        private Object _runOnConfirmationParameter;
        public ConfirmationViewModel(string confirmationText, DelegateCommand runOnConfirmation, object runOnConfirmationParameter)
        {
            ConfirmationText = confirmationText;
            _runOnConfirmation = runOnConfirmation;
            _runOnConfirmationParameter = runOnConfirmationParameter;
        }

        public ICommand ConfirmCommand
        {
            get { return new DelegateCommand(Confirm); }
        }

        private void Confirm(object obj)
        {
            UserConfirmed = true;
            _runOnConfirmation.Execute(_runOnConfirmationParameter);
            (obj as Popup).IsOpen = false;
        }
    }
}
