using OptiMate;
using OptiMate.ViewModels;
using OptiMate.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using OptiMate.Logging;
using OptiMate.Controls;
using System.Windows.Input;

namespace OptiMate.ViewModels
{
    public class TemplateStructureViewModel : ObservableObject
    {
        private MainModel _model;
        private TemplateStructure _templateStructure;
        private IEventAggregator _ea;
        
        public ObservableCollection<EclipseStructureViewModel> EclipseIds { get; set; } = new ObservableCollection<EclipseStructureViewModel>()
        {
            new EclipseStructureViewModel(){EclipseId = "DesignEclipseId1" },
            new EclipseStructureViewModel(){EclipseId = "DesignEclipseId1" }
        };

        public ObservableCollection<string> Aliases { get; private set; } = new ObservableCollection<string>();

        public EditControlViewModel EditControlVM { get; set; }
        public string PrincipalAlias
        {
            get
            {
                if (Aliases.Count > 0)
                {
                    return Aliases[0];
                }
                else
                {
                    return "";
                }
            }
        }

        public string TemplateStructureId
        {
            get
            {
                return _templateStructure.TemplateStructureId;
            }
            set
            {
                if (_model.IsNewTemplateStructureIdValid(value))
                {
                    _model.RenameTemplateStructure(_templateStructure.TemplateStructureId, value);
                    RaisePropertyChangedEvent(nameof(StructureIdColor));
                }
            }
        }


