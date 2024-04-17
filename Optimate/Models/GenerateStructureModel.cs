using OptiMate;
using OptiMate.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using OptiMate.Logging;
using System.Windows.Interop;
using System.Drawing;
using System.Windows.Media.Animation;
using System.Windows.Documents;

namespace OptiMate.Models
{
    public partial class MainModel
    {
        internal bool isDoseLevelValid(ushort? value)
        {
            if (value == null || value < 0 || value > 50000)
                return false;
            else
                return true;
        }

        private class GenerateStructureModel
        {

            private EsapiWorker ew;
            public IEventAggregator _ea;
            private GeneratedStructure genStructure;
            private List<TemplateStructure> templateStructures;
            private Structure generatedEclipseStructure;
            private List<string> _warnings = new List<string>();
            private string TempStructureName = @"TEMP_OptiMate";

            public GenerateStructureModel(EsapiWorker ew, IEventAggregator ea, GeneratedStructure genStructure, List<TemplateStructure> templateStructures)
            {
                this.ew = ew;
                this._ea = ea;
                this.genStructure = genStructure;
                this.templateStructures = templateStructures;
            }

            private Structure GetTempTargetStructure(StructureSet S, TemplateStructure Target)
            {
                // returns temporary structure with the same resolution and segment volume as the target so it can be modified without changing the target
                string TargetId = Target.EclipseStructureId;
                if (string.IsNullOrEmpty(TargetId))
                    return null;
                var TargetStructure = S.Structures.FirstOrDefault(x => x.Id.ToUpper() == TargetId.ToUpper());
                if (TargetStructure == null)
                {
                    string warning = string.Format("Opti structure ({0}) creation operation instruction references target {1} which was not found", genStructure.StructureId, TargetId);
                    _warnings.Add(warning);
                    SeriLogModel.AddLog(warning);
                    return null;
                }
                else
                {
                    Structure Temp = S.Structures.FirstOrDefault(x => x.Id.ToUpper() == TempStructureName.ToUpper());
                    if (Temp != null)
                        S.RemoveStructure(Temp);
                    Temp = S.AddStructure("CONTROL", TempStructureName);
                    if (TargetStructure.IsHighResolution)
                    {
                        Temp.ConvertToHighResolution();
                    }
                    Temp.SegmentVolume = TargetStructure.SegmentVolume;
                    if (generatedEclipseStructure.IsHighResolution && !Temp.IsHighResolution)
                    {
                        // if output structure is indicated to be in high resolution and temp structure has not already been converted, convert temp to high resolution
                        Temp.ConvertToHighResolution();
                    }
                    return Temp;
                }
            }

