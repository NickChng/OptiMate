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
using PropertyChanged;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Prism.Events;

[assembly: ESAPIScript(IsWriteable = true)]

namespace Optimate.ViewModels
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
                    _selectedTemplate = value;
                    InitializeProtocol(value);
                    RaisePropertyChangedEvent(nameof(ProtocolVisibility));
                    RaisePropertyChangedEvent(nameof(ActiveTemplate));
                    ValidateControls(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
        }

        public TemplateViewModel ActiveTemplate { get; set; }

       

        private void ValidateControls(object sender, DataErrorsChangedEventArgs e)
        {
            CheckAllInputsValid();
            RaisePropertyChangedEvent(nameof(allInputsValid));
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
                RaisePropertyChangedEvent(nameof(allInputsValid));
                RaisePropertyChangedEvent(nameof(StartButtonText));
            }
        }

        public bool HasCompletionWarnings { get; set; } = false;

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

        public string TemplatePath { get; set; }
        public string WaitMessage { get; set; }

        private bool _allInputsValid;

        private List<string> _validationWarnings;
        private List<string> _completionWarnings;
        private void CheckAllInputsValid()
        {
            ClearErrors(nameof(allInputsValid));
            StatusMessage = "Ready...";
            _validationWarnings = new List<string>();
            if (ActiveTemplate != null)
            {
                if (ActiveTemplate.ValidateInputs(_validationWarnings))
                {
                    _allInputsValid = true;
                }
                else
                {
                    _allInputsValid = false;
                    AddError(nameof(allInputsValid), "Inputs are not valid.");
                    StatusMessage = "Please review input parameters before continuing";
                }
            }
            else
            {
                _allInputsValid = false;
                AddError(nameof(allInputsValid), "No template loaded.");
            }
            RaisePropertyChangedEvent(nameof(allInputsValid));
        }
        public bool allInputsValid
        {
            get
            {
                return _allInputsValid;
            }
        }

        public SolidColorBrush ScriptCompletionStatusColour
        {
            get
            {
                return HasCompletionWarnings ? WarningColour : new SolidColorBrush(Colors.PaleGreen);
            }
        }

        public DescriptionViewModel ReviewInputValidationVM { get; set; } = new DescriptionViewModel("Review input validation errors", "");
        public DescriptionViewModel ReviewCompletionWarningsVM { get; set; } = new DescriptionViewModel("Review generated structure warnings", "");

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
            _ea.GetEvent<StructureGeneratedEvent>().Subscribe(UpdateStatus_GeneratedStructure);
            _ea.GetEvent<DataValidationRequiredEvent>().Subscribe(CheckAllInputsValid);
            _ea.GetEvent<ModelInitializedEvent>().Subscribe(Initialize);
        }

        private void UpdateStatus_GeneratedStructure(StructureGeneratedEventInfo info)
        {
            WaitMessage = $"{info.Structure.StructureId} created... ({info.IndexInQueue}/{info.TotalToGenerate})";
            Helpers.SeriLog.AddLog(WaitMessage);
        }

        private void InitializeProtocol(TemplatePointer value)
        {
            var template = _model.LoadTemplate(value);
            if (template != null)
                ActiveTemplate = new TemplateViewModel(template, _model, _ea);
            else
                ActiveTemplate = null;
        }
        private void StartWait(string message)
        {
            WaitMessage = message;
            Working = true;
        }
        private void EndWait()
        {
            WaitMessage = "";
            Working = false;
        }
        private void Initialize()
        {
            try
            {
                _ui.Invoke(() =>
                {
                    var AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    TemplatePath = Path.Combine(AssemblyPath, @"Templates");
                    ReloadTemplates();

                    CurrentStructureSet = _model.StructureSetId;
                });

            }
            catch (Exception ex)
            {
                Helpers.SeriLog.AddLog(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
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

        public ICommand ToggleWarningVisibilityCommand
        {
            get
            {
                return new DelegateCommand(ToggleWarningVisibility);
            }
        }

        public ICommand ShowValidationErrorsCommand
        {
            get
            {
                return new DelegateCommand(ShowValidationErrors);
            }
        }

        private void ShowValidationErrors(object obj)
        {
            ReviewInputValidationPopupVisibility ^= true;
            ReviewInputValidationVM.Description = HTMLWarningFormatter(_validationWarnings);
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

        public ICommand ReviewCompletionWarnignsCommand
        {
            get
            {
                return new DelegateCommand(ReviewCompletionWarnings);
            }
        }

        private void ReviewCompletionWarnings(object obj)
        {
            ReviewCompletionWarningsVM.Description = HTMLWarningFormatter(_completionWarnings);
            ReviewWarningsPopupVisibility ^= true;
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
                (HasCompletionWarnings, _completionWarnings) = await _model.GenerateStructures();
                RaisePropertyChangedEvent(nameof(ScriptCompletionStatusColour));
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Error during structure creation...");
                Helpers.SeriLog.AddError(errorMessage);
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                WaitMessage = errorMessage;
            }
            CanLoadTemplates = true;
            Working = false;
            ScriptDone = true;

        }


        public async void OpenTemplateFolder(object param = null)
        {
            await Task.Run(() => Process.Start(TemplatePath));
        }

        public void ReloadTemplates(object param = null)
        {
            WaitMessage = "Loading Templates...";
            Working = true;
            ScriptDone = false;
            TemplatePointers.Clear();
            TemplatePointers.SuppressNotification = true;

            try
            {
                XmlSerializer Ser = new XmlSerializer(typeof(OptiMateTemplate));

                foreach (var file in Directory.GetFiles(TemplatePath, "*.xml"))
                {
                    using (StreamReader protocol = new StreamReader(file))
                    {
                        try
                        {
                            var OMProtocol = (OptiMateTemplate)Ser.Deserialize(protocol);
                            TemplatePointers.Add(new TemplatePointer() { TemplateDisplayName = OMProtocol.TemplateDisplayName, TemplatePath = file });
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
            TemplatePointers.SuppressNotification = false;
            Working = false;
            WaitMessage = "";
        }

    }
}
