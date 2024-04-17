using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using OptiMate;
using OptiMate.ViewModels;
using OptiMate.Logging;

namespace OptiMate.Converters
{
    public class StructureTextBoxColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                bool V = (bool)value;
                if (V)
                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
                else
                    return new SolidColorBrush(Colors.White);
            }
            else
                return new SolidColorBrush(Colors.Transparent);

        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValidNonEmptyParameterToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            string target = value as string;
            if (string.IsNullOrEmpty(target))
                return new SolidColorBrush(Colors.Orange);
            else
            {
                int check;
                if (int.TryParse(target, out check))
                    return new SolidColorBrush(Colors.White);
                else
                    return new SolidColorBrush(Colors.Orange);
            }
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowRemoveInstructionButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var genStructure = value as GeneratedStructureViewModel;
            if (genStructure != null)
                if (genStructure.NumInstructions == 0)
                    return false;
                else
                    return true;
            else
                return false;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class isMarginInternal : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            string target = value as string;
            if (string.IsNullOrEmpty(target))
                return false;
            else
            {
                int check;
                if (int.TryParse(target, out check))
                {
                    if (check >= 0)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ValidIntegerParameterToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            string target = value as string;
            if (string.IsNullOrEmpty(target))
                return new SolidColorBrush(Colors.White);
            else
            {
                int check;
                if (int.TryParse(target, out check))
                    return new SolidColorBrush(Colors.White);
                else
                    return new SolidColorBrush(Colors.Orange);
            }
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TargetStatusToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string target = value as string;
            if (string.IsNullOrEmpty(target))
                return new SolidColorBrush(Colors.Orange);
            else
                return new SolidColorBrush(Colors.LightGoldenrodYellow);

        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;
            else
            {
                return value.ToString();
            }

        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            bool boolValue = (bool)value;
            return boolValue;
        }
    }

    public class CropVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            else
            {
                OperatorTypes op = (OperatorTypes)value;
                if (op == OperatorTypes.crop)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ExpanderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double MinHeight = double.Parse(parameter as string);
            double AnimationVar = (double)values[0];
            switch (values.Count())
            {
                case 0:
                    return MinHeight;
                case 1:
                    return AnimationVar * MinHeight;
                case 2:
                    return AnimationVar * MinHeight;
                case 3:
                    double? HeightPerElement = double.Parse(values[1] as string);
                    int? NumElements = values[2] as int?;
                    if (HeightPerElement != null && NumElements != null)
                        return AnimationVar * Math.Max((double)NumElements * (double)HeightPerElement, MinHeight);
                    else return MinHeight;
                default:
                    return MinHeight;
            }
            //return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }
    }

    public class AsymmetricCropVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            else
            {
                OperatorTypes op = (OperatorTypes)value;
                if (op == OperatorTypes.asymmetricCrop)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConvertDoseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            else
            {
                OperatorTypes op = (OperatorTypes)value;
                if (op == OperatorTypes.convertDose)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class AsymmetricMarginVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            else
            {
                OperatorTypes op = (OperatorTypes)value;
                if (op == OperatorTypes.asymmetricMargin)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MarginVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            else
            {
                OperatorTypes op = (OperatorTypes)value;
                if (op == OperatorTypes.margin)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //public class WarningColorConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if (value is WarningLevels)
    //        {
    //            WarningLevels V = (WarningLevels)value;
    //            switch (V)
    //            {
    //                case WarningLevels.Failure:
    //                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
    //                case WarningLevels.NoWarningCTVs:
    //                    return new SolidColorBrush(Colors.PaleGreen);
    //                case WarningLevels.NoWarningPTVs:
    //                    return new SolidColorBrush(Colors.PaleGreen);
    //                case WarningLevels.SubvolumeWarningCTVs:
    //                    return new SolidColorBrush(Colors.Orange);
    //                case WarningLevels.SubvolumeWarningCTVen:
    //                    return new SolidColorBrush(Colors.Orange);
    //                case WarningLevels.SubvolumeWarningGTVn:
    //                    return new SolidColorBrush(Colors.Tomato);
    //                case WarningLevels.SubvolumeWarningGTVp:
    //                    return new SolidColorBrush(Colors.Tomato);
    //                default:
    //                    return new SolidColorBrush(Colors.LightGray);
    //            }
    //        }
    //        else
    //            return new SolidColorBrush(Colors.Transparent);

    //    }
    //    public object ConvertBack(object value, Type targetTypes,
    //           object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    public class OperatorToTargetVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            OperatorTypes Op = (OperatorTypes)value;
            switch (Op)
            {
                case OperatorTypes.margin:
                    return Visibility.Hidden;
                case OperatorTypes.asymmetricMargin:
                    return Visibility.Hidden;
                default:
                    return Visibility.Visible;
            }
            //}
            //else
            //    return Visibility.Visible;

        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class AddInstructionVisibilityConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] value, Type targetType,
    //           object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        OptiMateProtocolOptiStructureInstruction I = value[0] as OptiMateProtocolOptiStructureInstruction;
    //        var ProtocolStructure = value[1] as OptiMateProtocolOptiStructure;
    //        if (I != null && ProtocolStructure != null)
    //        {
    //            if (ProtocolStructure.Instruction.ToList().IndexOf(I) == ProtocolStructure.Instruction.Count() - 1)
    //                return Visibility.Visible;
    //        }
    //        return Visibility.Hidden;
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    public class AvailableOperators : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<OperatorTypes> Operators = value[0] as ObservableCollection<OperatorTypes>;
            var genStructure = value[1] as GeneratedStructureViewModel;
            var thisInstruction = value[2] as InstructionViewModel;
            if (genStructure?.InstructionViewModels != null)
            {
                var index = genStructure.InstructionViewModels.ToList().IndexOf(thisInstruction);
                if (index > 0)
                {
                    return Operators;
                }
                else
                    return new ObservableCollection<OperatorTypes>() { OperatorTypes.or, OperatorTypes.convertResolution, OperatorTypes.convertDose };
            }
            else
            {
                return new ObservableCollection<OperatorTypes>() { OperatorTypes.or, OperatorTypes.convertResolution, OperatorTypes.convertDose };
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class InstructionNumberConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            InstructionViewModel I = value[0] as InstructionViewModel;
            IEnumerable<InstructionViewModel> InstructionArray = value[1] as IEnumerable<InstructionViewModel>;
            if (I != null && InstructionArray != null)
                return string.Format(@"({0})", ((InstructionArray.ToList()).IndexOf(I) + 1));
            else
                return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InstructionCommandParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class AvailableStructureConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string currentOptiId = (value[0] as string);
                ObservableCollection<string> Ids = value[1] as ObservableCollection<string>;
                ObservableCollection<string> AvailableIds = new ObservableCollection<string>(Ids);
                OptiMateTemplate P = value[2] as OptiMateTemplate;
                string defaultId = (value[3] as string);
                if (currentOptiId == null)
                {
                    return Ids;
                }
                if (P != null)
                {
                    if (P.GeneratedStructures != null)
                    {
                        int Index = P.GeneratedStructures.Select(x => x.StructureId).ToList().IndexOf(currentOptiId);
                        if (Index >= 0)
                        {
                            foreach (var optiId in P.GeneratedStructures.Select(x => x.StructureId).ToList().Take(Index))
                                AvailableIds.Add(optiId);
                        }
                    }
                }
                if (AvailableIds.Count != 0 && !string.IsNullOrEmpty(defaultId))
                {
                    double[] LD = new double[AvailableIds.Count];
                    for (int i = 0; i < AvailableIds.Count; i++)
                    {
                        LD[i] = double.PositiveInfinity;
                    }
                    int c = 0;
                    foreach (string S in AvailableIds)
                    {
                        var CurrentId = defaultId.ToUpper();
                        var stripString = S.Replace(@"B_", @"").Replace(@"_", @"").ToUpper();
                        var CompString = CurrentId.Replace(@"B_", @"").Replace(@"_", @"").ToUpper();
                        double LDist = Helpers.LevenshteinDistance.Compute(stripString, CompString);
                        if (stripString.ToUpper().Contains(CompString) && stripString != "" && CompString != "")
                            LDist = Math.Min(LDist, 1.5);
                        LD[c] = LDist;
                        c++;
                    }
                    var temp = new ObservableCollection<string>(AvailableIds.Zip(LD, (s, l) => new { key = s, LD = l }).OrderBy(x => x.LD).Select(x => x.key).ToList());
                    return temp;
                }
                else
                    return AvailableIds;
            }
            catch (Exception ex)
            {
                SeriLogUI.AddError("Error in AvailableStructureConverter", ex);
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SortStructuresConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            string init = (value[0] as string);
            ObservableCollection<EclipseStructureViewModel> AvailableStructures = value[1] as ObservableCollection<EclipseStructureViewModel>;
            if (init == null)
            {
                return AvailableStructures;
            }
            if (AvailableStructures.Count != 0)
            {
                double[] LD = new double[AvailableStructures.Count];
                for (int i = 0; i < AvailableStructures.Count; i++)
                {
                    LD[i] = double.PositiveInfinity;
                }
                int c = 0;
                foreach (string S in AvailableStructures.Select(x=>x.EclipseId))
                {
                    var CurrentId = init.ToUpper();
                    var stripString = S.Replace(@"B_", @"").Replace(@"_", @"").ToUpper();
                    var CompString = CurrentId.Replace(@"B_", @"").Replace(@"_", @"").ToUpper();
                    double LDist = Helpers.LevenshteinDistance.Compute(stripString, CompString);
                    if (stripString.ToUpper().Contains(CompString) && stripString != "" && CompString != "")
                        LDist = Math.Min(LDist, 1.5);
                    LD[c] = LDist;
                    c++;
                }
                return AvailableStructures.Zip(LD, (s, l) => new { key = s, LD = l }).OrderBy(x => x.LD).Select(x => x.key).ToList();
                //var temp = new ObservableCollection<string>(AvailableStructures.Zip(LD, (s, l) => new { key = s, LD = l }).OrderBy(x => x.LD).Select(x => x.key).ToList());
                //return temp;
            }
            else
                return AvailableStructures;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConvertToOperatorName : IValueConverter
    {
        public object Convert(object value, Type targetType,
             object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is OperatorTypes)
            {
                OperatorTypes V = (OperatorTypes)value;
                return Helpers.OperatorName(V);
            }
            else
                return "";
        }
        public object ConvertBack(object value, Type targetTypes,
             object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            bool? V = value as bool?;
            if (V == null)
                return Visibility.Collapsed;
            if (V == true)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility? V = value as Visibility?;
            if (V == Visibility.Hidden)
                return false;
            else
                return true;
        }
    }
    public class VisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            bool? V = value as bool?;
            if (V == null)
                return Visibility.Collapsed;
            if (V == false)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility? V = value as Visibility?;
            if (V == Visibility.Hidden)
                return false;
            else
                return true;
        }
    }
    public class VisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (bool v in value)
            {
                if (v)
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class Color2Brush : IValueConverter
    //{
    //    public object Convert(object value, Type targetType,
    //          object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if (value != null)
    //            return new SolidColorBrush((Color)value);
    //        else
    //            return new SolidColorBrush(Colors.Transparent);
    //    }

    //    public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
