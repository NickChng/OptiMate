using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
using OptiMate.Models;
using OptiMate.ViewModels;
using OptiMate.Logging;
using PropertyChanged;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Prism.Events;


[assembly: ESAPIScript(IsWriteable = true)]

namespace OptiMate.ViewModels
{

    public class TemplatePointer
    {
        public string TemplateDisplayName { get; set; }
        public string TemplatePath { get; set; }
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

    public class DataValidationRequiredEvent : PubSubEvent { }

    [AddINotifyPropertyChangedInterface]

    public class ViewModel : ObservableObject
    {
        private MainModel _model;
        private IEventAggregator _ea = new EventAggregator();
        private Dispatcher _ui;
        public bool IsErrorAcknowledged { get; set; } = true;
        public string CurrentStructureSet { get; set; }

        public bool CanLoadTemplates { get; set; } = true;
        public bool PopupLock { get; private set; } = true;

        private List<string> _warnings = new List<string>();

        private SolidColorBrush WarningColour = new SolidColorBrush(Colors.DarkOrange);
        public ObservableCollection<string> EclipseIds { get; set; } = new ObservableCollection<string>();
        public SometimesObservableCollection<TemplatePointer> TemplatePointers { get; set; } = new SometimesObservableCollection<TemplatePointer>();

