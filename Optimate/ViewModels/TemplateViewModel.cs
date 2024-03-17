using Optimate;
using OptiMate.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Events;
using Optimate.ViewModels;

namespace OptiMate.ViewModels
{
    public class TemplateViewModel : ObservableObject
    {
        private OptiMateTemplate _template = new OptiMateTemplate() { TemplateDisplayName = "Default" };

        private MainModel _model;

        private IEventAggregator _ea;
        public ObservableCollection<GeneratedStructureViewModel> GeneratedStructuresVM { get; set; } = new ObservableCollection<GeneratedStructureViewModel>();
        public ObservableCollection<TemplateStructureViewModel> TemplateStructuresVM { get; set; } = new ObservableCollection<TemplateStructureViewModel>();

        //public ObservableCollection<EclipseStructureViewModel> EclipseIds { get; set; } = new ObservableCollection<EclipseStructureViewModel>()
        //{
        //    new EclipseStructureViewModel() { EclipseId = "DesignEclipseId1" },
        //    new EclipseStructureViewModel() { EclipseId = "DesignEclipseId2" }
        //};
        public string TemplateDisplayName
        {
            get
            {
                return _template.TemplateDisplayName;
            }
            set
            {
                if (value != _template.TemplateDisplayName)
                {
                    _template.TemplateDisplayName = value;
                }
            }
        }
        public TemplateViewModel(OptiMateTemplate template, MainModel model, IEventAggregator ea)
        {
            _template = template;
            _model = model;
            _ea = ea;
            RegisterEvents();
            //EclipseIds.Clear();
            //foreach (var id in _model.GetEclipseStructureIds())
            //{
            //    EclipseIds.Add(new EclipseStructureViewModel(id, _model));
            //}
            
            GeneratedStructuresVM = new ObservableCollection<GeneratedStructureViewModel>();
            TemplateStructuresVM = new ObservableCollection<TemplateStructureViewModel>();
            foreach (var structure in template.GeneratedStructures)
            {
                GeneratedStructuresVM.Add(new GeneratedStructureViewModel(structure, _model, _ea));
            }
            foreach (var structure in template.TemplateStructures)
            {
                TemplateStructuresVM.Add(new TemplateStructureViewModel(structure, _model, _ea));
            }

        }

        private void RegisterEvents()
        {
            ErrorsChanged += PublishToEventAggregator;


        }
        private void PublishToEventAggregator(object sender, DataErrorsChangedEventArgs e)
        {
            _ea.GetEvent<DataValidationRequiredEvent>().Publish();
        }

        public TemplateViewModel()
        {
            // Design use only
            GeneratedStructuresVM.Add(new GeneratedStructureViewModel());
            TemplateStructuresVM.Add(new TemplateStructureViewModel());
        }



        public ICommand AddGeneratedStructureCommand
        {
            get
            {
                return new DelegateCommand(AddGeneratedStructure);
            }
        }
        public void AddGeneratedStructure(object param = null)
        {

            GeneratedStructure genStructure = _model.AddGeneratedStructure();
            GeneratedStructuresVM.Add(new GeneratedStructureViewModel(genStructure, _model, _ea, true));
            _ea.GetEvent<DataValidationRequiredEvent>().Publish();

        }

        public ICommand RemoveGeneratedStructureCommand
        {
            get
            {
                return new DelegateCommand(RemoveGeneratedStructure);
            }
        }

        public void RemoveGeneratedStructure(object param = null)
        {
            var genStructureVM = param as GeneratedStructureViewModel;
            _model.RemoveGeneratedStructure(genStructureVM.StructureId);
            GeneratedStructuresVM.Remove(genStructureVM);
            _ea.GetEvent<DataValidationRequiredEvent>().Publish();
        }

        public ICommand AddTemplateStructureCommand
        {
            get
            {
                return new DelegateCommand(AddTemplateStructure);
            }
        }

        private void AddTemplateStructure(object obj)
        {
            TemplateStructure t = _model.AddTemplateStructure();
            var tSVM = new TemplateStructureViewModel(t, _model, _ea, true);
            TemplateStructuresVM.Add(tSVM);
            NotifyErrorsChanged(nameof(AddTemplateStructure));
        }

        public ICommand RemoveTemplateStructureCommand
        {
            get
            {
                return new DelegateCommand(RemoveTemplateStructure);
            }
        }

        private void RemoveTemplateStructure(object obj)
        {
            var tSVM = obj as TemplateStructureViewModel;
            if (tSVM != null)
            {
                _model.RemoveTemplateStructure(tSVM.TemplateStructureId);
                TemplateStructuresVM.Remove(tSVM);
                _ea.GetEvent<DataValidationRequiredEvent>().Publish();
            }
        }

        internal List<GeneratedStructure> GetStructuresToGenerate()
        {
            List<GeneratedStructure> structures = new List<GeneratedStructure>();
            foreach (var structure in _template.GeneratedStructures)
            {
                structures.Add(structure);
            }
            return structures;
        }

        internal List<TemplateStructure> GetTemplateStructures()
        {
            List<TemplateStructure> structures = new List<TemplateStructure>();
            foreach (var structure in _template.TemplateStructures)
            {
                structures.Add(structure);
            }
            return structures;
        }

        internal bool ValidateInputs(List<string> aggregateWarnings)
        {
            bool isValid = true;
            foreach (var structure in GeneratedStructuresVM)
            {
                if (!structure.ValidateInputs(aggregateWarnings))
                {
                    isValid = false;
                }
            }
            foreach (var structure in TemplateStructuresVM)
            {
                if (!structure.ValidateInputs(aggregateWarnings))
                {
                    isValid = false;
                }
            }
            return isValid;
        }
    }
}
