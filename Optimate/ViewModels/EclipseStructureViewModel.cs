using OptiMate;
using OptiMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OptiMate.ViewModels
{
    public class EclipseStructureViewModel : ObservableObject
    {
        private MainModel _model;
        public string EclipseId { get; set; }
        public SolidColorBrush EclipseIdColor { get; private set; }

        public EclipseStructureViewModel(string eclipseId, MainModel model)
        {
            _model = model;
            EclipseId = eclipseId;
            UpdateEclipseStructureColors();
        }

        public EclipseStructureViewModel() { }

        private void UpdateEclipseStructureColors()
        {
            if (_model.IsEmpty(EclipseId))
                EclipseIdColor = new SolidColorBrush(Colors.DarkGray);
            else
                EclipseIdColor = new SolidColorBrush(Colors.Black);
            RaisePropertyChangedEvent(nameof(EclipseIdColor));
        }
    }
}