        public TemplatePointer _selectedTemplate;
        public TemplatePointer SelectedTemplate
        {
            get { return _selectedTemplate; }
            set
            {
                try
                {
                    if (value != null)
                    {
                        _selectedTemplate = value;
                        InitializeProtocol(value);
                        RaisePropertyChangedEvent(nameof(ProtocolVisibility));
                        RaisePropertyChangedEvent(nameof(ActiveTemplate));
                    }
                    else
                        StatusMessage = "Please select template...";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
        }

        public TemplateViewModel ActiveTemplate { get; set; }

        public SaveNewTemplateViewModel SaveTemplateVM { get; set; }

        



        private void ValidateControls(object sender, DataErrorsChangedEventArgs e)
        {
            CheckAllInputsValid();
        }
        public Visibility ProtocolVisibility
        {
            get
            {
                if (ActiveTemplate != null)
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
                RaisePropertyChangedEvent(nameof(StartButtonText));
            }
        }

        public bool HasWarnings
        {
            get
            {
                return warnings.Count > 0;
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

        public bool StatusMessageVisibility
        {
            get
            {
                return (!string.IsNullOrEmpty(StatusMessage) && Working == false);
            }
        }
        private string _statusMessage = @"Please select a template...";
        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                _statusMessage = value;
                RaisePropertyChangedEvent(nameof(StatusMessageVisibility));
            }
        }
        public string WaitMessage { get; set; }
        private List<string> warnings = new List<string>();
        public bool AllInputsValid { get; private set; }
        private void CheckAllInputsValid()
        {
            //ClearErrors(nameof(allInputsValid));
            warnings = new List<string>();
            if (ActiveTemplate != null)
            {
                if (ActiveTemplate.ValidateInputs(warnings))
                {
                    StatusMessage = "Ready...";
                    AllInputsValid= true;
                }
                else
                {
                    StatusMessage = "Please review input parameters before continuing, click for details...";
                    AllInputsValid = false;
                }
            }
            else
            {
                string error = "Unable to load template, please click for details...";
                warnings.Add(error);
                AllInputsValid = false;
                //AddError(nameof(allInputsValid), error);
            }
            RaisePropertyChangedEvent(nameof(HasWarnings));
            RaisePropertyChangedEvent(nameof(StatusBorderColor));
            RaisePropertyChangedEvent(nameof(Working));

        }
        public SolidColorBrush StatusBorderColor
        {
            get
            {
                return HasWarnings ? WarningColour : new SolidColorBrush(Colors.Transparent);
            }
        }

        public DescriptionViewModel ReviewWarningsVM { get; set; } = new DescriptionViewModel("Review generated structure warnings", "");

        public bool ReviewWarningsPopupVisibility { get; set; } = false;

        public bool ReviewInputValidationPopupVisibility { get; set; } = false;

        public ViewModel()
        {
            // These values only instantiated for XAML Design
            ActiveTemplate = new TemplateViewModel() { TemplateDisplayName = "Design" };
        }

        public ViewModel(MainModel model)
        {
            StartWait("Loading structures...");
            _ui = Dispatcher.CurrentDispatcher;
            _model = model;
            _model.SetEventAggregator(_ea);
            RegisterEvents();
            _model.Initialize();
        }

        private void RegisterEvents()
        {
            _ea.GetEvent<StructureGeneratingEvent>().Subscribe(UpdateStatus_GeneratingStructure);
            _ea.GetEvent<DataValidationRequiredEvent>().Subscribe(CheckAllInputsValid);
            _ea.GetEvent<ModelInitializedEvent>().Subscribe(Initialize);
            _ea.GetEvent<TemplateSavedEvent>().Subscribe(OnTemplateSaved);
            _ea.GetEvent<LockingPopupEvent>().Subscribe(LockForPopup);
        }

        private void LockForPopup(bool isLockingPopupActive)
        {
            PopupLock = !isLockingPopupActive;
        }

        private void OnTemplateSaved()
        {
            ReloadTemplates();
        }

        private void UpdateStatus_GeneratingStructure(StructureGeneratingEventInfo info)
        {
            WaitMessage = $"Creating {info.Structure.StructureId}... ({info.IndexInQueue+1}/{info.TotalToGenerate})";
            SeriLogUI.AddLog(WaitMessage);
        }

        private void InitializeProtocol(TemplatePointer value)
        {
            warnings.Clear();
            var template = _model.LoadTemplate(value);
            if (template != null)
            {
                ActiveTemplate = new TemplateViewModel(template, _model, _ea);
                ValidateControls(null, null);
            }
            else
            {
                StatusMessage = "Unable to load template, please click info icon for details...";
                warnings.AddRange(_model.ValidationErrors);
                ActiveTemplate = null;
            }
        }
        private void StartWait(string message)
        {
            WaitMessage = message;
            Working = true;
            RaisePropertyChangedEvent(nameof(StatusMessageVisibility));
        }
        private void EndWait()
        {
            WaitMessage = "";
            Working = false;
            RaisePropertyChangedEvent(nameof(StatusMessageVisibility));
        }
        private void Initialize()
        {
            try
            {
                SeriLogUI.Initialize(_model.LogPath, _model.CurrentUser); //    Initialize logger to same directory 
                _ui.Invoke(() =>
                {
                    ReloadTemplates();
                    CurrentStructureSet = _model.StructureSetId;
                });
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Exception during ViewModel initialization: {0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace);
                MessageBox.Show(errorMessage);
            }
            EndWait();

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

        public ICommand ShowWarningsCommand
        {
            get
            {
                return new DelegateCommand(ShowWarnings);
            }
        }

        private void ShowWarnings(object obj)
        {
            ReviewWarningsPopupVisibility ^= true;
            if (ScriptDone)
            {
                ReviewWarningsVM.Id = "Review generated structure warnings";
                ReviewWarningsVM.Description = HTMLWarningFormatter(warnings);
            }
            else
            {
                ReviewWarningsVM.Id = "Review input validation warnings";
                ReviewWarningsVM.Description = HTMLWarningFormatter(warnings);
            }
        }

        private string HTMLWarningFormatter(List<string> validationWarnings)
        {
            string html = "";
            foreach (var warning in validationWarnings)
            {
                html += "&bull; " + warning + "<br>";
            }
            return html;
        }

        public ICommand OpenTemplateFolderCommand
        {
            get
            {
                return new DelegateCommand(OpenTemplateFolder);
            }
        }

        public ICommand SaveAsPersonalCommand
        {
            get
            {
                return new DelegateCommand(SaveAsPersonal);
            }
        }

        private void SaveAsPersonal(object param = null)
        {
            if (ActiveTemplate == null)
            {
                StatusMessage = "No template loaded.";
            }
            else
            {
                SaveTemplateVM = new SaveNewTemplateViewModel(_ea, _model, ActiveTemplate.TemplateDisplayName);
                RaisePropertyChangedEvent(nameof(SaveTemplateVM));
                NewTemplateIdPopupVisibility ^= true;
            }
        }

        public bool NewTemplateIdPopupVisibility { get; set; } = false;
        public ICommand ReloadTemplateCommand
        {
            get
            {
                return new DelegateCommand(ReloadTemplates);
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
            try
            {
                (warnings) = await _model.GenerateStructures();
                if (HasWarnings)
                {
                    StatusMessage = "Structures generated with warnings, click for details";
                }
                else
                {
                    StatusMessage = "Structures generated successfully";
                }
                WaitMessage = "";
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Error during structure creation...");
                SeriLogUI.AddError(errorMessage);
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                WaitMessage = errorMessage;
            }
            RaisePropertyChangedEvent(nameof(StatusBorderColor));
            RaisePropertyChangedEvent(nameof(HasWarnings));
            CanLoadTemplates = true;
            Working = false;
            ScriptDone = true;

        }


        public async void OpenTemplateFolder(object param = null)
        {
            try
            {
                await Task.Run(() => Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", _model.GetUserTemplatePath()));
            }
            catch (Exception ex)
            {
                SeriLogUI.AddError(string.Format("Error opening template folder: {0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                MessageBox.Show(string.Format("Error opening template folder: {0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));

            }
        }

        public void ReloadTemplates(object param = null)
        {
            ActiveTemplate = null;
            warnings.Clear();
            Working = true;
            ScriptDone = false;
            TemplatePointers.Clear();
            TemplatePointers.SuppressNotification = true;
            foreach (TemplatePointer tp in _model.GetTemplates())
            {
                TemplatePointers.Add(tp);
            }
            TemplatePointers.SuppressNotification = false;
            Working = false;
            WaitMessage = "";
        }

      
    }
}