        private EclipseStructureViewModel _selectedEclipseStructure;
        public EclipseStructureViewModel SelectedEclipseStructure
        {
            get
            {
                return _selectedEclipseStructure;
            }
            set
            {
                if (value != _selectedEclipseStructure)
                {
                    _selectedEclipseStructure = value;
                    _templateStructure.EclipseStructureId = value.EclipseId;
                    ClearErrors(nameof(SelectedEclipseStructure));
                    RaisePropertyChangedEvent(nameof(WarningVisibilityDoesNotUseAlias));
                    RaisePropertyChangedEvent(nameof(WarningVisibilityMappedStructureEmpty));
                    RaisePropertyChangedEvent(nameof(MappedStructureWarningColor));
                    _ea.GetEvent<DataValidationRequiredEvent>().Publish();
                }
            }
        }
        public bool isNew { get; set; }
        public SolidColorBrush StructureIdColor
        {
            get
            {
                if (HasError(nameof(TemplateStructureId)))
                {
                    return new SolidColorBrush(Colors.Orange);
                }
                else
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        public SolidColorBrush MappedStructureWarningColor
        {
            get
            {
                if (HasError(nameof(SelectedEclipseStructure)))
                {
                    return new SolidColorBrush(Colors.Orange);
                }
                else
                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
            }
        }

        public Visibility WarningVisibilityDoesNotUseAlias
        {
            get
            {
                if (SelectedEclipseStructure == null)
                {
                    return Visibility.Collapsed;
                }
                if (isAnAlias(SelectedEclipseStructure.EclipseId))
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public bool ConfirmRemoveStructurePopupVisibility { get; set; }= false;

        public Visibility WarningVisibilityMappedStructureEmpty
        {
            get
            {
                if (SelectedEclipseStructure == null)
                {
                    return Visibility.Collapsed;
                }
                else if (_model.IsEmpty(SelectedEclipseStructure.EclipseId))
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public TemplateStructureViewModel(TemplateStructure templateStructure, MainModel model, IEventAggregator ea, bool isNew = false)
        {
            _templateStructure = templateStructure;
            _model = model;
            _ea = ea;
            RegisterEvents();
            this.isNew = isNew;
            EclipseIds.Clear();
            foreach (var Id in _model.GetEclipseStructureIds())
            {
                EclipseIds.Add(new EclipseStructureViewModel(Id, _model));
            }
            ea.GetEvent<StructureGeneratedEvent>().Subscribe(OnNewStructureCreated);
            ea.GetEvent<NewTemplateStructureEvent>().Subscribe(OnNewTemplateStructure);
            if (templateStructure.Alias != null)
            {
                foreach (string alias in templateStructure.Alias)
                {
                    Aliases.Add(alias);
                }
                ApplyAliases();
            }
            else
                AddError(nameof(SelectedEclipseStructure), $"No alias for template structure {TemplateStructureId} was found in the structure set.");
            RaisePropertyChangedEvent(nameof(MappedStructureWarningColor));
        }

        private void RegisterEvents()
        {
            ErrorsChanged += (sender, args) =>
            {
                _ea.GetEvent<DataValidationRequiredEvent>().Publish();
            };
            _ea.GetEvent<TemplateStructureIdChangedEvent>().Subscribe(OnTemplateStructureIdChanged);
        }

        private void OnTemplateStructureIdChanged(TemplateStructureIdChangedEventInfo info)
        {
            if (string.Equals(info.NewId, TemplateStructureId, StringComparison.OrdinalIgnoreCase))
            {
                RaisePropertyChangedEvent(nameof(TemplateStructureId));
            }
        }

        private void OnNewTemplateStructure(NewTemplateStructureEventInfo info)
        {
            if (!EclipseIds.Select(x => x.EclipseId).Contains(info.Structure.TemplateStructureId))
            {
                EclipseIds.Add(new EclipseStructureViewModel(info.Structure.TemplateStructureId, _model));
            }
        }

        public TemplateStructureViewModel(string newTemplateStructureId, MainModel model, IEventAggregator ea)
        {
            _templateStructure = new TemplateStructure() { TemplateStructureId = newTemplateStructureId };
            _model = model;
            _ea = ea;
            isNew = true;
            EclipseIds.Clear();
            foreach (var Id in _model.GetEclipseStructureIds())
            {
                EclipseIds.Add(new EclipseStructureViewModel(Id, _model));
            }
        }

        private void ApplyAliases()
        {
            foreach (var eclipseId in EclipseIds.Select(x => x.EclipseId))
            {
                if (isAnAlias(eclipseId))
                {
                    SelectedEclipseStructure = EclipseIds.FirstOrDefault(x => x.EclipseId == eclipseId);
                }
            }
            if (SelectedEclipseStructure == null)
            {
                AddError(nameof(SelectedEclipseStructure), $"No alias for template structure {TemplateStructureId} was found in the structure set.");
            }
        }

        private bool isAnAlias(string alias)
        {
            if (Aliases.Select(x=>x.CompactForm()).Contains(alias.CompactForm(), StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnNewStructureCreated(StructureGeneratedEventInfo info)
        {
            if (!EclipseIds.Select(x => x.EclipseId).Contains(info.Structure.StructureId))
            {
                EclipseIds.Add(new EclipseStructureViewModel(info.Structure.StructureId, _model));
            }
        }

        internal bool ValidateInputs(List<string> aggregateWarnings)
        {
            if (HasErrors)
            {
                aggregateWarnings.AddRange(GetAllErrors());
                return false;
            }
            else
                return true;
        }

        public TemplateStructureViewModel()
        {
            //Design only
            _templateStructure = new TemplateStructure() { TemplateStructureId = "DesignTemplateId", Alias = new string[] { "DesignAlias" } };
        }

        private bool _editTemplateStructurePopupVisibility = false;
        public bool EditTemplateStructurePopupVisibility 
        {
            get
            {
                return _editTemplateStructurePopupVisibility;
            }
            set
            {
                _editTemplateStructurePopupVisibility = value;
                _ea.GetEvent<LockingPopupEvent>().Publish(value);
            }
        }

        public ICommand EditTemplateStructureCommand
        {
            get
            {
                return new DelegateCommand(EditTemplateStructure);
            }
        }

        private void EditTemplateStructure(object obj)
        {
            string templateStructureId = obj as string;
            EditTemplateStructurePopupVisibility ^= true;
            EditControlVM = new EditControlViewModel(templateStructureId, _model.GetTemplateStructureAliases(templateStructureId), _model, _ea);
        }
    }
}
