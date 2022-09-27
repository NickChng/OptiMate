using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using PropertyChanged;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

[assembly: ESAPIScript(IsWriteable = true)]

namespace Optimate
{
    public class ProtocolPointer
    {
        public string ProtocolDisplayName { get; set; }
        public string ProtocolPath { get; set; }
    }
    public class SometimesObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotfication = false;
        public bool SuppressNotification
        {
            get { return _suppressNotfication; }
            set
            {
                _suppressNotfication = value;
                if (value == false)
                    base.OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
            }
        }
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotfication)
                base.OnCollectionChanged(e);
        }
    }


    [AddINotifyPropertyChangedInterface]

    public class ViewModel : ObservableObject
    {
        public bool IsErrorAcknowledged { get; set; } = true;
        public string CurrentStructureSet { get; set; }

        public bool CanLoadTemplates { get; set; } = true;
        public bool UseTest { get; set; } = true;


        private List<string> _warnings = new List<string>();

        private SolidColorBrush WarningColour = new SolidColorBrush(Colors.Goldenrod);
        public SometimesObservableCollection<OptiMateProtocolOptiStructure> ProtocolStructures { get; set; } = new SometimesObservableCollection<OptiMateProtocolOptiStructure>();

        public SometimesObservableCollection<OperatorType> Operators { get; set; } = new SometimesObservableCollection<OperatorType>() { OperatorType.copy, OperatorType.margin, OperatorType.or, OperatorType.and, OperatorType.crop, OperatorType.sub, OperatorType.subfrom };

        public ObservableCollection<string> EclipseIds { get; set; } = new ObservableCollection<string>();
        public SometimesObservableCollection<ProtocolPointer> Protocols { get; set; } = new SometimesObservableCollection<ProtocolPointer>();

        public ProtocolPointer _selectedProtocol;
        public ProtocolPointer SelectedProtocol
        {
            get { return _selectedProtocol; }
            set
            {
                try
                {
                    _selectedProtocol = value;
                    ProtocolStructures.SuppressNotification = true;
                    Operators.SuppressNotification = true;
                    InitializeProtocol(value);
                    ProtocolStructures.SuppressNotification = false;
                    Operators.SuppressNotification = false;
                    RaisePropertyChangedEvent(nameof(ProtocolVisibility));
                    RaisePropertyChangedEvent(nameof(ProtocolStructures));
                    RaisePropertyChangedEvent(nameof(allInputsValid));
                    RaisePropertyChangedEvent(nameof(ActiveProtocol));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
        }

        public OptiMateProtocol ActiveProtocol { get; set; }


        public string StartButtonTooltip
        {
            get
            {
                if (IsErrorAcknowledged)
                    if (allInputsValid)
                        return @"This generates all opti structures";
                    else
                        return @"Please review input parameters before continuing";
                else
                    return @"Acknowledge error and continue";
            }
        }

        public SolidColorBrush StartButtonColor
        {
            get
            {
                if (allInputsValid)
                    return new SolidColorBrush(Colors.PaleGreen);
                else
                    return new SolidColorBrush(Colors.Orange);
            }
        }
        private void ValidateControls(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChangedEvent(nameof(allInputsValid));
            RaisePropertyChangedEvent(nameof(StartButtonColor));
            RaisePropertyChangedEvent(nameof(StartButtonTooltip));
        }
        public Visibility ProtocolVisibility
        {
            get
            {
                if (ActiveProtocol != null)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }
        private bool _working;
        public bool Working
        {
            get
            {
                return _working;
            }
            set
            {
                _working = value;
                RaisePropertyChangedEvent(nameof(allInputsValid));
                RaisePropertyChangedEvent(nameof(StartButtonText));
            }
        }

        public string StartButtonText
        {
            get
            {
                if (_working)
                    return "Please wait...";
                else
                    return "Generate structures";
            }
        }

        public bool ScriptDone { get; set; } = false;
        public string StatusMessage { get; set; } = @"Done";

        public string ProtocolPath { get; set; }
        public string WaitMessage { get; set; }

        public bool allInputsValid
        {
            get
            {
                if (Working)
                    return false;
                if (ActiveProtocol != null)
                {
                    foreach (var PS in ProtocolStructures)
                    {
                        if (PS.HasErrors)
                            return false;
                        else
                        {
                            foreach (var I in PS.Instruction)
                            {
                                if (I.HasErrors)
                                    return false;
                                if (!I.isTargetParameterValid)
                                    return false;
                            }
                        }

                    }
                }
                else
                    return false;

                return true;
            }
        }

        string TempStructureName = @"TEMP_OptiMate";

        public SolidColorBrush ScriptCompletionStatusColour { get; set; } = new SolidColorBrush(Colors.PaleGreen);

        public ReviewWarningsViewModel ReviewWarningsVM { get; set; } = new ReviewWarningsViewModel();

        public bool ReviewWarningsPopupVisibility { get; set; } = false;
        public ViewModel()
        {
            // These values only instantiated for XAML Design
            var DummyInstructions = new List<OptiMateProtocolOptiStructureInstruction>()
            {
                new OptiMateProtocolOptiStructureInstruction() { Operator = OperatorType.copy, DefaultTarget = "Design", Target="Default" },
                new OptiMateProtocolOptiStructureInstruction() { Operator = OperatorType.and, DefaultTarget = "Design", Target="Default" },
            };
            var DummyStructure = new OptiMateProtocolOptiStructure() { StructureId = "StructureId", Instruction=DummyInstructions.ToArray(), BaseStructure = "BaseStructure", Type = @"CONTROL" };
            ActiveProtocol = new OptiMateProtocol() { OptiStructures = new OptiMateProtocolOptiStructure[] {DummyStructure, DummyStructure}, ProtocolDisplayName="Design", version=1};
            ProtocolStructures = new SometimesObservableCollection<OptiMateProtocolOptiStructure>() { DummyStructure, DummyStructure };
            ReviewWarningsVM = new ReviewWarningsViewModel();
        }
        public EsapiWorker ew = null;

        public ViewModel(EsapiWorker _ew = null)
        {
            ew = _ew;
            Initialize();
        }

        private void InitializeProtocol(ProtocolPointer value)
        {

            var newStructures = new SometimesObservableCollection<OptiMateProtocolOptiStructure>();
            XmlSerializer Ser = new XmlSerializer(typeof(OptiMateProtocol));
            if (value != null)
            {
                try
                {
                    using (StreamReader protocol = new StreamReader(value.ProtocolPath))
                    {
                        ActiveProtocol = (OptiMateProtocol)Ser.Deserialize(protocol);
                    }
                }
                catch (Exception ex)
                {
                    Helpers.SeriLog.AddError(string.Format("Unable to read protocol file: {0}", value.ProtocolPath, ex));
                    MessageBox.Show(string.Format("Unable to read/interpret protocol file {0}, see log for details.", value.ProtocolPath));
                }
                if (ActiveProtocol != null)
                {
                    // Unsubscribe
                    foreach (var OS in ProtocolStructures)
                    {
                        OS.PropertyChanged -= ValidateControls;
                        OS.StopDataValidationNotifications();
                        foreach (var I in OS.Instruction)
                        {
                            I.PropertyChanged -= ValidateControls;
                            I.StopDataValidationNotifications();
                        }
                    }
                }
                Helpers.SeriLog.AddLog(string.Format(@"Protocol [{0}] selected", ActiveProtocol.ProtocolDisplayName));
                if (ActiveProtocol != null)
                {
                    Helpers.SeriLog.AddLog("Structures cleared");
                    foreach (var ProtocolStructure in ActiveProtocol.OptiStructures)
                    {
                        ProtocolStructure.PropertyChanged += ValidateControls;
                        ProtocolStructure.StartDataValidationNotifications();
                        newStructures.Add(ProtocolStructure);
                        List<OptiMateProtocolOptiStructureInstruction> UpdatedInstructions = new List<OptiMateProtocolOptiStructureInstruction>();
                        if (ProtocolStructure.Instruction != null)
                            UpdatedInstructions = ProtocolStructure.Instruction.ToList();
                        UpdatedInstructions.Insert(0, new OptiMateProtocolOptiStructureInstruction() { Operator = OperatorType.copy, DefaultTarget = ProtocolStructure.BaseStructure });
                        ProtocolStructure.Instruction = UpdatedInstructions.ToArray(); // Add copy instruction 
                        foreach (var I in ProtocolStructure.Instruction)
                        {
                            I.PropertyChanged += ValidateControls;
                            I.StartDataValidationNotifications();
                            List<string> AvailableIds = new List<string>(EclipseIds);
                            int Index = ActiveProtocol.OptiStructures.Select(x => x.StructureId).ToList().IndexOf(ProtocolStructure.StructureId);
                            if (Index >= 0)
                            {
                                foreach (var optiId in ActiveProtocol.OptiStructures.Select(x => x.StructureId).ToList().Take(Index))
                                    AvailableIds.Add(optiId);
                            }
                            if (I.DefaultTarget != null)
                            {
                                string target = AvailableIds.FirstOrDefault(x => x.ToUpper() == I.DefaultTarget.ToUpper());
                                I.Target = target;
                            }
                        }
                    }

                }
                else
                    Helpers.SeriLog.AddLog("Null protocol selected");
            }
            else
                ActiveProtocol = null;

            ProtocolStructures = newStructures;
            Working = false;
        }
        private async void Initialize()
        {
            try
            {
                var AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ProtocolPath = Path.Combine(AssemblyPath, @"Protocols");
                ReloadTemplates();

                string currentPlanId = "";
                List<string> structures = new List<string>();
                bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, ss, ui) =>
                {
                    p.BeginModifications();
                    currentPlanId = ss.Id;
                    structures = ss.Structures.Select(x => x.Id).ToList();
                }
                ));
                if (Done)
                {
                    CurrentStructureSet = currentPlanId;
                    foreach (string Id in structures)
                        EclipseIds.Add(Id);
                    Working = false;
                }
            }
            catch (Exception ex)
            {
                Helpers.SeriLog.AddLog(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
            }
        }

        private void ViewModel_ErrorAcknowledged(object sender, EventArgs e)
        {
            IsErrorAcknowledged = true;
        }

        public ICommand StartCommand
        {
            get
            {
                return new DelegateCommand(Start);
            }
        }

        public ICommand ToggleWarningVisibilityCommand
        {
            get
            {
                return new DelegateCommand(ToggleWarningVisibility);
            }
        }

        public void ToggleWarningVisibility(object param = null)
        {
            ReviewWarningsPopupVisibility ^= true;
        }
        public ICommand OpenTemplateFolderCommand
        {
            get
            {
                return new DelegateCommand(OpenTemplateFolder);
            }
        }

        public ICommand ReloadTemplateCommand
        {
            get
            {
                return new DelegateCommand(ReloadTemplates);
            }
        }

        public ICommand RemoveInstructionCommand
        {
            get
            {
                return new DelegateCommand(RemoveInstruction);
            }
        }

        public ICommand AddStructureCommand
        {
            get
            {
                return new DelegateCommand(AddStructure);
            }
        }

        public ICommand AddInstructionCommand
        {
            get
            {
                return new DelegateCommand(AddInstruction);
            }
        }

        public ICommand ContinueCommand
        {
            get
            {
                return new DelegateCommand(Continue);
            }
        }

        public ICommand RemoveAutoStructureCommand
        {
            get
            {
                return new DelegateCommand(RemoveAutoStructure);
            }
        }

        public void Continue(object param = null)
        {
            IsErrorAcknowledged = true;
        }

        public void AddInstruction(object param = null)
        {
            object[] InstructionObject = param as object[];
            if (InstructionObject != null)
            {
                OptiMateProtocolOptiStructureInstruction I = InstructionObject[0] as OptiMateProtocolOptiStructureInstruction;
                OptiMateProtocolOptiStructure OS = InstructionObject[1] as OptiMateProtocolOptiStructure;
                if (OS != null && I != null)
                {
                    var newInstructions = OS.Instruction.ToList();
                    var newI = new OptiMateProtocolOptiStructureInstruction() { Operator = OperatorType.crop };
                    newI.PropertyChanged += ValidateControls;
                    newInstructions.Insert(newInstructions.IndexOf(I) + 1, newI);
                    OS.Instruction = newInstructions.ToArray();
                    newI.StartDataValidationNotifications();

                }
            }
            
        }
        public void AddStructure(object param = null)
        {
            var newInstruction = new OptiMateProtocolOptiStructureInstruction() { Operator = OperatorType.copy };
            newInstruction.PropertyChanged += ValidateControls;
            List<OptiMateProtocolOptiStructureInstruction> Instructions = new List<OptiMateProtocolOptiStructureInstruction>() { newInstruction };
            var OS = new OptiMateProtocolOptiStructure() { isNew = true, Instruction = Instructions.ToArray(), Type = DICOMTypes.CONTROL.ToString() };
            OS.PropertyChanged += ValidateControls;
            ProtocolStructures.Add(OS);
            newInstruction.StartDataValidationNotifications();
            OS.StartDataValidationNotifications();
        }

        public void RemoveInstruction(object param = null)
        {
            object[] InstructionObject = param as object[];
            if (InstructionObject != null)
            {
                var I = InstructionObject[0] as OptiMateProtocolOptiStructureInstruction;
                var ProtocolStructure = InstructionObject[1] as OptiMateProtocolOptiStructure;
                List<OptiMateProtocolOptiStructureInstruction> UpdatedInstructions = new List<OptiMateProtocolOptiStructureInstruction>();
                if (I != null && ProtocolStructure != null)
                {
                    I.PropertyChanged -= ValidateControls;
                    I.StopDataValidationNotifications();
                    ProtocolStructure.SuppressNotification = true;
                    foreach (var i in ProtocolStructure.Instruction)
                        if (i != I)
                            UpdatedInstructions.Add(i);
                    if (UpdatedInstructions.Count() > 0)
                        ProtocolStructure.Instruction = UpdatedInstructions.ToArray();
                    ProtocolStructure.SuppressNotification = false;
                }
            }
            
        }

        public void RemoveAutoStructure(object param = null)
        {
            OptiMateProtocolOptiStructure ProtocolStructure = param as OptiMateProtocolOptiStructure;
            if (ProtocolStructure != null)
            {
                ProtocolStructure.PropertyChanged -= ValidateControls;
                if (ProtocolStructures.Contains(ProtocolStructure))
                    ProtocolStructures.Remove(ProtocolStructure);
                //ValidateControls(null, new PropertyChangedEventArgs(nameof(AddInstruction)));
            }
        }

        public async void Start(object param = null)
        {
            if (Working)
            {
                return;
            }
            CanLoadTemplates = false;
            Working = true;
            ScriptDone = false;
            _warnings.Clear();
            bool Errors = false;
            var newstructures = new List<string>();
            bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
            {
                int numStructures = ProtocolStructures.Count();
                int numCompleted = 0;
                foreach (var ProtocolStructure in ProtocolStructures)
                {
                    Structure OS = null;
                    try
                    {
                        bool abortStructure = false;
                        if (ProtocolStructure.Instruction != null)
                            foreach (var I in ProtocolStructure.Instruction)
                            {
                                if (abortStructure)
                                    break;
                                switch (I.Operator)
                                {
                                    case OperatorType.copy:
                                        string BaseStructure_Id = I.Target;
                                        if (string.IsNullOrEmpty(I.Target))
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Copy target for structure {0} is null, skipping structure...", ProtocolStructure.StructureId);
                                                _warnings.Add(warning);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            continue;
                                        }
                                        Structure BaseStructure = S.Structures.FirstOrDefault(x => x.Id.ToUpper() == I.Target.ToUpper());
                                        if (BaseStructure == null)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Attempt to create structure {1} failed as copy target {0} does not exist in structure set, skipping structure...", I.Target, ProtocolStructure.StructureId);
                                                _warnings.Add(warning);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            abortStructure = true;
                                        }
                                        else if (BaseStructure.IsEmpty)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Attempt to create structure {1} failed as copy target {0} is empty, skipping structure...", I.Target, ProtocolStructure.StructureId);
                                                _warnings.Add(warning);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            abortStructure = true;
                                        }
                                        else
                                        {
                                            OS = S.Structures.FirstOrDefault(x => x.Id.ToUpper() == ProtocolStructure.StructureId.ToUpper());
                                            if (OS != null)
                                            {
                                                if (!OS.IsEmpty)
                                                {
                                                    OS.SegmentVolume = OS.SegmentVolume.And(OS.SegmentVolume.Not()); // clear structure;
                                                }
                                                if (BaseStructure.IsHighResolution && !OS.IsHighResolution)
                                                {
                                                    OS.ConvertToHighResolution();
                                                    OS.SegmentVolume = BaseStructure.SegmentVolume;
                                                }
                                                else if (!BaseStructure.IsHighResolution && OS.IsHighResolution)
                                                {
                                                    string TempHRStructureName = @"TEMPHR_OptiMate";
                                                    var HRTemp = S.Structures.FirstOrDefault(x => x.Id == TempHRStructureName);
                                                    if (HRTemp != null)
                                                        S.RemoveStructure(HRTemp);
                                                    HRTemp = S.AddStructure(ProtocolStructure.Type, TempHRStructureName);
                                                    HRTemp.SegmentVolume = BaseStructure.SegmentVolume;
                                                    HRTemp.ConvertToHighResolution();
                                                    OS.SegmentVolume = HRTemp.SegmentVolume;
                                                    S.RemoveStructure(HRTemp);
                                                    OS.SegmentVolume = BaseStructure.SegmentVolume;
                                                }
                                                else
                                                    OS.SegmentVolume = BaseStructure.SegmentVolume;
                                                if (ProtocolStructure.isHighResolution && !OS.IsHighResolution)
                                                    OS.ConvertToHighResolution();

                                            }
                                            else
                                            {
                                                DICOMTypes DT;
                                                Enum.TryParse(ProtocolStructure.Type.ToUpper(), out DT);
                                                bool validNewStructure = S.CanAddStructure(DT.ToString(), ProtocolStructure.StructureId);
                                                OS = S.AddStructure(DT.ToString(), ProtocolStructure.StructureId);
                                                if (BaseStructure.IsHighResolution)
                                                {
                                                    OS.ConvertToHighResolution();
                                                }
                                                OS.SegmentVolume = BaseStructure.SegmentVolume;
                                                if (ProtocolStructure.isHighResolution && !OS.IsHighResolution)
                                                {
                                                    OS.ConvertToHighResolution();
                                                }
                                            }
                                        }
                                        break;
                                    case OperatorType.crop:
                                        Structure Target = GetTargetStructure(OS, ProtocolStructure, S, I.Target);
                                        if (Target == null)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Target of CROP operation [{0}] for structure {1} is null/empty, skipping instruction...", I.Target, ProtocolStructure.StructureId);
                                                _warnings.Add(warning);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        else if (Target.IsEmpty)
                                        {
                                            Errors = true;
                                            var warning = string.Format("Target of CROP operation [{0}] for structure {1} is empty, skipping instruction...", I.Target, ProtocolStructure.StructureId);
                                            _warnings.Add(warning);
                                            //PauseTillErrorAcknowledged(ui, warning);
                                        }
                                        double cropParameter = 0;
                                        if (!string.IsNullOrEmpty(I.OperatorParameter))
                                        {
                                            if (!double.TryParse(I.OperatorParameter, out cropParameter))
                                            {
                                                ui.Invoke(() =>
                                                {
                                                    Errors = true;
                                                    var warning = string.Format("Specified margin to CROP structure {0} is invalid, defaulting to no additional margin...", ProtocolStructure.StructureId);
                                                    _warnings.Add(warning);
                                                    //PauseTillErrorAcknowledged(ui, warning);
                                                });
                                            }
                                        }
                                        bool cropExternal = false;
                                        if (!string.IsNullOrEmpty(I.OperatorParameter2))
                                        {
                                            if (!bool.TryParse(I.OperatorParameter2, out cropExternal))
                                            {
                                                ui.Invoke(() =>
                                                {
                                                    Errors = true;
                                                    var warning = string.Format("Unable to read external CROP setting (true/false) for {0}, defaulting to external crop...", ProtocolStructure.StructureId);
                                                    _warnings.Add(warning);
                                                    //PauseTillErrorAcknowledged(ui, warning);
                                                });
                                            }
                                        }
                                        if (cropExternal)
                                        {
                                            OS.SegmentVolume = OS.SegmentVolume.And(Target.SegmentVolume.Margin(-cropParameter));
                                        }
                                        else
                                        {
                                            OS.SegmentVolume = OS.SegmentVolume.Sub(Target.SegmentVolume.Margin(cropParameter));
                                        }
                                        break;
                                    case OperatorType.sub:
                                        Target = GetTargetStructure(OS, ProtocolStructure, S, I.Target);
                                        if (Target == null)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Target of SUB operation could not be found during creation of {0}, skipping operation...", ProtocolStructure.StructureId);
                                                _warnings.Add(warning);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        else if (Target.IsEmpty)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Target of SUB operation [{1}] was empty during creation of {0}, skipping operation...", ProtocolStructure.StructureId, I.Target);
                                                _warnings.Add(warning);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        OS.SegmentVolume = OS.SegmentVolume.Sub(Target);
                                        break;
                                    case OperatorType.convertDose:
                                        // Not implemented
                                        break;
                                    case OperatorType.margin:
                                        double UniformMargin;
                                        double leftmargin;
                                        double antmargin;
                                        double postmargin;
                                        double infmargin;
                                        double supmargin;
                                        if (string.IsNullOrEmpty(I.OperatorParameter))
                                        {
                                            Errors = true;
                                            var warning = string.Format(@"MARGIN for {0} is invalid, aborting margin operation...", OS.Id);
                                            _warnings.Add(warning);
                                           // PauseTillErrorAcknowledged(ui, warning);
                                            break;
                                        }
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter, out UniformMargin))
                                            {
                                                Errors = true;
                                                var warning = string.Format(@"MARGIN for {0} is invalid, aborting margin operation...", OS.Id);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                                _warnings.Add(warning);
                                                break;
                                            }
                                            else if (UniformMargin < 0)
                                            {
                                                OS.SegmentVolume = OS.SegmentVolume.Margin(UniformMargin);
                                                break;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(I.OperatorParameter2))
                                            antmargin = UniformMargin;
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter2, out antmargin))
                                            {
                                                Errors = true;
                                                string warning = string.Format(@"ANTERIOR margin for {0} is invalid, using uniform margin...", OS.Id);
                                               //PauseTillErrorAcknowledged(ui, warning);
                                                _warnings.Add(warning);
                                                antmargin = UniformMargin;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(I.OperatorParameter3))
                                            infmargin = UniformMargin;
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter3, out infmargin))
                                            {
                                                Errors = true;
                                                string warning = string.Format(@"INFERIOR margin for {0} is invalid, using uniform margin...", OS.Id);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                                _warnings.Add(warning);
                                                infmargin = UniformMargin;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(I.OperatorParameter4))
                                            leftmargin = UniformMargin;
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter4, out leftmargin))
                                            {
                                                Errors = true;
                                                string warning = string.Format(@"LEFT margin for {0} is invalid, using uniform margin...", OS.Id);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                                leftmargin = UniformMargin;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(I.OperatorParameter5))
                                            postmargin = UniformMargin;
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter5, out postmargin))
                                            {
                                                Errors = true;
                                                string warning = string.Format(@"POSTERIOR margin for {0} is invalid, using uniform margin...", OS.Id);
                                               // PauseTillErrorAcknowledged(ui, warning);
                                                _warnings.Add(warning);
                                                postmargin = UniformMargin;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(I.OperatorParameter6))
                                            supmargin = UniformMargin;
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter6, out supmargin))
                                            {
                                                Errors = true;
                                                string warning = string.Format(@"INFERIOR margin for {0} is invalid, using uniform margin...", OS.Id);
                                               // PauseTillErrorAcknowledged(ui, warning);
                                                _warnings.Add(warning);
                                                supmargin = UniformMargin;
                                            }
                                        }
                                        OS.SegmentVolume = OS.SegmentVolume.AsymmetricMargin(Helpers.OrientationInvariantMargins.getAxisAlignedMargins(S.Image.ImagingOrientation, UniformMargin, antmargin, infmargin, leftmargin, postmargin, supmargin));
                                        break;
                                    case OperatorType.and:
                                        Target = GetTargetStructure(OS, ProtocolStructure, S, I.Target);
                                        if (Target == null)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Target of AND operation could not be found during creation of {0}, skipping instruction...", ProtocolStructure.StructureId);
                                                _warnings.Add(warning);
                                               // PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        else if (Target.IsEmpty)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Target of AND operation [{1}] was empty during creation of {0}, skipping instruction...", ProtocolStructure.StructureId, I.Target);
                                                _warnings.Add(warning);
                                                // PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        OS.SegmentVolume = OS.SegmentVolume.And(Target);
                                        break;
                                    case OperatorType.subfrom:
                                        Target = GetTargetStructure(OS, ProtocolStructure, S, I.Target);
                                        if (Target == null)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Target of SUB FROM operation [{1}] could not be found during creation of {0}, skiping operation", ProtocolStructure.StructureId, I.Target);
                                                _warnings.Add(warning);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        else if (Target.IsEmpty)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Target of SUB FROM operation [{1}] was empty during creation of {0}, skipping operation", ProtocolStructure.StructureId, I.Target);
                                                _warnings.Add(warning);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        OS.SegmentVolume = Target.Sub(OS.SegmentVolume);
                                        break;
                                    case OperatorType.or:
                                        Target = GetTargetStructure(OS, ProtocolStructure, S, I.Target);
                                        if (Target == null)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Target of OR operation could not be found during creation of {0}, skipping instruction", ProtocolStructure.StructureId);
                                                _warnings.Add(warning);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        else if (Target.IsEmpty)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Target of OR operation [{1}] was empty during creation of {0}, skipping instruction", ProtocolStructure.StructureId, I.Target);
                                                _warnings.Add(warning);
                                                //PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        OS.SegmentVolume = OS.SegmentVolume.Or(Target);
                                        break;
                                    default:
                                        ui.Invoke(() =>
                                        {
                                            Errors = true;
                                            var warning = string.Format("Opti structure ({0}) creation operation instruction references unrecognized operator ({1}), skipping instruction...", ProtocolStructure.StructureId, I.Operator);
                                            _warnings.Add(warning);
                                            //PauseTillErrorAcknowledged(ui, warning);
                                        });
                                        break;
                                }
                                //else
                                //{
                                //    ui.Invoke(() =>
                                //    {
                                //        Errors = true;
                                //        StatusMessage = string.Format(@"Structure {0} has an instruction with an invalid operator. Please review.", ProtocolStructure.StructureId);
                                //    });
                                //    return;
                                //}
                                var TemOStructure = S.Structures.FirstOrDefault(x => x.Id.ToUpper() == TempStructureName.ToUpper());
                                if (TemOStructure != null)
                                    S.RemoveStructure(TemOStructure);

                            }
                        if (!abortStructure)
                        {
                            Helpers.SeriLog.AddLog(string.Format("@Created opti structure: {0}", ProtocolStructure.StructureId));
                            newstructures.Add(ProtocolStructure.StructureId);
                            ui.Invoke(() => WaitMessage = string.Format("{0} created... ({1}/{2})", ProtocolStructure.StructureId, numCompleted, numStructures));
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Helpers.SeriLog.AddError(string.Format("Error creating structure {0}", ProtocolStructure.StructureId), ex);
                        MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                        ui.Invoke(() => WaitMessage = string.Format("{0} experienced error... ({1}/{2})", ProtocolStructure.StructureId, numCompleted, numStructures));
                    }
                    numCompleted++;
                }
            }
            ));
            foreach (string newS in newstructures)
            {
                EclipseIds.Add(newS);
            }
            Working = false;
            ScriptDone = true;
            if (Errors)
            {
                StatusMessage = "Script completed with warnings, please click here to review...";
                ScriptCompletionStatusColour = new SolidColorBrush(Colors.Orange);
                ReviewWarningsVM.SetWarnings(_warnings);
            }
            else
            {
                StatusMessage = "Structure creation complete";
                ScriptCompletionStatusColour = new SolidColorBrush(Colors.PaleGreen);
            }
            CanLoadTemplates = true;

        }


        private bool PauseTillErrorAcknowledged(Dispatcher ui, string warning)
        {
            Helpers.SeriLog.AddLog(warning);
            ui.Invoke(() =>
            {
                Working = false;
                ScriptDone = true;
                StatusMessage = warning;
                ScriptCompletionStatusColour = WarningColour;

            });
            IsErrorAcknowledged = false;
            while (!IsErrorAcknowledged)
                Thread.Sleep(100);
            ui.Invoke(() =>
            {
                Working = true;
                ScriptDone = false;
            });
            Helpers.SeriLog.AddLog(@"Warning acknowledged");
            return true;
        }

        private Structure GetTargetStructure(Structure OS, OptiMateProtocolOptiStructure ProtocolStructure, StructureSet S, string TargetId)
        {
            // returns either the target structure or a temporary structure with the same resolution   
            if (string.IsNullOrEmpty(TargetId))
                return null;
            var TargetStructure = S.Structures.FirstOrDefault(x => x.Id.ToUpper() == TargetId.ToUpper());
            if (TargetStructure == null)
            {
                Helpers.SeriLog.AddLog(string.Format("Opti structure ({0}) creation operation instruction references target {1} which was not found", ProtocolStructure.StructureId, TargetId));
                return null;
            }
            else
            {
                Structure Temp = null;
                if (TargetStructure.IsHighResolution != OS.IsHighResolution)
                {
                    if (TargetStructure.IsHighResolution)
                    {
                        OS.ConvertToHighResolution();
                        Temp = TargetStructure;
                    }
                    else
                    {
                        Temp = S.Structures.FirstOrDefault(x => x.Id.ToUpper() == TempStructureName.ToUpper());
                        if (Temp != null)
                            S.RemoveStructure(Temp);
                        Temp = S.AddStructure(ProtocolStructure.Type, TempStructureName);
                        Temp.SegmentVolume = TargetStructure.SegmentVolume;
                        Temp.ConvertToHighResolution();
                    }
                }
                else
                    Temp = TargetStructure;
                return Temp;
            }
        }
        public async void OpenTemplateFolder(object param = null)
        {
            await Task.Run(() => Process.Start(ProtocolPath));
        }

        public async void ReloadTemplates(object param = null)
        {
            WaitMessage = "Loading Templates...";
            Working = true;
            Protocols.Clear();
            Protocols.SuppressNotification = true;
            //await Task.Run(() =>
            //{
            try
            {
                XmlSerializer Ser = new XmlSerializer(typeof(OptiMateProtocol));

                foreach (var file in Directory.GetFiles(ProtocolPath, "*.xml"))
                {
                    using (StreamReader protocol = new StreamReader(file))
                    {
                        try
                        {
                            var OMProtocol = (OptiMateProtocol)Ser.Deserialize(protocol);
                            Protocols.Add(new ProtocolPointer() { ProtocolDisplayName = OMProtocol.ProtocolDisplayName, ProtocolPath = file });
                        }
                        catch (Exception ex)
                        {
                            Helpers.SeriLog.AddLog(string.Format("Unable to read protocol file: {0}\r\n\r\nDetails: {1}", file, ex.InnerException));
                            MessageBox.Show(string.Format("Unable to read protocol file {0}\r\n\r\nDetails: {1}", file, ex.InnerException));

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
            }
            //}          );
            Protocols.SuppressNotification = false;
            Working = false;
            WaitMessage = "";
        }

    }

    public enum DICOMTypes
    {
        CONTROL,
        AVOIDANCE,
        CAVITY,
        CONTRAST_AGENT,
        CTV,
        EXTERNAL,
        GTV,
        IRRAD_VOLUME,
        ORGAN,
        PTV,
        TREATED_VOLUME,
        SUPPORT,
        FIXATION,
        DOSE_REGION
    }

    //public enum OperatorTypes
    //{
    //    UNDEFINED,
    //    copy,
    //    margin,
    //    or,
    //    and,
    //    crop,
    //    sub,
    //    subfrom
    //}
}
