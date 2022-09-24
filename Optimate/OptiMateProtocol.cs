
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using VMS.TPS;
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class OptiMateProtocol
{

    private byte versionField;

    private OptiMateProtocolOptiStructure[] optiStructuresField;

    private string protocolDisplayNameField;

    /// <remarks/>
    public byte version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("OptiStructure", IsNullable = false)]
    public OptiMateProtocolOptiStructure[] OptiStructures
    {
        get
        {
            return this.optiStructuresField;
        }
        set
        {
            this.optiStructuresField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ProtocolDisplayName
    {
        get
        {
            return this.protocolDisplayNameField;
        }
        set
        {
            this.protocolDisplayNameField = value;
        }
    }
}

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
    /// <remarks/>
    [System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[PropertyChanged.AddINotifyPropertyChangedInterface]
public partial class OptiMateProtocolOptiStructure
{

    private OptiMateProtocolOptiStructureInstruction[] instructionField;

    private string structureIdField;

    private bool isHighResolutionField = false;

    private string typeField = "CONTROL";

    private string baseStructureField;

    private bool isNewField = false;

    private bool isNewFieldSpecified;

    public OptiMateProtocolOptiStructure()
    {
        AddError(nameof(StructureId), @"Structure must have an Id");
    }

      /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Instruction")]
    public OptiMateProtocolOptiStructureInstruction[] Instruction
    {
        get
        {
            return this.instructionField;
        }
        set
        {
            this.instructionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string StructureId
    {
        get
        {
            return this.structureIdField;
        }
        set
        {
            ClearErrors(nameof(StructureId));
            if (string.IsNullOrEmpty(value))
                AddError(nameof(StructureId), @"Structure must have an Id");
            this.structureIdField = value.Substring(0,Math.Min(value.Length,16));

        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool isHighResolution
    {
        get
        {
            return this.isHighResolutionField;
        }
        set
        {
            this.isHighResolutionField = value;
        }
    }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool isNew
    {
        get
        {
            return this.isNewField;
        }
        set
        {
            this.isNewField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string BaseStructure
    {
        get
        {
            return this.baseStructureField;
        }
        set
        {
            this.baseStructureField = value;
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
            OperatorTypes OT;
            if (Enum.TryParse(Operator, out OT))
            {
                if (OT != OperatorTypes.margin)
                    return (!string.IsNullOrEmpty(targetField));
                else
                    return true;
            }
            return true;
        }
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


/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[PropertyChanged.AddINotifyPropertyChangedInterface]
public partial class OptiMateProtocolOptiStructureInstruction : ObservableObject
{

    private string operatorField;

    private string defaultTargetField;

    private string targetField;

    private string operatorParameterField;
   
    private string operatorParameter2Field;

    private bool operatorParameter2FieldSpecified;

    private string operatorParameter3Field;

    private bool operatorParameter3FieldSpecified;

    private string operatorParameter4Field;

    private bool operatorParameter4FieldSpecified;

    private string operatorParameter5Field;

    private bool operatorParameter5FieldSpecified;

    private string operatorParameter6Field;

    private bool operatorParameter6FieldSpecified;

    private string operatorParameter7Field;

    private bool operatorParameter7FieldSpecified;

    private bool isNewField;

    private bool isNewFieldSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Operator
    {
        get
        {
            return this.operatorField;
        }
        set
        {
            this.operatorField = value;
            // reset all operator parameters
            OperatorParameter = string.Empty;
            OperatorParameter2 = string.Empty;
            OperatorParameter3 = string.Empty;
            OperatorParameter4 = string.Empty;
            OperatorParameter5 = string.Empty;
            OperatorParameter6 = string.Empty;
            OperatorParameter7 = string.Empty;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string DefaultTarget
    {
        get
        {
            return this.defaultTargetField;
        }
        set
        {
            this.defaultTargetField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Target
    {
        get
        {
            return this.targetField;
        }
        set
        {
            this.targetField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string OperatorParameter
    {
        get
        {
            return this.operatorParameterField;
        }
        set
        {
            OperatorTypes OT;
            ClearErrors(nameof(OperatorParameter));
            double marginVal;
            if (Enum.TryParse(Operator, out OT))
            {
                switch (OT)
                {
                    case OperatorTypes.margin:
                        if (string.IsNullOrEmpty(value))
                            AddError(nameof(OperatorParameter), @"Margin is not defined");
                        if (double.TryParse(value, out marginVal))
                        {
                            if (marginVal < -20)
                                AddError(nameof(OperatorParameter), @"Internal margin too large");
                            if (marginVal > 50)
                                AddError(nameof(OperatorParameter), @"External margin too large");
                        }
                        else
                            AddError(nameof(OperatorParameter), @"Invalid input");
                        break;
                    case OperatorTypes.crop:
                        if (double.TryParse(value, out marginVal))
                        {
                            if (marginVal < 0)
                                AddError(nameof(OperatorParameter), @"Crop margin must be positive");
                            if (marginVal > 50)
                                AddError(nameof(OperatorParameter), @"Crop margin too large");
                        }
                        else
                            if (!string.IsNullOrEmpty(value))
                                AddError(nameof(OperatorParameter), @"Invalid input");
                        break;

                }
                RaisePropertyChangedEvent(nameof(operatorParameterColor));
                RaisePropertyChangedEvent(nameof(operatorParameterError));
            }
            this.operatorParameterField = value;
        }
    }


    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string OperatorParameter2
    {
        get
        {
            return this.operatorParameter2Field;
        }
        set
        {
            OperatorTypes OT;
            ClearErrors(nameof(OperatorParameter2));
            if (!string.IsNullOrEmpty(value))
            {
                if (Enum.TryParse(Operator, out OT))
                {
                    switch (OT)
                    {
                        case OperatorTypes.margin:
                            double marginVal;
                            if (double.TryParse(value, out marginVal))
                            {
                                if (marginVal < 0)
                                    AddError(nameof(OperatorParameter2), @"Margin must be positive");
                                if (marginVal > 50)
                                    AddError(nameof(OperatorParameter2), @"Margin too large");
                            }
                            else
                                AddError(nameof(OperatorParameter2), @"Invalid input");
                            break;

                    }
                }
            }
            RaisePropertyChangedEvent(nameof(operatorParameter2Color));
            RaisePropertyChangedEvent(nameof(operatorParameter2Error));
            this.operatorParameter2Field = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool OperatorParameter2Specified
    {
        get
        {
            return this.operatorParameter2FieldSpecified;
        }
        set
        {
            this.operatorParameter2FieldSpecified = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string OperatorParameter3
    {
        get
        {
            return this.operatorParameter3Field;
        }
        set
        {
            OperatorTypes OT;
            ClearErrors(nameof(OperatorParameter3));
            if (!string.IsNullOrEmpty(value))
            {
                if (Enum.TryParse(Operator, out OT))
                {
                    switch (OT)
                    {
                        case OperatorTypes.margin:
                            double marginVal;
                            if (double.TryParse(value, out marginVal))
                            {
                                if (marginVal < 0)
                                    AddError(nameof(OperatorParameter3), @"Margin must be positive");
                                if (marginVal > 50)
                                    AddError(nameof(OperatorParameter3), @"Margin too large");
                            }
                            else
                                AddError(nameof(OperatorParameter3), @"Invalid input");
                            break;

                    }
                }
            }
            RaisePropertyChangedEvent(nameof(operatorParameter3Color));
            RaisePropertyChangedEvent(nameof(operatorParameter3Error));
            this.operatorParameter3Field = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool OperatorParameter3Specified
    {
        get
        {
            return this.operatorParameter3FieldSpecified;
        }
        set
        {
            this.operatorParameter3FieldSpecified = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string OperatorParameter4
    {
        get
        {
            return this.operatorParameter4Field;
        }
        set
        {
            OperatorTypes OT;
            ClearErrors(nameof(OperatorParameter4));
            if (!string.IsNullOrEmpty(value))
            {
                if (Enum.TryParse(Operator, out OT))
                {
                    switch (OT)
                    {
                        case OperatorTypes.margin:
                            double marginVal;
                            if (double.TryParse(value, out marginVal))
                            {
                                if (marginVal < 0)
                                    AddError(nameof(OperatorParameter4), @"Margin must be positive");
                                if (marginVal > 50)
                                    AddError(nameof(OperatorParameter4), @"Margin too large");
                            }
                            else
                                AddError(nameof(OperatorParameter4), @"Invalid input");
                            break;

                    }
                }
            }
            RaisePropertyChangedEvent(nameof(operatorParameter4Color));
            RaisePropertyChangedEvent(nameof(operatorParameter4Error));
            this.operatorParameter4Field = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool OperatorParameter4Specified
    {
        get
        {
            return this.operatorParameter4FieldSpecified;
        }
        set
        {
            this.operatorParameter4FieldSpecified = value;
        }
    }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string OperatorParameter5
    {
        get
        {
            return this.operatorParameter5Field;
        }
        set
        {
            OperatorTypes OT;
            ClearErrors(nameof(OperatorParameter5));
            if (!string.IsNullOrEmpty(value))
            {
                if (Enum.TryParse(Operator, out OT))
                {
                    switch (OT)
                    {
                        case OperatorTypes.margin:
                            double marginVal;
                            if (double.TryParse(value, out marginVal))
                            {
                                if (marginVal < 0)
                                    AddError(nameof(OperatorParameter5), @"Margin must be positive");
                                if (marginVal > 50)
                                    AddError(nameof(OperatorParameter5), @"Margin too large");
                            }
                            else
                                AddError(nameof(OperatorParameter5), @"Invalid input");
                            break;

                    }
                }
            }
            RaisePropertyChangedEvent(nameof(operatorParameter5Color));
            RaisePropertyChangedEvent(nameof(operatorParameter5Error));
            this.operatorParameter5Field = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool OperatorParameter5Specified
    {
        get
        {
            return this.operatorParameter5FieldSpecified;
        }
        set
        {
            this.operatorParameter5FieldSpecified = value;
        }
    }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string OperatorParameter6
    {
        get
        {
            return this.operatorParameter6Field;
        }
        set
        {
            OperatorTypes OT;
            ClearErrors(nameof(OperatorParameter6));
            if (!string.IsNullOrEmpty(value))
            {
                if (Enum.TryParse(Operator, out OT))
                {
                    switch (OT)
                    {
                        case OperatorTypes.margin:
                            double marginVal;
                            if (double.TryParse(value, out marginVal))
                            {
                                if (marginVal < 0)
                                    AddError(nameof(OperatorParameter6), @"Margin must be positive");
                                if (marginVal > 50)
                                    AddError(nameof(OperatorParameter6), @"Margin too large");
                            }
                            else
                                AddError(nameof(OperatorParameter6), @"Invalid input");
                            break;

                    }
                }
            }
            RaisePropertyChangedEvent(nameof(operatorParameter6Color));
            RaisePropertyChangedEvent(nameof(operatorParameter6Error));
            this.operatorParameter6Field = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool OperatorParameter6Specified
    {
        get
        {
            return this.operatorParameter6FieldSpecified;
        }
        set
        {
            this.operatorParameter6FieldSpecified = value;
        }
    }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string OperatorParameter7
    {
        get
        {
            return this.operatorParameter7Field;
        }
        set
        {
            OperatorTypes OT;
            ClearErrors(nameof(OperatorParameter7));
            if (!string.IsNullOrEmpty(value))
            {
                if (Enum.TryParse(Operator, out OT))
                {
                    switch (OT)
                    {
                        case OperatorTypes.margin:
                            double marginVal;
                            if (double.TryParse(value, out marginVal))
                            {
                                if (marginVal < 0)
                                    AddError(nameof(OperatorParameter7), @"Margin must be positive");
                                if (marginVal > 50)
                                    AddError(nameof(OperatorParameter7), @"Margin too large");
                            }
                            else
                                AddError(nameof(OperatorParameter7), @"Invalid input");
                            break;

                    }
                }
            }
            RaisePropertyChangedEvent(nameof(operatorParameter7Color));
            RaisePropertyChangedEvent(nameof(operatorParameter7Error));
            this.operatorParameter7Field = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool OperatorParameter7Specified
    {
        get
        {
            return this.operatorParameter7FieldSpecified;
        }
        set
        {
            this.operatorParameter7FieldSpecified = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool isNew
    {
        get
        {
            return this.isNewField;
        }
        set
        {
            this.isNewField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool isNewSpecified
    {
        get
        {
            return this.isNewFieldSpecified;
        }
        set
        {
            this.isNewFieldSpecified = value;
        }
    }
}

