using Optimate;
using Optimate.ViewModels;
using OptiMate.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static OptiMate.ViewModels.TemplateStructureViewModel;

namespace OptiMate.ViewModels
{
    public enum OperatorTypes
    {
        undefined,
        copy,
        margin,
        asymmetricMargin,
        or,
        and,
        crop,
        sub,
        subfrom,
        convertDose
    }

    public class InstructionViewModel : ObservableObject
    {
        private MainModel _model;
        private IEventAggregator _ea;
        private Instruction _instruction;
        private GeneratedStructure _parentGeneratedStructure;
        public SometimesObservableCollection<OperatorTypes> Operators { get; set; } = new SometimesObservableCollection<OperatorTypes>()
        {
              OperatorTypes.copy,
              OperatorTypes.margin,
              OperatorTypes.asymmetricMargin,
              OperatorTypes.or,
              OperatorTypes.and,
              OperatorTypes.crop,
              OperatorTypes.sub,
              OperatorTypes.subfrom,
              OperatorTypes.convertDose
        };
        public SometimesObservableCollection<MarginTypes> MarginTypeOptions { get; set; } = new SometimesObservableCollection<MarginTypes>()
        {
            MarginTypes.Outer,
            MarginTypes.Inner
        };

        private OperatorTypes _selectedOperator;
        public OperatorTypes SelectedOperator

        {
            get
            {
                return _selectedOperator;
            }
            set
            {
                if (_selectedOperator != value)
                {
                    isModified = true;
                    _instruction = _model.ReplaceInstruction(_parentGeneratedStructure, _instruction, value);
                    InitializeViewModel();
                    _selectedOperator = value; // order of setter matters because we don't want to invoke property changed events before the instruction is updated
                    ValidateOperator();
                }
            }
        }
        public bool isCopy { get; set; } = false;
        public bool isOr { get; set; } = false;
        public bool isAnd { get; set; } = false;
        public bool isSub { get; set; } = false;
        public bool isMargin { get; set; } = false;
        public bool isAsymmetricMargin { get; set; } = false;
        public bool isConvertDose { get; set; } = false;
        public bool isCrop { get; set; } = false;

