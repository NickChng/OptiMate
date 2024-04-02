using OptiMate;
using OptiMate.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
using Prism.Events;
using OptiMate.ViewModels;
using OptiMate.Logging;

namespace OptiMate.ViewModels
{

    public class GeneratedStructureViewModel : ObservableObject
    {
        private GeneratedStructure _generatedStructure;
        private MainModel _model;
        private IEventAggregator _ea;
        public GeneratedStructureViewModel(GeneratedStructure genStructure, MainModel model, IEventAggregator ea, bool isNew = false)
        {
            _generatedStructure = genStructure;
            EditMode = isNew;
            _model = model;
            _ea = ea;
            foreach (var instruction in genStructure.Instructions.Items)
            {
                InstructionViewModels.Add(new InstructionViewModel(instruction, genStructure, _model, _ea));
            }
            RegisterEvents();
        }

        private void RegisterEvents()
        {
           _ea.GetEvent<RemovingInstructionViewModelEvent>().Subscribe(OnRemovingInstructionViewModel);
        }

        private void OnRemovingInstructionViewModel(InstructionViewModel ivm)
        {
            if (InstructionViewModels.Contains(ivm))
            {
                InstructionViewModels.Remove(ivm);
                RaisePropertyChangedEvent(nameof(InstructionViewModels));
                _ea.GetEvent<DataValidationRequiredEvent>().Publish();
            }
        }

        public GeneratedStructureViewModel()
        {
            _generatedStructure = new GeneratedStructure() { StructureId = "DesignNameMaxLth" };
            InstructionViewModels = new ObservableCollection<InstructionViewModel>();
            InstructionViewModels.Add(new InstructionViewModel());

        }
        public bool EditMode { get; private set; } = false;

        public string StructureId
        {
            get
            {
                return _generatedStructure.StructureId;
            }
            set
            {
                if (value != _generatedStructure.StructureId)
                {
                    _model.RenameGeneratedStructure(_generatedStructure.StructureId, value);
                    RaisePropertyChangedEvent(nameof(StructureId));
                }
            }
        }

        public ObservableCollection<InstructionViewModel> InstructionViewModels { get; set; } = new ObservableCollection<InstructionViewModel>();

        private bool isStructureIdValid
        {
            get
            {
                return !HasError(nameof(StructureId));
            }
        }

        public string StructureIdError
        {
            get
            {
                var errors = GetErrors(nameof(StructureId));
                if (errors == null)
                    return null;
                else
                    return string.Join("\r", (List<string>)errors);
            }
        }
        public SolidColorBrush StructureIdColor
        {
            get
            {
                if (isStructureIdValid)
                    return new SolidColorBrush(Colors.White);
                else
                    return new SolidColorBrush(Colors.Orange);
            }
        }

        public bool ConfirmRemoveStructurePopupVisibility { get; set; }

        public ICommand AddInstructionCommand
        {
            get
            {
                return new DelegateCommand(AddInstruction);
            }
        }
        public void AddInstruction(object param = null)
        {
            var priorInstruction = (param as object[])[0] as InstructionViewModel;
            int index;
            try
            {
                index = InstructionViewModels.IndexOf(priorInstruction);
            }
            catch (Exception e)
            {
                SeriLogModel.AddError("Could not find prior instruction when inserting new instruction, using index=0", e);
                index = 0;
            }
            var newInstruction = _model.AddInstruction(_generatedStructure, OperatorTypes.undefined, index+1);
            InstructionViewModels.Insert(index + 1, new InstructionViewModel(newInstruction, _generatedStructure, _model, _ea));
            RaisePropertyChangedEvent(nameof(InstructionViewModels));
            _ea.GetEvent<DataValidationRequiredEvent>().Publish();
            SeriLogModel.AddLog($"Added new instruction to {_generatedStructure.StructureId}");
        }

        public ICommand RemoveInstructionCommand
        {
            get
            {
                return new DelegateCommand(RemoveInstruction);
            }
        }

        public int NumInstructions
        {
            get
            {
                return InstructionViewModels.Count;
            }
        }

        public void RemoveInstruction(object param = null)
        {
            var ivm = (param as object[])[0] as InstructionViewModel;
            ivm.RemoveInstruction();
        }

        public ICommand EditGenStructureCommand
        {
            get
            {
                return new DelegateCommand(ToggleEditMode);
            }
        }

        private void ToggleEditMode(object obj = null)
        {
            EditMode ^= true;
        }

        internal bool ValidateInputs(List<string> aggregateWarnings)
        {
            bool isValid = true;
            if (HasErrors)
            {
                aggregateWarnings.AddRange(GetAllErrors());
                isValid = false;
            }
            foreach (var ivm in InstructionViewModels)
                if (ivm.HasErrors)
                {
                    aggregateWarnings.AddRange(ivm.GetAllErrors());
                    isValid = false;
                }
            return isValid;
        }
    }

}
