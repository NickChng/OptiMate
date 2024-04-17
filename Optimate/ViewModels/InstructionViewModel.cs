using OptiMate;
using OptiMate.ViewModels;
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
        convertResolution,
        margin,
        asymmetricMargin,
        or,
        and,
        crop,
        asymmetricCrop,
        sub,
        subfrom,
        convertDose
    }

    public class RemovingInstructionViewModelEvent : PubSubEvent<InstructionViewModel> { }
    public class InstructionViewModel : ObservableObject
    {
        private MainModel _model;
        private IEventAggregator _ea;
        private Instruction _instruction;
        private GeneratedStructure _parentGeneratedStructure;
        public SometimesObservableCollection<OperatorTypes> Operators { get; set; } = new SometimesObservableCollection<OperatorTypes>()
        {
              OperatorTypes.convertResolution,
              OperatorTypes.margin,
              OperatorTypes.asymmetricMargin,
              OperatorTypes.or,
              OperatorTypes.and,
              OperatorTypes.crop,
              OperatorTypes.asymmetricCrop,
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
 
        public ushort DoseLevel
        {
            get 
            {
                if (SelectedOperator == OperatorTypes.convertDose)
                    return (_instruction as ConvertDose).DoseLevel; 
                else
                    return 0;
            }
            set
            {
                if (_model.isDoseLevelValid(value))
                {
                    (_instruction as ConvertDose).DoseLevel = value;
                    isModified = true;
                }

            }
        }

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
                    if ((_instruction as AsymmetricMargin)?.AntMargin == null)
                    {
                        return "0";
                    }
                    else
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
                    if ((_instruction as AsymmetricMargin)?.PostMargin == null)
                    {
                        return "0";
                    }
                    else
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
                    if ((_instruction as AsymmetricMargin)?.SupMargin == null)
                    {
                        return "0";
                    }
                    else
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
                    if ((_instruction as AsymmetricMargin)?.InfMargin == null)
                    {
                        return "0";
                    }
                    else
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
                    if ((_instruction as AsymmetricMargin)?.LeftMargin == null)
                    {
                        return "0";
                    }
                    else
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
                    if ((_instruction as AsymmetricMargin)?.RightMargin == null)
                    {
                        return "0";
                    }
                    else
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
                    case OperatorTypes.asymmetricCrop:
                        return (_instruction as AsymmetricCrop).InternalCrop;
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
                    case OperatorTypes.asymmetricCrop:
                        (_instruction as AsymmetricCrop).InternalCrop = value;
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
                _ea.GetEvent<DataValidationRequiredEvent>().Publish();
            }
        }
        public string IsoCropOffset
        {
            get
            {
                if (SelectedOperator == OperatorTypes.crop)
                {
                    if (string.IsNullOrEmpty((_instruction as Crop).IsotropicOffset))
                    {
                        return "0";
                    }
                    else
                        return (_instruction as Crop).IsotropicOffset;
                }
                else return "";
            }
            set
            {
                if (SelectedOperator == OperatorTypes.crop)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as Crop).IsotropicOffset = value;
                        isModified = true;
                    }
                }
            }
        }

        public string LeftCropOffset
        {
            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (string.IsNullOrEmpty((_instruction as AsymmetricCrop).LeftOffset))
                    {
                        return "0";
                    }
                    else
                        return (_instruction as AsymmetricCrop).LeftOffset;
                }
                else return "";
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as AsymmetricCrop).LeftOffset = value;
                        isModified = true;
                    }
                }
            }
        }
        public string RightCropOffset
        {
            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (string.IsNullOrEmpty((_instruction as AsymmetricCrop).RightOffset))
                    {
                        return "0";
                    }
                    else
                        return (_instruction as AsymmetricCrop).RightOffset;
                }
                else return "";
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as AsymmetricCrop).RightOffset = value;
                        isModified = true;
                    }
                }
            }
        }

        public string AntCropOffset
        {
            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (string.IsNullOrEmpty((_instruction as AsymmetricCrop).AntOffset))
                    {
                        return "0";
                    }
                    else
                        return (_instruction as AsymmetricCrop).AntOffset;
                }
                else return "";
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as AsymmetricCrop).AntOffset = value;
                        isModified = true;
                    }
                }
            }
        }

        public string PostCropOffset
        {
            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (string.IsNullOrEmpty((_instruction as AsymmetricCrop).PostOffset))
                    {
                        return "0";
                    }
                    else
                        return (_instruction as AsymmetricCrop).PostOffset;
                }
                else return "";
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as AsymmetricCrop).PostOffset = value;
                        isModified = true;
                    }
                }
            }
        }

        public string InfCropOffset
        {
            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (string.IsNullOrEmpty((_instruction as AsymmetricCrop).InfOffset))
                    {
                        return "0";
                    }
                    else
                        return (_instruction as AsymmetricCrop).InfOffset;
                }
                else return "";
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as AsymmetricCrop).InfOffset = value;
                        isModified = true;
                    }
                }
            }
        }

        public string SupCropOffset
        {
            get
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (string.IsNullOrEmpty((_instruction as AsymmetricCrop).SupOffset))
                    {
                        return "0";
                    }
                    else
                        return (_instruction as AsymmetricCrop).SupOffset;
                }
                else return "";
            }
            set
            {
                if (SelectedOperator == OperatorTypes.asymmetricCrop)
                {
                    if (isMarginValueValid(value))
                    {
                        (_instruction as AsymmetricCrop).SupOffset = value;
                        isModified = true;
                    }
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
                    case OperatorTypes.convertResolution:
                        return Visibility.Hidden;
                    case OperatorTypes.convertDose:
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
                    case AsymmetricCrop crop:
                        return crop.TemplateStructureId;
                    case And and:
                        return and.TemplateStructureId;
                    case Or or:
                        return or.TemplateStructureId;
                    case Sub sub:
                        return sub.TemplateStructureId;
                    case SubFrom subFrom:
                        return subFrom.TemplateStructureId;
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
                    case AsymmetricCrop crop:
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
                    case SubFrom subFrom:
                        subFrom.TemplateStructureId = value;
                        break;
                    default:
                        return;
                }
                ValidateTargetTemplateStructureId();
            }
        }

        private void ValidateTargetTemplateStructureId()
        {
            ClearErrors(nameof(TargetTemplateStructureId));
            if (instructionHasTarget())
            {
                if (!string.Equals(DefaultTemplateStructureId, TargetTemplateStructureId, StringComparison.OrdinalIgnoreCase))
                    isModified = true;
                else
                    isModified = false;
                if (string.IsNullOrEmpty(TargetTemplateStructureId))
                {
                    int instructionNumber = _model.GetInstructionNumber(_parentGeneratedStructure.StructureId, _instruction);
                    AddError(nameof(TargetTemplateStructureId), $"{_parentGeneratedStructure.StructureId} operator #{instructionNumber + 1} has an invalid target of operation.");
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
                case OperatorTypes.subfrom:
                    return true;
                case OperatorTypes.convertDose:
                    return false;
                case OperatorTypes.crop:
                    return true;
                case OperatorTypes.margin:
                    return false;
                case OperatorTypes.convertResolution:
                    return false;
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

        //public Visibility TargetTemplateStructureVisibility
        //{
        //    get
        //    {
        //        switch (SelectedOperator)
        //        {
        //            case OperatorTypes.or:
        //                return Visibility.Visible;
        //            case OperatorTypes.and:
        //                return Visibility.Visible;
        //            case OperatorTypes.sub:
        //                return Visibility.Visible;
        //            case OperatorTypes.subfrom:
        //                return Visibility.Visible;
        //            case OperatorTypes.convertDose:
        //                return Visibility.Collapsed;
        //            case OperatorTypes.crop:
        //                return Visibility.Visible;
        //            case OperatorTypes.margin:
        //                return Visibility.Collapsed;
        //            case OperatorTypes.convertResolution:
        //                return Visibility.Collapsed;
        //            default:
        //                return Visibility.Collapsed;
        //        }
        //    }
        //}

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
                case Or inst:
                    TargetTemplateStructureId = inst.TemplateStructureId;
                    DefaultTemplateStructureId = TargetTemplateStructureId;
                    _selectedOperator = OperatorTypes.or;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    break;
                case And inst:
                    TargetTemplateStructureId = inst.TemplateStructureId;
                    DefaultTemplateStructureId = TargetTemplateStructureId;
                    _selectedOperator = OperatorTypes.and;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    break;
                case Sub inst:
                    TargetTemplateStructureId = inst.TemplateStructureId;
                    DefaultTemplateStructureId = TargetTemplateStructureId;
                    _selectedOperator = OperatorTypes.sub;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    break;
                case SubFrom inst:
                    TargetTemplateStructureId = inst.TemplateStructureId;
                    DefaultTemplateStructureId = TargetTemplateStructureId;
                    _selectedOperator = OperatorTypes.subfrom;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    break;
                case Margin _:
                    _selectedOperator = OperatorTypes.margin;
                    RaisePropertyChangedEvent(nameof(IsoMargin));
                    break;
                case AsymmetricMargin _:
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
                    _selectedOperator = OperatorTypes.convertDose;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    break;
                case ConvertResolution _:
                    _selectedOperator = OperatorTypes.convertResolution;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    break;
                case Crop inst:
                    TargetTemplateStructureId = inst.TemplateStructureId;
                    DefaultTemplateStructureId = TargetTemplateStructureId;
                    _selectedOperator = OperatorTypes.crop;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    RaisePropertyChangedEvent(nameof(IsoCropOffset));
                    RaisePropertyChangedEvent(nameof(InternalCrop));
                    break;
                case AsymmetricCrop inst:
                    TargetTemplateStructureId = inst.TemplateStructureId;
                    DefaultTemplateStructureId = TargetTemplateStructureId;
                    _selectedOperator = OperatorTypes.asymmetricCrop;
                    RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
                    RaisePropertyChangedEvent(nameof(RightCropOffset));
                    RaisePropertyChangedEvent(nameof(LeftCropOffset));
                    RaisePropertyChangedEvent(nameof(PostCropOffset));
                    RaisePropertyChangedEvent(nameof(AntCropOffset));
                    RaisePropertyChangedEvent(nameof(SupCropOffset));
                    RaisePropertyChangedEvent(nameof(InfCropOffset));
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
            _instruction = new Or { TemplateStructureId = TargetTemplateStructureId };
        }

        public void RegisterEvents()
        {
            _ea.GetEvent<NewTemplateStructureEvent>().Subscribe(OnNewTemplateStructureEvent);
            _ea.GetEvent<TemplateStructureIdChangedEvent>().Subscribe(OnTemplateStructureIdChanged);
            _ea.GetEvent<RemovedTemplateStructureEvent>().Subscribe(RemoveTemplateStructureFromList);
            _ea.GetEvent<RemovedGeneratedStructureEvent>().Subscribe(RemoveGeneratedStructureFromList);
            _ea.GetEvent<RemovedInstructionEvent>().Subscribe(RemoveInstructionViewModel);
            _ea.GetEvent<GeneratedStructureIdChangedEvent>().Subscribe(OnGeneratedStructureIdChanged);
            _ea.GetEvent<GeneratedStructureOrderChangedEvent>().Subscribe(OnGeneratedStructureOrderChanged);
            ErrorsChanged += (sender, args) => { _ea.GetEvent<DataValidationRequiredEvent>().Publish(); };
        }

        private void RemoveGeneratedStructureFromList(RemovedGeneratedStructureEventInfo info)
        {
            RaisePropertyChangedEvent(nameof(TargetTemplateIds));
        }

        private void OnGeneratedStructureOrderChanged()
        {
            RaisePropertyChangedEvent(nameof(TargetTemplateIds));
        }

        private void OnGeneratedStructureIdChanged(GeneratedStructureIdChangedEventInfo info)
        {
            if (TargetTemplateStructureId == info.OldId)
            {
                DefaultTemplateStructureId = info.NewId;
                TargetTemplateStructureId = info.NewId;
                RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
            }
            RaisePropertyChangedEvent(nameof(TargetTemplateIds));
        }

        private void OnNewTemplateStructureEvent(NewTemplateStructureEventInfo info)
        {
            RaisePropertyChangedEvent(nameof(TargetTemplateIds));
        }

        private void RemoveInstructionViewModel(InstructionRemovedEventInfo info)
        {
            if (_instruction == info.RemovedInstruction)
                _ea.GetEvent<RemovingInstructionViewModelEvent>().Publish(this);
            
        }

        private void RemoveTemplateStructureFromList(RemovedTemplateStructureEventInfo info)
        {
            RaisePropertyChangedEvent(nameof(TargetTemplateIds));
        }

        private void OnTemplateStructureIdChanged(TemplateStructureIdChangedEventInfo info)
        {
            RaisePropertyChangedEvent(nameof(TargetTemplateIds));
            if (info.OldId == TargetTemplateStructureId)
            {
                DefaultTemplateStructureId = info.NewId;
                TargetTemplateStructureId = info.NewId;
                RaisePropertyChangedEvent(nameof(TargetTemplateStructureId));
            }
        }
       
        internal void RemoveInstruction()
        {
            _model.RemoveInstruction(_parentGeneratedStructure.StructureId, _instruction);
        }
    }

}
