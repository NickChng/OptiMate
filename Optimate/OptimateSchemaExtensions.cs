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
        // properties for error checking
        public bool isBaseStructureValid
        {
            get
            {
                return !HasError(nameof(BaseStructure));
            }
        }

        public string baseStructureError
        {
            get
            {
                var errors = GetErrors(nameof(BaseStructure));
                if (errors == null)
                    return null;
                else
                    return string.Join("\r", (List<string>)errors);
            }
        }
        public SolidColorBrush BaseStructureColor
        {
            get
            {
                if (isBaseStructureValid)
                    return new SolidColorBrush(Colors.White);
                else
                    return new SolidColorBrush(Colors.Orange);
            }
        }

    }
    public partial class OptiMateProtocolOptiStructureInstruction : ObservableObject
    {
        // This contains error management and formatting properties

        public bool isTargetParameterValid
        {
            get
            {
                if (Operator != OperatorType.margin)
                    return (!string.IsNullOrEmpty(targetField));
                else
                    return true;

            }
        }

        public bool isOperatorParameterValid
        {
            get
            {
                ClearErrors(nameof(OperatorParameter));
                switch (Operator)
                {
                    case OperatorType.margin:
                        if (double.TryParse(OperatorParameter, out double marginValue))
                            if (marginValue > 50 || marginValue < -20)
                                AddError(nameof(OperatorParameter), @"Value out of range");
                        break;
                    case OperatorType.crop:
                        if (double.TryParse(OperatorParameter, out double cropValue))
                            if (cropValue > 50 || cropValue < 0)
                                AddError(nameof(OperatorParameter), @"Value out of range");
                        break;
                }
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

        public bool isOperatorParameter2Valid
        {
            get
            {
                ClearErrors(nameof(OperatorParameter2));
                switch (Operator)
                {
                    case OperatorType.margin:
                        if (double.TryParse(OperatorParameter2, out double marginValue))
                            if (marginValue > 50 || marginValue < 0)
                                AddError(nameof(OperatorParameter2), @"Value out of range");
                        break;
                }
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

        public bool isOperatorParameter3Valid
        {
            get
            {
                ClearErrors(nameof(OperatorParameter3));
                switch (Operator)
                {
                    case OperatorType.margin:
                        if (double.TryParse(OperatorParameter3, out double marginValue))
                            if (marginValue > 50 || marginValue < 0)
                                AddError(nameof(OperatorParameter3), @"Value out of range");
                        break;
                }
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

        public bool isOperatorParameter4Valid
        {
            get
            {
                ClearErrors(nameof(OperatorParameter4));
                switch (Operator)
                {
                    case OperatorType.margin:
                        if (double.TryParse(OperatorParameter4, out double marginValue))
                            if (marginValue > 50 || marginValue < 0)
                                AddError(nameof(OperatorParameter4), @"Value out of range");
                        break;
                }
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
        public bool isOperatorParameter5Valid
        {
            get
            {
                ClearErrors(nameof(OperatorParameter5));
                switch (Operator)
                {
                    case OperatorType.margin:
                        if (double.TryParse(OperatorParameter5, out double marginValue))
                            if (marginValue > 50 || marginValue < 0)
                                AddError(nameof(OperatorParameter5), @"Value out of range");
                        break;
                }
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

        public bool isOperatorParameter6Valid
        {
            get
            {
                ClearErrors(nameof(OperatorParameter6));
                switch (Operator)
                {
                    case OperatorType.margin:
                        if (double.TryParse(OperatorParameter6, out double marginValue))
                            if (marginValue > 50 || marginValue < 0)
                                AddError(nameof(OperatorParameter6), @"Value out of range");
                        break;
                }
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
        public bool isOperatorParameter7Valid
        {
            get
            {
                ClearErrors(nameof(OperatorParameter7));
                switch (Operator)
                {
                    case OperatorType.margin:
                        if (double.TryParse(OperatorParameter7, out double marginValue))
                            if (marginValue > 50 || marginValue < 0)
                                AddError(nameof(OperatorParameter7), @"Value out of range");
                        break;
                }
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
