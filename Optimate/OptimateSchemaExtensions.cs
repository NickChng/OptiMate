using Optimate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Optimate
{
    public partial class OptiMateProtocolOptiStructure : ObservableObject
    {
        // This extension contains error management and formatting properties independent of the automatically generated class so it can be regenerated if needed

        public void StartDataValidationNotifications()
        {
            PropertyChanged += OptiMateProtocolOptiStructure_PropertyChanged;
            RaisePropertyChangedEvent(nameof(StructureId));
        }

        public void StopDataValidationNotifications()
        {
            PropertyChanged -= OptiMateProtocolOptiStructure_PropertyChanged;
        }

        private void OptiMateProtocolOptiStructure_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(StructureId):
                    {
                        ValidateStructureId();
                    }
                    break;
            }
        }

        public void ValidateStructureId()
        {
            ClearErrors(nameof(StructureId));
            if (string.IsNullOrEmpty(StructureId))
            {
                AddError(nameof(StructureId), @"Structure Id is required");
            }
            else if (StructureId.Length > 16)
                AddError(nameof(StructureId), @"Structure Id must be less than 16 characters");
            RaisePropertyChangedEvent(nameof(StructureIdColor));
            RaisePropertyChangedEvent(nameof(structureIdError));
        }

        public bool isStructureIdValid
        {
            get
            {
                return !HasError(nameof(StructureId));
            }
        }

        public string structureIdError
        {
            get
            {
                var errors = GetErrors(nameof(StructureId));
                if (errors == null)
                    return null;
                else
                    return string.Join("\r", (List<string>)errors);
            }
        }
        public SolidColorBrush StructureIdColor
        {
            get
            {
                if (isStructureIdValid)
                    return new SolidColorBrush(Colors.White);
                else
                    return new SolidColorBrush(Colors.Orange);
            }
        }

    }
    public partial class OptiMateProtocolOptiStructureInstruction : ObservableObject
    {
        // This extension contains error management and formatting properties independent of the automatically generated class so it can be regenerated if needed

        public void StartDataValidationNotifications()
        {
            PropertyChanged += OptiMateProtocolOptiStructureInstruction_PropertyChanged;
            RaisePropertyChangedEvent(nameof(Target));
            RaisePropertyChangedEvent(nameof(Operator));
            RaisePropertyChangedEvent(nameof(OperatorParameter));
            RaisePropertyChangedEvent(nameof(OperatorParameter2));
            RaisePropertyChangedEvent(nameof(OperatorParameter3));
            RaisePropertyChangedEvent(nameof(OperatorParameter4));
            RaisePropertyChangedEvent(nameof(OperatorParameter5));
            RaisePropertyChangedEvent(nameof(OperatorParameter6));
            RaisePropertyChangedEvent(nameof(OperatorParameter7));
        }
        public void StopDataValidationNotifications()
        {
            PropertyChanged -= OptiMateProtocolOptiStructureInstruction_PropertyChanged;
        }

        private void OptiMateProtocolOptiStructureInstruction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Target):
                    {
                        ValidateTarget();
                    }
                    break;
                case nameof(Operator):
                    {
                        ValidateTarget();
                        ValidateOperatorParameter();
                        ValidateOperatorParameter2();
                        ValidateOperatorParameter3();
                        ValidateOperatorParameter4();
                        ValidateOperatorParameter5();
                        ValidateOperatorParameter6();
                        ValidateOperatorParameter7();
                    }
                    break;
                case nameof(OperatorParameter):
                    {
                        ValidateOperatorParameter();
                    }
                    break;
                case nameof(OperatorParameter2):
                    {
                        ValidateOperatorParameter2();
                    }
                    break;
                case nameof(OperatorParameter3):
                    {
                        ValidateOperatorParameter3();
                    }
                    break;
                case nameof(OperatorParameter4):
                    {
                        ValidateOperatorParameter4();
                    }
                    break;
                case nameof(OperatorParameter5):
                    {
                        ValidateOperatorParameter5();
                    }
                    break;
                case nameof(OperatorParameter6):
                    {
                        ValidateOperatorParameter6();
                    }
                    break;
                case nameof(OperatorParameter7):
                    {
                        ValidateOperatorParameter7();
                    }
                    break;
            }
        }

        public void ValidateTarget()
        {
            ClearErrors(nameof(Target));
            switch (Operator)
            {
                case OperatorType.crop:
                    if (string.IsNullOrEmpty(Target))
                        AddError(nameof(Target), "Invalid selection");
                    break;
                case OperatorType.sub:
                    if (string.IsNullOrEmpty(Target))
                        AddError(nameof(Target), "Invalid selection");
                    break;
                case OperatorType.or:
                    if (string.IsNullOrEmpty(Target))
                        AddError(nameof(Target), "Invalid selection");
                    break;
                case OperatorType.subfrom:
                    if (string.IsNullOrEmpty(Target))
                        AddError(nameof(Target), "Invalid selection");
                    break;
                default:
                    break;
            }
        }

        public bool isTargetParameterValid
        {
            get
            {
                return !HasError(nameof(Target));

            }
        }

        public void ValidateOperatorParameter()
        {
            ClearErrors(nameof(OperatorParameter));
            switch (Operator)
            {
                case OperatorType.margin:
                    if (double.TryParse(OperatorParameter, out double marginValue))
                    {
                        if (marginValue > 50 || marginValue < -20)
                            AddError(nameof(OperatorParameter), @"Value out of range");
                    }
                    else
                        AddError(nameof(OperatorParameter), @"Input is invalid");
                    break;
                case OperatorType.crop:
                    if (string.IsNullOrEmpty(OperatorParameter))
                        break;
                    if (double.TryParse(OperatorParameter, out marginValue))
                    {
                        if (marginValue > 50 || marginValue < 0)
                            AddError(nameof(OperatorParameter), @"Value out of range");
                    }
                    else
                        AddError(nameof(OperatorParameter), @"Input is invalid");
                    break;
            }
            RaisePropertyChangedEvent(nameof(operatorParameterColor));
            RaisePropertyChangedEvent(nameof(operatorParameterError));
        }
        public bool isOperatorParameterValid
        {
            get
            {
                return !HasError(nameof(OperatorParameter));
            }
        }

        public string operatorParameterError
        {
            get
            {
                var errors = GetErrors(nameof(OperatorParameter));
                if (errors == null)
                    return null;
                else
                    return string.Join("\r", (List<string>)errors);
            }
        }
        public SolidColorBrush operatorParameterColor
        {
            get
            {
                if (isOperatorParameterValid)
                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
                else
                    return new SolidColorBrush(Colors.Tomato);
            }
        }

        public void ValidateOperatorParameter2()
        {
            ClearErrors(nameof(OperatorParameter2));
            switch (Operator)
            {
                case OperatorType.margin:
                    if (string.IsNullOrEmpty(OperatorParameter2))
                        break;
                    if (double.TryParse(OperatorParameter2, out double marginValue))
                    {
                        if (marginValue > 50 || marginValue < 0)
                            AddError(nameof(OperatorParameter2), @"Value out of range");
                    }
                    else
                        AddError(nameof(OperatorParameter2), @"Input is invalid");
                    break;
            }
            RaisePropertyChangedEvent(nameof(operatorParameter2Color));
            RaisePropertyChangedEvent(nameof(operatorParameter2Error));
        }

        public bool isOperatorParameter2Valid
        {
            get
            {
                return !HasError(nameof(OperatorParameter2));
            }
        }

        public string operatorParameter2Error
        {
            get
            {
                var errors = GetErrors(nameof(OperatorParameter2));
                if (errors == null)
                    return "";
                else
                    return string.Join("\r", (List<string>)errors);
            }
        }
        public SolidColorBrush operatorParameter2Color
        {
            get
            {
                if (isOperatorParameter2Valid)
                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
                else
                    return new SolidColorBrush(Colors.Tomato);
            }
        }

        public void ValidateOperatorParameter3()
        {
            ClearErrors(nameof(OperatorParameter3));
            switch (Operator)
            {
                case OperatorType.margin:
                    if (string.IsNullOrEmpty(OperatorParameter3))
                        break;
                    if (double.TryParse(OperatorParameter3, out double marginValue))
                    {
                        if (marginValue > 50 || marginValue < 0)
                            AddError(nameof(OperatorParameter3), @"Value out of range");
                    }
                    else
                        AddError(nameof(OperatorParameter3), @"Input is invalid");
                    break;
            }
            RaisePropertyChangedEvent(nameof(operatorParameter3Color));
            RaisePropertyChangedEvent(nameof(operatorParameter3Error));
        }
        public bool isOperatorParameter3Valid
        {
            get
            {
                return !HasError(nameof(OperatorParameter3));
            }
        }

        public string operatorParameter3Error
        {
            get
            {
                var errors = GetErrors(nameof(OperatorParameter3));
                if (errors == null)
                    return "";
                else
                    return string.Join("\r", (List<string>)errors);
            }
        }
        public SolidColorBrush operatorParameter3Color
        {
            get
            {
                if (isOperatorParameter3Valid)
                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
                else
                    return new SolidColorBrush(Colors.Tomato);
            }
        }
        public void ValidateOperatorParameter4()
        {
            ClearErrors(nameof(OperatorParameter4));
            switch (Operator)
            {
                case OperatorType.margin:
                    if (string.IsNullOrEmpty(OperatorParameter4))
                        break;
                    if (double.TryParse(OperatorParameter4, out double marginValue))
                    {
                        if (marginValue > 50 || marginValue < 0)
                            AddError(nameof(OperatorParameter4), @"Value out of range");
                    }
                    else
                        AddError(nameof(OperatorParameter4), @"Input is invalid");
                    break;
            }
            RaisePropertyChangedEvent(nameof(operatorParameter4Color));
            RaisePropertyChangedEvent(nameof(operatorParameter4Error));
        }
        public bool isOperatorParameter4Valid
        {
            get
            {
                return !HasError(nameof(OperatorParameter4));
            }
        }

        public string operatorParameter4Error
        {
            get
            {
                var errors = GetErrors(nameof(OperatorParameter4));
                if (errors == null)
                    return "";
                else
                    return string.Join("\r", (List<string>)errors);
            }
        }
        public SolidColorBrush operatorParameter4Color
        {
            get
            {
                if (isOperatorParameter4Valid)
                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
                else
                    return new SolidColorBrush(Colors.Tomato);
            }
        }
        public void ValidateOperatorParameter5()
        {
            ClearErrors(nameof(OperatorParameter5));
            switch (Operator)
            {
                case OperatorType.margin:
                    if (string.IsNullOrEmpty(OperatorParameter5))
                        break;
                    if (double.TryParse(OperatorParameter5, out double marginValue))
                    {
                        if (marginValue > 50 || marginValue < 0)
                            AddError(nameof(OperatorParameter5), @"Value out of range");
                    }
                    else
                        AddError(nameof(OperatorParameter5), @"Input is invalid");
                    break;
            }
            RaisePropertyChangedEvent(nameof(operatorParameter5Color));
            RaisePropertyChangedEvent(nameof(operatorParameter5Error));
        }
        public bool isOperatorParameter5Valid
        {
            get
            {
                return !HasError(nameof(OperatorParameter5));
            }
        }

        public string operatorParameter5Error
        {
            get
            {
                var errors = GetErrors(nameof(OperatorParameter5));
                if (errors == null)
                    return "";
                else
                    return string.Join("\r", (List<string>)errors);
            }
        }
        public SolidColorBrush operatorParameter5Color
        {
            get
            {
                if (isOperatorParameter5Valid)
                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
                else
                    return new SolidColorBrush(Colors.Tomato);
            }
        }

        public void ValidateOperatorParameter6()
        {
            ClearErrors(nameof(OperatorParameter6));
            switch (Operator)
            {
                case OperatorType.margin:
                    if (string.IsNullOrEmpty(OperatorParameter6))
                        break;
                    if (double.TryParse(OperatorParameter6, out double marginValue))
                    {
                        if (marginValue > 50 || marginValue < 0)
                            AddError(nameof(OperatorParameter6), @"Value out of range");
                    }
                    else
                        AddError(nameof(OperatorParameter6), @"Input is invalid");
                    break;
            }
            RaisePropertyChangedEvent(nameof(operatorParameter6Color));
            RaisePropertyChangedEvent(nameof(operatorParameter6Error));
        }
        public bool isOperatorParameter6Valid
        {
            get
            {
                return !HasError(nameof(OperatorParameter6));
            }
        }

        public string operatorParameter6Error
        {
            get
            {
                var errors = GetErrors(nameof(OperatorParameter6));
                if (errors == null)
                    return "";
                else
                    return string.Join("\r", (List<string>)errors);
            }
        }
        public SolidColorBrush operatorParameter6Color
        {
            get
            {
                if (isOperatorParameter6Valid)
                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
                else
                    return new SolidColorBrush(Colors.Tomato);
            }
        }
        public void ValidateOperatorParameter7()
        {
            ClearErrors(nameof(OperatorParameter7));
            switch (Operator)
            {
                case OperatorType.margin:
                    if (string.IsNullOrEmpty(OperatorParameter7))
                        break;
                    if (double.TryParse(OperatorParameter7, out double marginValue))
                    {
                        if (marginValue > 50 || marginValue < 0)
                            AddError(nameof(OperatorParameter7), @"Value out of range");
                    }
                    else
                        AddError(nameof(OperatorParameter7), @"Input is invalid");
                    break;
            }
            RaisePropertyChangedEvent(nameof(operatorParameter7Color));
            RaisePropertyChangedEvent(nameof(operatorParameter7Error));
        }
        public bool isOperatorParameter7Valid
        {
            get
            {
                return !HasError(nameof(OperatorParameter7));
            }
        }

        public string operatorParameter7Error
        {
            get
            {
                var errors = GetErrors(nameof(OperatorParameter7));
                if (errors == null)
                    return "";
                else
                    return string.Join("\r", (List<string>)errors);
            }
        }
        public SolidColorBrush operatorParameter7Color
        {
            get
            {
                if (isOperatorParameter7Valid)
                    return new SolidColorBrush(Colors.White);
                else
                    return new SolidColorBrush(Colors.Tomato);
            }
        }
    }

}
