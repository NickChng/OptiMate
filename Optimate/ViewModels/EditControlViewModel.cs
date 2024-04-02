using OptiMate.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace OptiMate.ViewModels
{
    public class EditControlViewModel : ObservableObject
    {
        private MainModel _model;
        private IEventAggregator _ea;

        public bool UserConfirmed { get; private set; } = false;
        public int SelectedAliasIndex { get; set; }
        private string _templateStructureId = "DesignTemplateStructure";
        public string TemplateStructureId
        {
            get
            {
                return _templateStructureId;
            }
            set
            {
                if (_model.IsNewTemplateStructureIdValid(value))
                {
                    _model.RenameTemplateStructure(_templateStructureId, value);
                    _templateStructureId = value;
                }
            }
        }

        private string _newAlias;
        public string NewAlias
        {
            get { return _newAlias; }
            set
            {
                ClearErrors(nameof(NewAlias));
                _newAlias = value;
                if (!_model.IsAliasValid(TemplateStructureId, value))
                {
                    AddError(nameof(NewAlias), "Alias is invalid");
                }
                RaisePropertyChangedEvent(nameof(NewAliasTextBoxColor));
            }
        }

        public SolidColorBrush NewAliasTextBoxColor
        {
            get
            {
                if (HasError(nameof(NewAlias)))
                {
                    return Brushes.DarkOrange;
                }
                else
                {
                    return Brushes.LightGoldenrodYellow;
                }
            }
        }



        public ObservableCollection<string> Aliases { get; private set; } = new ObservableCollection<string>() { "DesignAlias1", "DesignAlias2", "DesignAlias3", "DesignAlias4" };

        public EditControlViewModel() { }
        public EditControlViewModel(string templateStructureId, List<string> aliases, MainModel model, IEventAggregator ea)
        {
            _templateStructureId = templateStructureId;
            Aliases.Clear();
            Aliases.AddRange(aliases);
            _model = model;
            _ea = ea;
        }


        public ICommand AddNewAliasCommand
        {
            get { return new DelegateCommand(AddNewAlias); }
        }

        private void AddNewAlias(object obj)
        {
            if (!_model.IsAliasValid(TemplateStructureId, _newAlias))
            {
                return;
            }
            else
            {
                _model.AddNewTemplateStructureAlias(TemplateStructureId, NewAlias);
                Aliases.Add(NewAlias);
            }
        }

        public ICommand RemoveAliasCommand
        {
            get { return new DelegateCommand(RemoveAlias); }
        }

        private void RemoveAlias(object obj)
        {
            string alias = obj as string;
            _model.RemoveTemplateStructureAlias(TemplateStructureId, alias);
            Aliases.Remove(alias);
        }



        internal void ReorderAliases(int a, int b)
        {
            _model.ReorderTemplateStructureAliases(TemplateStructureId, a, b);
            Aliases.Move(a, b);
        }


        public ICommand ConfirmCommand
        {
            get { return new DelegateCommand(Confirm); }
        }
        private void Confirm(object obj)
        {
            UserConfirmed = true;
            (obj as Popup).IsOpen = false;
        }
    }
}