            //private async Task<InstructionCompletionStatus> ApplyInstruction(Copy copyInstruction)
            //{
            //    InstructionCompletionStatus completionStatus = InstructionCompletionStatus.Pending;
            //    bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
            //    {
            //        try
            //        {
            //            string StructureToCopyId = templateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, copyInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase))?.EclipseStructureId;
            //            if (string.IsNullOrEmpty(StructureToCopyId))
            //            {
            //                string warningMessage = string.Format($"Copy target for structure {genStructure.StructureId} is null, skipping structure...");
            //                _warnings.Add(warningMessage);
            //                SeriLogModel.AddLog(warningMessage);
            //                completionStatus = InstructionCompletionStatus.Failed;
            //                return;
            //            }
            //            Structure BaseStructure = S.Structures.FirstOrDefault(x => string.Equals(x.Id, StructureToCopyId, StringComparison.OrdinalIgnoreCase));
            //            if (BaseStructure == null)
            //            {
            //                string warningMessage = string.Format($"Attempt to create structure {genStructure.StructureId} failed as copy target {StructureToCopyId} does not exist in structure set, skipping structure...");
            //                _warnings.Add(warningMessage);
            //                SeriLogModel.AddLog(warningMessage);
            //                completionStatus = InstructionCompletionStatus.Failed;
            //                return;
            //            }
            //            else if (BaseStructure.IsEmpty)
            //            {
            //                string warningMessage = string.Format($"Attempt to create structure {genStructure.StructureId} failed as copy target {StructureToCopyId} is empty, skipping structure...");
            //                _warnings.Add(warningMessage);
            //                SeriLogModel.AddLog(warningMessage);
            //                completionStatus = InstructionCompletionStatus.Failed;
            //                return;
            //            }
            //            else
            //            {
            //                if (string.Equals(StructureToCopyId, generatedEclipseStructure.Id, StringComparison.OrdinalIgnoreCase)) // if OS is the same as the copy structure, then only need to check for high res conversion
            //                {
            //                    if (genStructure.isHighResolution && !generatedEclipseStructure.IsHighResolution)
            //                    {
            //                        generatedEclipseStructure.ConvertToHighResolution();
            //                    }
            //                    return;
            //                }
            //                if (!generatedEclipseStructure.IsEmpty)
            //                {
            //                    generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.And(generatedEclipseStructure.SegmentVolume.Not()); // clear structure;
            //                }
            //                if (BaseStructure.IsHighResolution && !generatedEclipseStructure.IsHighResolution)
            //                {
            //                    generatedEclipseStructure.ConvertToHighResolution();
            //                    generatedEclipseStructure.SegmentVolume = BaseStructure.SegmentVolume;
            //                }
            //                else if (!BaseStructure.IsHighResolution && generatedEclipseStructure.IsHighResolution)
            //                {
            //                    var HRTemp = S.Structures.FirstOrDefault(x => x.Id == TempStructureName);
            //                    if (HRTemp != null)
            //                        S.RemoveStructure(HRTemp);
            //                    HRTemp = S.AddStructure(genStructure.DicomType, TempStructureName);
            //                    HRTemp.SegmentVolume = BaseStructure.SegmentVolume;
            //                    HRTemp.ConvertToHighResolution();
            //                    generatedEclipseStructure.SegmentVolume = HRTemp.SegmentVolume;
            //                    S.RemoveStructure(HRTemp);
            //                    generatedEclipseStructure.SegmentVolume = BaseStructure.SegmentVolume;
            //                }
            //                else
            //                    generatedEclipseStructure.SegmentVolume = BaseStructure.SegmentVolume;
            //                SeriLogModel.AddLog($"Copied structure {StructureToCopyId} to {genStructure.StructureId}");
            //                completionStatus = InstructionCompletionStatus.Completed;
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            SeriLogModel.AddError($"Exception in ApplyInstruction for {genStructure.StructureId}", e);
            //        }
            //    }));
            //    return completionStatus;
            //}

            private async Task<InstructionCompletionStatus> ApplyInstruction(Margin marginInstruction)
            {
                InstructionCompletionStatus completionStatus = InstructionCompletionStatus.Pending;
                bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                {
                    double IsotropicMargin = 0;
                    if (string.IsNullOrEmpty(marginInstruction.IsotropicMargin))
                    {
                        marginInstruction.IsotropicMargin = "0";
                    }
                    if (double.TryParse(marginInstruction.IsotropicMargin, out IsotropicMargin))
                    {
                        if (IsotropicMargin > 0 && IsotropicMargin < 50)
                        {
                            if (marginInstruction.MarginType == MarginTypes.Outer)
                                generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.Margin(IsotropicMargin);
                            else
                                generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.Margin(-IsotropicMargin);
                            SeriLogModel.AddLog($"Applied isotropic margin of {IsotropicMargin}mm to {genStructure.StructureId}");
                            completionStatus = InstructionCompletionStatus.Completed;
                        }
                        else
                        {
                            completionStatus = InstructionCompletionStatus.Failed;
                            SeriLogModel.AddWarning($"Isotropic margin for {genStructure.StructureId} is invalid (value = {IsotropicMargin}), skipping structure...");
                        }
                    }
                    else
                    {
                        completionStatus = InstructionCompletionStatus.Failed;
                        SeriLogModel.AddWarning($"Isotropic margin for {genStructure.StructureId} is invalid (value = {IsotropicMargin}), skipping structure...");
                    }
                }));
                return completionStatus;
            }