        public string marginValueToolTip
        {
            get
            {
                return "Enter a value between 0 and 50";
            }
        }
        public string AntMargin
        {
            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    return (_instruction as AsymmetricMargin)?.AntMargin;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    if (isMarginValueValid(value))
                        (_instruction as AsymmetricMargin).AntMargin = value;
                }
            }
        }
        public string PostMargin
        {

            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    return (_instruction as AsymmetricMargin)?.PostMargin;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as AsymmetricMargin).PostMargin = value;
                        isModified = true;
                    }
                }
            }
        }
        public string SupMargin
        {
            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    return (_instruction as AsymmetricMargin)?.SupMargin;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as AsymmetricMargin).SupMargin = value;
                        isModified = true;
                    }
                }
            }
        }
        public string InfMargin
        {
            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    return (_instruction as AsymmetricMargin)?.InfMargin;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as AsymmetricMargin).InfMargin = value;
                        isModified = true;
                    }
                }
            }
        }
        public string LeftMargin
        {
            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    return (_instruction as AsymmetricMargin)?.LeftMargin;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as AsymmetricMargin).LeftMargin = value;
                        isModified = true;
                    }
                }
            }
        }
        public string RightMargin
        {
            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    return (_instruction as AsymmetricMargin)?.RightMargin;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricMargin)
                {
                    ClearErrors(nameof(RightMargin));
                    if (isMarginValueValid(value))
                    {
                        (_instruction as AsymmetricMargin).RightMargin = value;
                        isModified = true;
                    }
                }
            }
        }

        private bool isMarginValueValid(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }
            if (int.TryParse(value, out int result))
            {
                if (result < 0 || result > 50)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public string IsoMargin
        {
            get
            {
                if (SelectedOperator == OperatorTypes.margin)
                {
                    return (_instruction as Margin)?.IsotropicMargin;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (SelectedOperator == OperatorTypes.margin)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as Margin).IsotropicMargin = value;
                        isModified = true;
                    }
                }
            }
        }

        public MarginTypes SelectedMargin
        {
            get
            {
                if (_instruction is Margin)
                {
                    var val = (_instruction as Margin).MarginType;
                    return val;
                }
                else if (_instruction is AsymmetricMargin)
                {
                    var val = (_instruction as AsymmetricMargin).MarginType;
                    return val;
                }
                else
                {
                    return MarginTypes.Outer;
                }
            }
            set
            {
                if (_instruction is Margin)
                {
                    (_instruction as Margin).MarginType = value;
                }
                else if (_instruction is AsymmetricMargin)
                {
                    (_instruction as AsymmetricMargin).MarginType = value;
                }
                isModified = true;
            }
        }

        public Visibility MarginVisibility
        {
            get
            {
                switch (SelectedOperator)
                {
                    case OperatorTypes.margin:
                        return Visibility.Visible;
                    default:
                        return Visibility.Collapsed;
                }
            }
        }

        public Visibility DeleteButtonVisibiilty
        {
            get
            {
                int index = _model.GetInstructionNumber(_parentGeneratedStructure.StructureId, _instruction);
                if (index == 0)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public Visibility AsymmetricMarginVisibility
        {
            get
            {
                switch (SelectedOperator)
                {
                    case OperatorTypes.asymmetricMargin:
                        return Visibility.Visible;
                    default:
                        return Visibility.Collapsed;
                }
            }
        }

        public bool InternalCrop
        {
            get
            {
                switch (SelectedOperator)
                {
                    case OperatorTypes.crop:
                        return (_instruction as Crop).InternalCrop;
                    default:
                        return false;
                }
            }
            set
            {
                switch (SelectedOperator)
                {
                    case OperatorTypes.crop:
                        (_instruction as Crop).InternalCrop = value;
                        isModified = true;
                        break;
                }
            }
        }

        private bool _isModified = false;
        public bool isModified
        {
            get
            {
                return _isModified;
            }
            set
            {
                _isModified = value;
                RaisePropertyChangedEvent(nameof(WarningVisibility_OperatorChanged));
            }
        }
        public string IsoCropOffset
        {
            get
            {
                if (SelectedOperator == OperatorTypes.crop)
                {
                    return (_instruction as Crop).IsotropicOffset;
                }
                else return "";
            }
            set
            {
                if (SelectedOperator == OperatorTypes.crop)
                {
                    (_instruction as Crop).IsotropicOffset = value;
                    isModified = true;
                }
            }
        }
        public Visibility CropVisibility
        {
            get
            {
                switch (SelectedOperator)
                {
                    case OperatorTypes.crop:
                        return Visibility.Visible;
                    default:
                        return Visibility.Collapsed;
                }
            }
        }

        public string DefaultTemplateStructureId { get; private set; }

        public Visibility WarningVisibility_TargetChanged
        {
            get
            {
                if (!string.Equals(DefaultTemplateStructureId, TargetTemplateStructureId, StringComparison.OrdinalIgnoreCase) && !isModified)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public Visibility VisibilityTargetSelection
        {
            get
            {
                switch (_selectedOperator)
                {
                    case OperatorTypes.margin:
                        return Visibility.Hidden;
                    case OperatorTypes.asymmetricMargin:
                        return Visibility.Hidden;
                    default:
                        return Visibility.Visible;
                }
            }
        }

        public Visibility WarningVisibility_OperatorChanged
        {
            get
            {
                if (isModified)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public string TargetTemplateStructureId
        {
            get
            {
                switch (_instruction)
                {
                    case Crop crop:
                        return crop.TemplateStructureId;
                    case And and:
                        return and.TemplateStructureId;
                    case Or or:
                        return or.TemplateStructureId;
                    case Sub sub:
                        return sub.TemplateStructureId;
                    case Copy copy:
                        return copy.TemplateStructureId;
                    default:
                        return "";
                }
            }
            set
            {
                switch (_instruction)
                {
                    case Crop crop:
                        crop.TemplateStructureId = value;
                        break;
                    case And and:
                        and.TemplateStructureId = value;
                        break;
                    case Or or:
                        or.TemplateStructureId = value;
                        break;
                    case Sub sub:
                        sub.TemplateStructureId = value;
                        break;
                    case Copy copy:
                        copy.TemplateStructureId = value;
                        break;
                    default:
                        return;
                }
                RaisePropertyChangedEvent(nameof(WarningVisibility_TargetChanged));
                ValidateTargetTemplateStructureId();
            }
        }

        private void ValidateTargetTemplateStructureId()
        {
            ClearErrors(nameof(TargetTemplateStructureId));
            if (instructionHasTarget())
            {
                if (string.IsNullOrEmpty(TargetTemplateStructureId))
                {
                    int instructionNumber = _model.GetInstructionNumber(_parentGeneratedStructure.StructureId, _instruction);
                    AddError(nameof(TargetTemplateStructureId), $"{_parentGeneratedStructure.StructureId} operator #{instructionNumber + 1} has an invalid target");
                }
            }
            RaisePropertyChangedEvent(nameof(TargetTemplateBackgroundColor));
        }

        private void ValidateOperator()
        {
            ClearErrors(nameof(SelectedOperator));
            switch (_selectedOperator)
            {
                case OperatorTypes.undefined:
                    AddError(nameof(SelectedOperator), $"{_parentGeneratedStructure.StructureId} has an invalid operator");
                    break;
                default:
                    break;
            }
            RaisePropertyChangedEvent(nameof(OperatorBackgroundColor));
            RaisePropertyChangedEvent(nameof(VisibilityTargetSelection));
        }
        public SolidColorBrush TargetTemplateBackgroundColor
        {
            get
            {
                if (HasError(nameof(TargetTemplateStructureId)))
                {
                    return new SolidColorBrush(Colors.DarkOrange);
                }
                else
                {
                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
                }
            }
        }

        public SolidColorBrush OperatorBackgroundColor
        {
            get
            {
                if (HasError(nameof(SelectedOperator)))
                {
                    return new SolidColorBrush(Colors.DarkOrange);
                }
                else
                {
                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
                }
            }
        }

        private bool instructionHasTarget()
        {
            switch (SelectedOperator)
            {
                case OperatorTypes.or:
                    return true;
                case OperatorTypes.and:
                    return true;
                case OperatorTypes.sub:
                    return true;
                case OperatorTypes.convertDose:
                    return false;
                case OperatorTypes.crop:
                    return true;
                case OperatorTypes.margin:
                    return false;
                case OperatorTypes.copy:
                    return true;
                case OperatorTypes.asymmetricMargin:
                    return false;
                default:
                    return false;
            }
        }

        public ObservableCollection<string> TargetTemplateIds
        {
            get
            {
                var ids = new ObservableCollection<string>(_model.GetAvailableTemplateTargetIds(_parentGeneratedStructure.StructureId));
                return ids;
            }
        }

        public Visibility TargetTemplateStructureVisibility
        {
            get
            {
                switch (SelectedOperator)
                {
                    case OperatorTypes.or:
                        return Visibility.Visible;
                    case OperatorTypes.and:
                        return Visibility.Visible;
                    case OperatorTypes.sub:
                        return Visibility.Visible;
                    case OperatorTypes.convertDose:
                        return Visibility.Collapsed;
                    case OperatorTypes.crop:
                        return Visibility.Visible;
                    case OperatorTypes.margin:
                        return Visibility.Collapsed;
                    case OperatorTypes.copy:
                        return Visibility.Collapsed;
                    default:
                        return Visibility.Collapsed;
                }
            }
        }

        public ObservableCollection<TemplateStructure> TemplateStructures { get; set; } = new ObservableCollection<TemplateStructure>();
        public InstructionViewModel(Instruction instruction, GeneratedStructure parentStructure, MainModel model, IEventAggregator ea)
        {
            _instruction = instruction;
            _parentGeneratedStructure = parentStructure;
            _model = model;
            _ea = ea;
            RegisterEvents();
            InitializeViewModel();
            ValidateOperator();
        }

        private void InitializeViewModel()
        {
            switch (_instruction)
            {
                case Or _:
                    isOr = true;
                    TargetTemplateStructureId = (_instruction as Or).TemplateStructureId;
                    DefaultTemplateStructureId = TargetTemplateStructureId;
                    _selectedOperator = OperatorTypes.or;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    break;
                case And _:
                    isAnd = true;
                    TargetTemplateStructureId = (_instruction as And).TemplateStructureId;
                    DefaultTemplateStructureId = TargetTemplateStructureId;
                    _selectedOperator = OperatorTypes.and;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    break;
                case Sub _:
                    isSub = true;
                    TargetTemplateStructureId = (_instruction as Sub).TemplateStructureId;
                    DefaultTemplateStructureId = TargetTemplateStructureId;
                    _selectedOperator = OperatorTypes.sub;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    break;
                case Margin _:
                    isMargin = true;
                    _selectedOperator = OperatorTypes.margin;
                    RaisePropertyChangedEvent(nameof(IsoMargin));
                    break;
                case AsymmetricMargin _:
                    isAsymmetricMargin = true;
                    _selectedOperator = OperatorTypes.asymmetricMargin;
                    RaisePropertyChangedEvent(nameof(RightMargin));
                    RaisePropertyChangedEvent(nameof(LeftMargin));
                    RaisePropertyChangedEvent(nameof(PostMargin));
                    RaisePropertyChangedEvent(nameof(AntMargin));
                    RaisePropertyChangedEvent(nameof(SupMargin));
                    RaisePropertyChangedEvent(nameof(InfMargin));
                    RaisePropertyChangedEvent(nameof(SelectedMargin));
                    break;
                case ConvertDose _:
                    isConvertDose = true;
                    _selectedOperator = OperatorTypes.convertDose;
                    break;
                case Copy _:
                    isCopy = true;
                    TargetTemplateStructureId = (_instruction as Copy).TemplateStructureId;
                    DefaultTemplateStructureId = TargetTemplateStructureId;
                    _selectedOperator = OperatorTypes.copy;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    break;
                case Crop _:
                    isCrop = true;
                    TargetTemplateStructureId = (_instruction as Crop).TemplateStructureId;
                    DefaultTemplateStructureId = TargetTemplateStructureId;
                    _selectedOperator = OperatorTypes.crop;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    RaisePropertyChangedEvent(nameof(IsoCropOffset));
                    RaisePropertyChangedEvent(nameof(InternalCrop));
                    break;
                default:
                    _selectedOperator = OperatorTypes.undefined;
                    break;

            }
            ValidateTargetTemplateStructureId();
        }

        public InstructionViewModel()
        {
            // Design use only
            TargetTemplateStructureId = "DesignId";
            _instruction = new Copy { TemplateStructureId = TargetTemplateStructureId };
        }

        public void RegisterEvents()
        {
            _ea.GetEvent<NewTemplateStructureEvent>().Subscribe(UpdateTargetStructureList);
            _ea.GetEvent<TemplateStructureIdChanged>().Subscribe(UpdateTargetStructureList);
            _ea.GetEvent<RemovedTemplateStructureEvent>().Subscribe(RemoveTargetStructureFromList);
            ErrorsChanged += (sender, args) => { _ea.GetEvent<DataValidationRequiredEvent>().Publish(); };
        }

        private void RemoveTargetStructureFromList(RemovedTemplateStructureEventInfo info)
        {
            RaisePropertyChangedEvent(nameof(TargetTemplateIds));
        }

        private void UpdateTargetStructureList(NewTemplateStructureEventInfo info)
        {
            RaisePropertyChangedEvent(nameof(TargetTemplateIds));
        }
        private void UpdateTargetStructureList(string newStructureId)
        {
            RaisePropertyChangedEvent(nameof(TargetTemplateIds));
        }

        internal void RemoveInstruction()
        {
            _model.RemoveInstruction(_parentGeneratedStructure.StructureId, _instruction);
        }
    }

}
