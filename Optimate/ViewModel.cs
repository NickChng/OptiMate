using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

        private SolidColorBrush ErrorColour = new SolidColorBrush(Colors.Orange);

        private SolidColorBrush WarningColour = new SolidColorBrush(Colors.Goldenrod);
        public SometimesObservableCollection<OptiMateProtocolOptiStructure> ProtocolStructures { get; set; } = new SometimesObservableCollection<OptiMateProtocolOptiStructure>();

        public SometimesObservableCollection<string> Operators { get; set; } = new SometimesObservableCollection<string>() { "copy", "crop", "margin", "and", "or", "subfrom" };
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
        private void ValidateControl(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
        public bool Working { get; set; } = false;
        public bool ScriptDone { get; set; } = false;
        public string StatusMessage { get; set; } = @"Done";

        public string ProtocolPath { get; set; }
        public string WaitMessage { get; set; }

        public bool allInputsValid
        {
            get
            {
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

        public ViewModel()
        {

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
                    Helpers.Logger.AddLog(string.Format("Unable to read protocol file: {0}\r\n\r\nDetails: {1}", value.ProtocolPath, ex.InnerException));
                    MessageBox.Show(string.Format("Unable to read protocol file {0}\r\n\r\nDetails: {1}", value.ProtocolPath, ex.InnerException));

                }
                if (ActiveProtocol != null)
                {
                    // Unsubscribe
                    foreach (var OS in ProtocolStructures)
                    {
                        OS.PropertyChanged -= ValidateControl;
                        foreach (var I in OS.Instruction)
                        {
                            I.PropertyChanged -= ValidateControl;
                        }
                    }
                }
                Helpers.Logger.AddLog("Protocol changed");
                if (ActiveProtocol != null)
                {
                    Helpers.Logger.AddLog("Structures cleared");
                    foreach (var ProtocolStructure in ActiveProtocol.OptiStructures)
                    {
                        ProtocolStructure.PropertyChanged += ValidateControl;
                        newStructures.Add(ProtocolStructure);
                        List<OptiMateProtocolOptiStructureInstruction> UpdatedInstructions = new List<OptiMateProtocolOptiStructureInstruction>();
                        if (ProtocolStructure.Instruction != null)
                            UpdatedInstructions = ProtocolStructure.Instruction.ToList();
                        UpdatedInstructions.Insert(0, new OptiMateProtocolOptiStructureInstruction() { Operator = OperatorType.copy, DefaultTarget = ProtocolStructure.BaseStructure });
                        ProtocolStructure.Instruction = UpdatedInstructions.ToArray(); // Add copy instruction 
                        foreach (var I in ProtocolStructure.Instruction)
                        {
                            I.PropertyChanged += ValidateControl;
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
                    Helpers.Logger.AddLog("Null protocol selected");
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
                Helpers.Logger.AddLog(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
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
                    newI.PropertyChanged += ValidateControl;
                    newInstructions.Insert(newInstructions.IndexOf(I) + 1, newI);
                    OS.Instruction = newInstructions.ToArray();
                }
            }
            ValidateControl(null, new PropertyChangedEventArgs(nameof(AddInstruction)));
        }
        public void AddStructure(object param = null)
        {
            var newInstruction = new OptiMateProtocolOptiStructureInstruction() { Operator = OperatorType.copy };
            newInstruction.PropertyChanged += ValidateControl;
            List<OptiMateProtocolOptiStructureInstruction> Instructions = new List<OptiMateProtocolOptiStructureInstruction>() { newInstruction };
            var OS = new OptiMateProtocolOptiStructure() { isNew = true, Instruction = Instructions.ToArray() };
            OS.PropertyChanged += ValidateControl;
            ProtocolStructures.Add(OS);
            ValidateControl(null, new PropertyChangedEventArgs(nameof(AddInstruction)));
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
                    I.PropertyChanged -= ValidateControl;
                    ProtocolStructure.SuppressNotification = true;
                    foreach (var i in ProtocolStructure.Instruction)
                        if (i != I)
                            UpdatedInstructions.Add(i);
                    if (UpdatedInstructions.Count() > 0)
                        ProtocolStructure.Instruction = UpdatedInstructions.ToArray();
                    ProtocolStructure.SuppressNotification = false;


                }
            }
            //ValidateControl(null, new PropertyChangedEventArgs(nameof(RemoveInstruction)));
        }

        public void RemoveAutoStructure(object param = null)
        {
            OptiMateProtocolOptiStructure ProtocolStructure = param as OptiMateProtocolOptiStructure;
            if (ProtocolStructure != null)
            {
                ProtocolStructure.PropertyChanged -= ValidateControl;
                if (ProtocolStructures.Contains(ProtocolStructure))
                    ProtocolStructures.Remove(ProtocolStructure);
                ValidateControl(null, new PropertyChangedEventArgs(nameof(AddInstruction)));
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
                        if (ProtocolStructure.Instruction != null)
                            foreach (var I in ProtocolStructure.Instruction)
                            {
                                OperatorType Op = OperatorType.UNDEFINED;
                                switch (Op)
                                {
                                    case OperatorType.copy:
                                        string BaseStructure_Id = I.Target;
                                        if (string.IsNullOrEmpty(I.Target))
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Copy structure for {0} is not defined, skipping structure...", ProtocolStructure.StructureId);
                                                PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            continue;
                                        }
                                        Structure BaseStructure = S.Structures.FirstOrDefault(x => x.Id.ToUpper() == I.Target.ToUpper());
                                        if (BaseStructure == null)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Copy structure for {0} does not exist, skipping structure...", ProtocolStructure.StructureId);
                                                PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            continue;
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
                                                var warning = string.Format("Crop target for {0} is invalid, crop aborted...", ProtocolStructure.StructureId);
                                                PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        double cropParameter = 0;
                                        if (!string.IsNullOrEmpty(I.OperatorParameter))
                                        {
                                            if (!double.TryParse(I.OperatorParameter, out cropParameter))
                                            {
                                                ui.Invoke(() =>
                                                {
                                                    Errors = true;
                                                    var warning = string.Format("Crop margin for {0} is invalid, defaulting to no additional margin...", ProtocolStructure.StructureId);
                                                    PauseTillErrorAcknowledged(ui, warning);
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
                                                    var warning = string.Format("Unable to read external crop setting (true/false) for {0}, defaulting to external crop...", ProtocolStructure.StructureId);
                                                    PauseTillErrorAcknowledged(ui, warning);
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
                                                var warning = string.Format("Error during creation of {0} : Target of SUB operation could not be found", ProtocolStructure.StructureId);
                                                PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        OS.SegmentVolume = OS.SegmentVolume.Sub(Target);
                                        break;
                                    case OperatorType.margin:
                                        double UniformMargin;
                                        double X2margin;
                                        double Y1margin;
                                        double Y2margin;
                                        double Z1margin;
                                        double Z2margin;
                                        if (string.IsNullOrEmpty(I.OperatorParameter))
                                        {
                                            Errors = true;
                                            var warning = string.Format(@"Margin for {0} is invalid, aborting margin operation...", OS.Id);
                                            PauseTillErrorAcknowledged(ui, warning);
                                            break;
                                        }
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter, out UniformMargin))
                                            {
                                                Errors = true;
                                                var warning = string.Format(@"Margin for {0} is invalid, aborting margin operation...", OS.Id);
                                                PauseTillErrorAcknowledged(ui, warning);
                                                break;
                                            }
                                            else if (UniformMargin < 0)
                                            {
                                                OS.SegmentVolume = OS.SegmentVolume.Margin(UniformMargin);
                                                break;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(I.OperatorParameter2))
                                            Y1margin = UniformMargin;
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter2, out Y1margin))
                                            {
                                                Errors = true;
                                                PauseTillErrorAcknowledged(ui, string.Format(@"ANTERIOR margin for {0} is invalid, using uniform margin...", OS.Id));
                                                Y1margin = UniformMargin;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(I.OperatorParameter3))
                                            Z1margin = UniformMargin;
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter3, out Z1margin))
                                            {
                                                Errors = true;
                                                PauseTillErrorAcknowledged(ui, string.Format(@"INFERIOR margin for {0} is invalid, using uniform margin...", OS.Id));
                                                Z1margin = UniformMargin;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(I.OperatorParameter4))
                                            X2margin = UniformMargin;
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter4, out X2margin))
                                            {
                                                Errors = true;
                                                PauseTillErrorAcknowledged(ui, string.Format(@"LEFT margin for {0} is invalid, using uniform margin...", OS.Id));
                                                X2margin = UniformMargin;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(I.OperatorParameter5))
                                            Y2margin = UniformMargin;
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter5, out Y2margin))
                                            {
                                                Errors = true;
                                                PauseTillErrorAcknowledged(ui, string.Format(@"POSTERIOR margin for {0} is invalid, using uniform margin...", OS.Id));
                                                Y2margin = UniformMargin;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(I.OperatorParameter6))
                                            Z2margin = UniformMargin;
                                        else
                                        {
                                            if (!double.TryParse(I.OperatorParameter6, out Z2margin))
                                            {
                                                Errors = true;
                                                PauseTillErrorAcknowledged(ui, string.Format(@"POSTERIOR margin for {0} is invalid, using uniform margin...", OS.Id));
                                                Z2margin = UniformMargin;
                                            }
                                        }
                                        OS.SegmentVolume = OS.SegmentVolume.AsymmetricMargin(new AxisAlignedMargins(StructureMarginGeometry.Outer, UniformMargin, Y1margin, Z1margin, X2margin, Y2margin, Z2margin));
                                        break;
                                    case OperatorType.and:
                                        Target = GetTargetStructure(OS, ProtocolStructure, S, I.Target);
                                        if (Target == null)
                                        {
                                            ui.Invoke(() =>
                                            {
                                                Errors = true;
                                                var warning = string.Format("Error during creation of {0} : Target of AND operation could not be found", ProtocolStructure.StructureId);
                                                PauseTillErrorAcknowledged(ui, warning);
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
                                                var warning = string.Format("Error during creation of {0} : Target of AND operation could not be found", ProtocolStructure.StructureId);
                                                PauseTillErrorAcknowledged(ui, warning);
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
                                                var warning = string.Format("Error during creation of {0} : Target of OR operation could not be found", ProtocolStructure.StructureId);
                                                PauseTillErrorAcknowledged(ui, warning);
                                            });
                                            break;
                                        }
                                        OS.SegmentVolume = OS.SegmentVolume.Or(Target);
                                        break;
                                    default:
                                        ui.Invoke(() =>
                                        {
                                            Errors = true;
                                            var warning = string.Format("Opti structure ({0}) creation operation instruction references unrecognized operator ({1})", ProtocolStructure.StructureId, I.Operator);
                                            PauseTillErrorAcknowledged(ui, warning);
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
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                    }
                    Helpers.Logger.AddLog(string.Format("{0}, {1}, {2}{3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), @"Created opti structure: ", ProtocolStructure.StructureId));
                    newstructures.Add(ProtocolStructure.StructureId);
                    numCompleted++;
                    ui.Invoke(() => WaitMessage = string.Format("{0} created... ({1}/{2})", ProtocolStructure.StructureId, numCompleted, numStructures));
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
                StatusMessage = "Script completed with errors, please review...";
                ScriptCompletionStatusColour = new SolidColorBrush(Colors.Orange);
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
            Helpers.Logger.AddLog(warning);
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
            Helpers.Logger.AddLog(@"Warning acknowledged");
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
                Helpers.Logger.AddLog(string.Format("Opti structure ({0}) creation operation instruction references target {1} which was not found", ProtocolStructure.StructureId, TargetId));
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
                            Helpers.Logger.AddLog(string.Format("Unable to read protocol file: {0}\r\n\r\nDetails: {1}", file, ex.InnerException));
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