            private async Task<InstructionCompletionStatus> ApplyInstruction(AsymmetricMargin marginInstruction)
            {
                InstructionCompletionStatus completionStatus = InstructionCompletionStatus.Completed;
                bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                {
                    double leftmargin = 0;
                    double rightmargin = 0;
                    double antmargin = 0;
                    double postmargin = 0;
                    double infmargin = 0;
                    double supmargin = 0;
                    if (!double.TryParse(marginInstruction.RightMargin, out rightmargin))
                    {
                        string warning = "Right margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                    }
                    else
                    {
                        SeriLogModel.AddLog($"Right margin for {genStructure.StructureId} set to {rightmargin}");
                    }
                    if (!double.TryParse(marginInstruction.LeftMargin, out leftmargin))
                    {
                        string warning = "Left margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                    else
                    {
                        SeriLogModel.AddLog($"Left margin for {genStructure.StructureId} set to {leftmargin}");
                    }
                    if (!double.TryParse(marginInstruction.AntMargin, out antmargin))
                    {
                        string warning = "Anterior margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                    else
                    {
                        SeriLogModel.AddLog($"Anterior margin for {genStructure.StructureId} set to {antmargin}");
                    }
                    if (!double.TryParse(marginInstruction.PostMargin, out postmargin))
                    {
                        string warning = "Posterior margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                    else
                    {
                        SeriLogModel.AddLog($"Posterior margin for {genStructure.StructureId} set to {postmargin}");
                    }
                    if (!double.TryParse(marginInstruction.InfMargin, out infmargin))
                    {
                        string warning = "Inferior margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                    else
                    {
                        SeriLogModel.AddLog($"Inferior margin for {genStructure.StructureId} set to {infmargin}");
                    }
                    if (!double.TryParse(marginInstruction.SupMargin, out supmargin))
                    {
                        string warning = "Superior margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                    else
                    {
                        SeriLogModel.AddLog($"Superior margin for {genStructure.StructureId} set to {supmargin}");
                    }

                    var AAM = Helpers.OrientationInvariantMargins.getAxisAlignedMargins(S.Image.ImagingOrientation, marginInstruction.MarginType, rightmargin, antmargin, infmargin, leftmargin, postmargin, supmargin);
                    generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.AsymmetricMargin(AAM);
                }));
                return completionStatus;
            }

            private async Task<InstructionCompletionStatus> ApplyInstruction(AsymmetricCrop cropInstruction)
            {
                InstructionCompletionStatus completionStatus = InstructionCompletionStatus.Pending;
                bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                {
                    TemplateStructure TargetStructure = templateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, cropInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                    Structure EclipseTarget = GetTempTargetStructure(S, TargetStructure);
                    CheckForHRConversion(TargetStructure, EclipseTarget);
                    if (TargetStructure == null)
                    {
                        var warning = $"Target of CROP operation [{cropInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is null/empty, skipping instruction...";
                        _warnings.Add(warning);
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                        return;
                    }
                    else if (EclipseTarget.IsEmpty)
                    {
                        var warning = $"Target of CROP operation [{cropInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is empty, skipping instruction...";
                        _warnings.Add(warning);
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                        return;
                    }
                    //Determine crop offset parameters
                    double leftOffset = 0;
                    double rightOffset = 0;
                    double antOffset = 0;
                    double postOffset = 0;
                    double infOffset = 0;
                    double supOffset = 0;
                    var logMsg = $"Applying asymmetric crop for {genStructure.StructureId} from {TargetStructure.EclipseStructureId}";
                    // Anisotropic crop margins
                    logMsg = $"Using anisotropic crop margins for {genStructure.StructureId}...";
                    SeriLogModel.AddLog(logMsg);
                    if (!double.TryParse(cropInstruction.RightOffset, out rightOffset))
                    {
                        var warning = $"Right crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                    if (!double.TryParse(cropInstruction.LeftOffset, out leftOffset))
                    {
                        var warning = $"Left crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                    if (!double.TryParse(cropInstruction.AntOffset, out antOffset))
                    {
                        var warning = $"Anterior crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                    if (!double.TryParse(cropInstruction.PostOffset, out postOffset))
                    {
                        var warning = $"Posterior crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                    if (!double.TryParse(cropInstruction.InfOffset, out infOffset))
                    {
                        var warning = $"Inferior crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                    if (!double.TryParse(cropInstruction.SupOffset, out supOffset))
                    {
                        var warning = $"Superior crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                        SeriLogModel.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                    if (cropInstruction.InternalCrop)
                    {
                        MarginTypes cropType = MarginTypes.Inner;
                        var AAM = Helpers.OrientationInvariantMargins.getAxisAlignedMargins(S.Image.ImagingOrientation, cropType, rightOffset, antOffset, infOffset, leftOffset, postOffset, supOffset);
                        SeriLogModel.AddLog($"Using internal crop for {genStructure.StructureId}");
                        generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.And(EclipseTarget.SegmentVolume.AsymmetricMargin(AAM));
                    }
                    else
                    {
                        MarginTypes cropType = MarginTypes.Outer;
                        var AAM = Helpers.OrientationInvariantMargins.getAxisAlignedMargins(S.Image.ImagingOrientation, cropType, rightOffset, antOffset, infOffset, leftOffset, postOffset, supOffset);
                        SeriLogModel.AddLog($"Using external crop for {genStructure.StructureId}");
                        generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.Sub(EclipseTarget.SegmentVolume.AsymmetricMargin(AAM));
                    }

                }));
                return completionStatus;
            }
            private async Task<InstructionCompletionStatus> ApplyInstruction(Crop cropInstruction)
            {
                InstructionCompletionStatus completionStatus = InstructionCompletionStatus.Completed;
                bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                {
                    TemplateStructure TargetStructure = templateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, cropInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                    Structure EclipseTarget = GetTempTargetStructure(S, TargetStructure);
                    CheckForHRConversion(TargetStructure, EclipseTarget);
                    if (EclipseTarget == null)
                    {
                        var warning = $"Target of CROP operation [{cropInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is null/empty, skipping instruction...";
                        _warnings.Add(warning);
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                        return;
                    }
                    else
                    {
                        if (EclipseTarget.IsEmpty)
                        {
                            var warning = $"Target of CROP operation [{cropInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is empty, skipping instruction...";
                            _warnings.Add(warning);
                            completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                            return;
                        }
                        //Determine crop offset parameters
                        double isotropicOffset = 0;
                        var logMsg = $"Applying crop for {genStructure.StructureId} from {TargetStructure.EclipseStructureId}";
                        SeriLogModel.AddLog(logMsg);
                        if (string.IsNullOrEmpty(cropInstruction.IsotropicOffset))
                        {
                            cropInstruction.IsotropicOffset = "0";
                            var msg = $"Isotropic crop margin for {genStructure.StructureId} is not-specified, using zero offset...";
                            SeriLogModel.AddLog(msg);
                        }
                        if (double.TryParse(cropInstruction.IsotropicOffset, out isotropicOffset))
                        {
                            logMsg = $"Using isotropic crop margins for {genStructure.StructureId}...";
                            SeriLogModel.AddLog(logMsg);
                            if (isotropicOffset > -50 && isotropicOffset < 50)
                            {
                                if (cropInstruction.InternalCrop)
                                {
                                    SeriLogModel.AddLog($"Applying internal isotropic crop for {genStructure.StructureId}...");
                                    generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.And(EclipseTarget.SegmentVolume.Margin(-isotropicOffset));
                                }
                                else
                                {
                                    SeriLogModel.AddLog($"Applying external isotropic crop for {genStructure.StructureId}...");
                                    generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.Sub(EclipseTarget.SegmentVolume.Margin(isotropicOffset));
                                }
                            }
                            else
                            {
                                string errorMessage = $"Isotropic crop margin for {genStructure.StructureId} was outside the valid Eclipse range, aborting structure...";
                                _warnings.Add(errorMessage);
                                SeriLogModel.AddError(errorMessage);
                                completionStatus = InstructionCompletionStatus.Failed;
                            }
                        }
                        else
                        {
                            string errorMessage = $"Isotropic crop margin for {genStructure.StructureId} was specified but invalid, aborting structure...";
                            _warnings.Add(errorMessage);
                            SeriLogModel.AddError(errorMessage);
                            completionStatus = InstructionCompletionStatus.Failed;
                        }
                        logMsg = $"Applied crop to {genStructure.StructureId}...";
                        SeriLogModel.AddLog(logMsg);
                        S.RemoveStructure(EclipseTarget);
                    }
                }));
                return completionStatus;
            }


            private async Task<InstructionCompletionStatus> ApplyInstruction(ConvertDose convertDoseInstruction)
            {
                InstructionCompletionStatus completionStatus = InstructionCompletionStatus.Completed;
                bool Done = await Task.Run(() => ew.AsyncRunPlanContext((p, pl, S, ui) =>
                {
                    if (pl == null)
                    {
                        completionStatus = InstructionCompletionStatus.Failed;
                        var warning = $"Convert dose instruction failed as no plan was found";
                        _warnings.Add(warning);
                        SeriLogModel.AddWarning(warning);
                    }
                    else if (pl.Dose == null)
                    {
                        completionStatus = InstructionCompletionStatus.Failed;
                        var warning = $"Convert dose instruction failed as no dose distribution was found";
                        _warnings.Add(warning);
                        SeriLogModel.AddWarning(warning);
                    }
                    else
                    {
                        try
                        {
                            pl.DoseValuePresentation = VMS.TPS.Common.Model.Types.DoseValuePresentation.Absolute;
                            generatedEclipseStructure.ConvertDoseLevelToStructure(pl.Dose, new VMS.TPS.Common.Model.Types.DoseValue(convertDoseInstruction.DoseLevel, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.cGy));
                            SeriLogModel.AddLog($"{genStructure.StructureId} has been converted to the {convertDoseInstruction.DoseLevel} cGy isodose level...");
                        }
                        catch (Exception e)
                        {
                            completionStatus = InstructionCompletionStatus.Failed;
                            var warning = $"Convert dose instruction failed with exception: {e.Message}";
                            _warnings.Add(warning);
                            SeriLogModel.AddWarning(warning);
                        }
                    }
                }));
                return completionStatus;
            }

            private void CheckForHRConversion(TemplateStructure templateTarget, Structure eclipseTarget)
            {
                if (eclipseTarget.IsHighResolution && !generatedEclipseStructure.IsHighResolution)
                {
                    // log if this structure wasn't designated as needing to be high resolution
                    var warning = $"Generated structure {genStructure.StructureId} was not high-resolution, but must be converted because one of its inputs ({templateTarget.TemplateStructureId}) is.";
                    _warnings.Add(warning);
                    generatedEclipseStructure.ConvertToHighResolution();
                }
            }

            private async Task<InstructionCompletionStatus> ApplyInstruction(And andInstruction)
            {
                InstructionCompletionStatus completionStatus = InstructionCompletionStatus.Completed;
                bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                {
                    TemplateStructure TargetStructure = templateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, andInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                    Structure EclipseTarget = GetTempTargetStructure(S, TargetStructure);
                    CheckForHRConversion(TargetStructure, EclipseTarget);
                    if (TargetStructure == null)
                    {
                        var warning = $"Target of AND operation [{andInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is null, clearing generated structure...";
                        _warnings.Add(warning);
                        SeriLogModel.AddWarning(warning);
                        generatedEclipseStructure = ClearStructure(S, generatedEclipseStructure);
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                    }
                    else if (EclipseTarget.IsEmpty)
                    {
                        var warning = $"Target of AND operation [{andInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is empty, clearing generated structure...";
                        _warnings.Add(warning);
                        SeriLogModel.AddWarning(warning);
                        generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.And(EclipseTarget.SegmentVolume);
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                    }
                    else
                    {
                        SeriLogModel.AddWarning($"Performing AND between {andInstruction.TemplateStructureId} and {genStructure.StructureId}...");
                        generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.And(EclipseTarget.SegmentVolume);
                    }
                    S.RemoveStructure(EclipseTarget);
                }));
                return completionStatus;
            }
            private async Task<InstructionCompletionStatus> ApplyInstruction(Sub subInstruction)
            {
                InstructionCompletionStatus completionStatus = InstructionCompletionStatus.Completed;
                bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                {
                    TemplateStructure TargetStructure = templateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, subInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                    Structure EclipseTarget = GetTempTargetStructure(S, TargetStructure);
                    CheckForHRConversion(TargetStructure, EclipseTarget);
                    if (EclipseTarget == null)
                    {
                        var warning = $"Target of SUB operation [{subInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is null/empty, skipping instruction...";
                        _warnings.Add(warning);
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                        return;
                    }
                    else
                    {
                        if (EclipseTarget.IsEmpty)
                        {
                            var warning = $"Target of SUB operation [{subInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is empty, skipping instruction...";
                            _warnings.Add(warning);
                            completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                            return;
                        }
                        else
                        {
                            SeriLogModel.AddWarning($"Performing SUB of {subInstruction.TemplateStructureId} from {genStructure.StructureId}...");
                            generatedEclipseStructure = ClearStructure(S, generatedEclipseStructure);
                        }
                        S.RemoveStructure(EclipseTarget);
                    }
                }));
                return completionStatus;
            }
            private async Task<InstructionCompletionStatus> ApplyInstruction(SubFrom subFromInstruction)
            {
                InstructionCompletionStatus completionStatus = InstructionCompletionStatus.Completed;
                bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                {
                    TemplateStructure TargetStructure = templateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, subFromInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                    Structure EclipseTarget = GetTempTargetStructure(S, TargetStructure);
                    CheckForHRConversion(TargetStructure, EclipseTarget);
                    if (EclipseTarget == null)
                    {
                        var warning = $"Target of SUBFROM operation [{subFromInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is null/empty, aborting structure creation...";
                        _warnings.Add(warning);
                        completionStatus = InstructionCompletionStatus.Failed;
                        return;
                    }
                    else
                    {
                        if (EclipseTarget.IsEmpty)
                        {
                            var warning = $"Target of SUBFROM operation [{subFromInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is empty, aborting structure creation...";
                            completionStatus = InstructionCompletionStatus.Failed;
                            _warnings.Add(warning);
                        }
                        else
                        {
                            SeriLogModel.AddWarning($"Performing SUBFROM of {genStructure.StructureId} from {subFromInstruction.TemplateStructureId}...");
                            generatedEclipseStructure = ClearStructure(S, generatedEclipseStructure);
                        }
                        S.RemoveStructure(EclipseTarget);
                    }
                }));
                return completionStatus;
            }

            private Structure ClearStructure(StructureSet ss, Structure generatedEclipseStructure)
            {
                var structureId = generatedEclipseStructure.Id;
                var dicomType = generatedEclipseStructure.DicomType;
                var color = generatedEclipseStructure.Color;
                if (generatedEclipseStructure.DicomType == "")
                {
                    SeriLogModel.AddWarning($"Clearing structure {generatedEclipseStructure.Id} by segment as dicom type is null...");
                    generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.Sub(generatedEclipseStructure.SegmentVolume.Margin(5));
                    return generatedEclipseStructure; // workaround as structures with type "" cannot be removed
                }
                else
                {
                    ss.RemoveStructure(generatedEclipseStructure);
                    var newStructure = ss.AddStructure(dicomType, structureId);
                    newStructure.Color = color;
                    return newStructure;
                }
            }

            private async Task<InstructionCompletionStatus> ApplyInstruction(Or orInstruction)
            {
                InstructionCompletionStatus completionStatus = InstructionCompletionStatus.Completed;
                bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                {
                    TemplateStructure TargetStructure = templateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, orInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                    Structure EclipseTarget = GetTempTargetStructure(S, TargetStructure);
                    CheckForHRConversion(TargetStructure, EclipseTarget);
                    if (EclipseTarget == null)
                    {
                        var warning = $"Target of OR operation [{orInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is null/empty, skipping instruction...";
                        completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                        _warnings.Add(warning);
                        return;
                    }
                    else
                    {
                        if (EclipseTarget.IsEmpty)
                        {
                            var warning = $"Target of OR operation [{orInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is empty, skipping instruction...";
                            completionStatus = InstructionCompletionStatus.CompletedWithWarning;
                            _warnings.Add(warning);
                            return;
                        }
                        else
                        {
                            SeriLogModel.AddWarning($"Performing OR between {orInstruction.TemplateStructureId} and {genStructure.StructureId}...");
                            generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.Or(EclipseTarget.SegmentVolume);
                        }
                        S.RemoveStructure(EclipseTarget);
                    }
                }));
                return completionStatus;
            }

            private async Task<InstructionCompletionStatus> ApplyInstruction(ConvertResolution resolutionInstruction)
            {
                InstructionCompletionStatus completionStatus = InstructionCompletionStatus.Completed;
                bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                {
                    if (generatedEclipseStructure != null && !generatedEclipseStructure.IsHighResolution)
                    {
                        SeriLogModel.AddWarning($"Converting {genStructure.StructureId} to high resolution...");
                        generatedEclipseStructure.ConvertToHighResolution();
                    }
                }));
                return completionStatus;
            }
            internal async Task GenerateStructure()
            {
                try
                {
                    _warnings.Clear();
                    bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                    {
                        generatedEclipseStructure = S.Structures.FirstOrDefault(x => string.Equals(x.Id, genStructure.StructureId, StringComparison.OrdinalIgnoreCase));
                        if (generatedEclipseStructure == null)
                        {
                            DICOMTypes DT = 0;
                            Enum.TryParse(genStructure.DicomType.ToUpper(), out DT);
                            bool validNewStructure = S.CanAddStructure(DT.ToString(), genStructure.StructureId);
                            if (validNewStructure)
                            {
                                generatedEclipseStructure = S.AddStructure(DT.ToString(), genStructure.StructureId);
                            }
                            else
                            {
                                _warnings.Add($"Unable to create structure {genStructure.StructureId}...");
                                throw new Exception($"Unable to create structure {genStructure.StructureId}...");
                            }
                        }
                        else
                        {
                            SeriLogModel.AddWarning($"Structure {genStructure.StructureId} already exists, overwriting...");
                            generatedEclipseStructure = ClearStructure(S, generatedEclipseStructure);
                        }
                        SetStructureColor(generatedEclipseStructure, genStructure.StructureColor);

                    }));
                    InstructionCompletionStatus status = InstructionCompletionStatus.Pending;
                    foreach (var inst in genStructure.Instructions.Items)
                    {
                        switch (inst)
                        {
                            case ConvertResolution resolutionInstruction:
                                if (resolutionInstruction != null)
                                    status = await ApplyInstruction(resolutionInstruction);
                                break;
                            case Margin marginInstruction:
                                if (marginInstruction != null)
                                    status = await ApplyInstruction(marginInstruction);
                                break;
                            case AsymmetricMargin asymmMarginInstruction:
                                if (asymmMarginInstruction != null)
                                    status = await ApplyInstruction(asymmMarginInstruction);
                                break;
                            case AsymmetricCrop asymmCropInstruction:
                                if (asymmCropInstruction != null)
                                    status = await ApplyInstruction(asymmCropInstruction);
                                break;
                            case Crop cropInstruction:
                                if (cropInstruction != null)
                                    status = await ApplyInstruction(cropInstruction);
                                break;
                            case And andInstruction:
                                if (andInstruction != null)
                                    status = await ApplyInstruction(andInstruction);
                                break;
                            case Or orInstruction:
                                if (orInstruction != null)
                                    status = await ApplyInstruction(orInstruction);
                                break;
                            case Sub subInstruction:
                                if (subInstruction != null)
                                    status = await ApplyInstruction(subInstruction);
                                break;
                            case SubFrom subFromInstruction:
                                if (subFromInstruction != null)
                                    status = await ApplyInstruction(subFromInstruction);
                                break;
                            case ConvertDose convertDoseInstruction:
                                if (convertDoseInstruction != null)
                                    status = await ApplyInstruction(convertDoseInstruction);
                                break;
                        }
                        if (status == InstructionCompletionStatus.Failed)
                        {
                            break;
                        }
                    }
                    if (status == InstructionCompletionStatus.Failed)
                    {
                        Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                        {
                            SeriLogUI.AddLog($"Clearing and aborting structure {genStructure.StructureId} due to failed instruction...");
                            generatedEclipseStructure = ClearStructure(S, generatedEclipseStructure);
                        }));

                    }
                    await ConvertToHighResolution();

                }
                catch (Exception ex)
                {
                    SeriLogModel.AddError($"Exce[topm reached generating structure {genStructure.StructureId}", ex);
                    throw new Exception($"Exception reached in GenerateStructure, please contact your OptiMate admnistrator.");
                }
            }

            private void SetStructureColor(Structure generatedEclipseStructure, string structureColor)
            {
                try
                {
                    if (string.IsNullOrEmpty(structureColor))
                    {
                        SeriLogModel.AddWarning($"No color specified for structure {generatedEclipseStructure.Id}, using default color...");
                        return;
                    }
                    var colArray = structureColor.Split(',');
                    if (colArray.Length == 3)
                    {
                        byte r = Convert.ToByte(colArray[0]);
                        byte g = Convert.ToByte(colArray[1]);
                        byte b = Convert.ToByte(colArray[2]);
                        generatedEclipseStructure.Color = System.Windows.Media.Color.FromRgb(r, g, b);
                        SeriLogModel.AddLog($"Setting color for structure {generatedEclipseStructure.Id} to R:{r},G:{g},B:{b}...");
                    }
                    else
                    {
                        SeriLogModel.AddWarning($"Invalid color format for structure {generatedEclipseStructure.Id}, using default color...");
                    }
                }
                catch (Exception ex)
                {
                    SeriLogModel.AddError($"Exception reached in SetStructureColor for {generatedEclipseStructure.Id}", ex);
                    throw new Exception($"Exception reached in SetStructureColor for {generatedEclipseStructure.Id}, please contact your OptiMate admnistrator.");
                }
            }

            private async Task ConvertToHighResolution()
            {
                bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
                {
                    if (!generatedEclipseStructure.IsHighResolution)
                    {
                        generatedEclipseStructure.ConvertToHighResolution();
                    }
                }));
            }

            internal IEnumerable<string> GetCompletionWarnings()
            {
                return _warnings;
            }
        }
    }
}
