using Optimate;
using Optimate.ViewModels;
using OptiMate.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace OptiMate.Models
{

    public class GenerateStructureModel
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

        private Structure GetTargetStructure(StructureSet S, TemplateStructure Target)
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
                Helpers.SeriLog.AddLog(warning);
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
                if (generatedEclipseStructure.IsHighResolution)
                {
                    Temp.ConvertToHighResolution();
                }
                return Temp;
            }
        }

        private async Task ApplyInstruction(Copy copyInstruction)
        {
            bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
            {
                try
                {
                    string StructureToCopyId = templateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, copyInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase))?.EclipseStructureId;
                    if (string.IsNullOrEmpty(StructureToCopyId))
                    {
                        string warningMessage = string.Format($"Copy target for structure {genStructure.StructureId} is null, skipping structure...");
                        _warnings.Add(warningMessage);
                        Helpers.SeriLog.AddLog(warningMessage);
                        throw new Exception();
                    }
                    Structure BaseStructure = S.Structures.FirstOrDefault(x => string.Equals(x.Id, StructureToCopyId, StringComparison.OrdinalIgnoreCase));
                    if (BaseStructure == null)
                    {
                        string warningMessage = string.Format($"Attempt to create structure {genStructure.StructureId} failed as copy target {StructureToCopyId} does not exist in structure set, skipping structure...");
                        _warnings.Add(warningMessage);
                        Helpers.SeriLog.AddLog(warningMessage);
                        throw new Exception();
                    }
                    else if (BaseStructure.IsEmpty)
                    {
                        string warningMessage = string.Format($"Attempt to create structure {genStructure.StructureId} failed as copy target {StructureToCopyId} is empty, skipping structure...");
                        _warnings.Add(warningMessage);
                        Helpers.SeriLog.AddLog(warningMessage);
                        throw new Exception();
                    }
                    else
                    {
                        if (string.Equals(StructureToCopyId, generatedEclipseStructure.Id, StringComparison.OrdinalIgnoreCase)) // if OS is the same as the copy structure, then only need to check for high res conversion
                        {
                            if (genStructure.isHighResolution && !generatedEclipseStructure.IsHighResolution)
                            {
                                generatedEclipseStructure.ConvertToHighResolution();
                            }
                            return;
                        }
                        if (!generatedEclipseStructure.IsEmpty)
                        {
                            generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.And(generatedEclipseStructure.SegmentVolume.Not()); // clear structure;
                        }
                        if (BaseStructure.IsHighResolution && !generatedEclipseStructure.IsHighResolution)
                        {
                            generatedEclipseStructure.ConvertToHighResolution();
                            generatedEclipseStructure.SegmentVolume = BaseStructure.SegmentVolume;
                        }
                        else if (!BaseStructure.IsHighResolution && generatedEclipseStructure.IsHighResolution)
                        {
                            var HRTemp = S.Structures.FirstOrDefault(x => x.Id == TempStructureName);
                            if (HRTemp != null)
                                S.RemoveStructure(HRTemp);
                            HRTemp = S.AddStructure(genStructure.DicomType, TempStructureName);
                            HRTemp.SegmentVolume = BaseStructure.SegmentVolume;
                            HRTemp.ConvertToHighResolution();
                            generatedEclipseStructure.SegmentVolume = HRTemp.SegmentVolume;
                            S.RemoveStructure(HRTemp);
                            generatedEclipseStructure.SegmentVolume = BaseStructure.SegmentVolume;
                        }
                        else
                            generatedEclipseStructure.SegmentVolume = BaseStructure.SegmentVolume;
                    }
                }
                catch (Exception e)
                {
                    Helpers.SeriLog.AddError($"Error in ApplyInstruction for {genStructure.StructureId}", e);
                }

            }));
        }

        private async Task ApplyInstruction(Margin marginInstruction)
        {
            bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
            {
                double IsotropicMargin;
                if (!string.IsNullOrEmpty(marginInstruction.IsotropicMargin))
                {
                    if (!double.TryParse(marginInstruction.IsotropicMargin, out IsotropicMargin))
                    {
                        if (IsotropicMargin > -50 && IsotropicMargin < 50)
                            generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.Margin(IsotropicMargin);
                        else
                            throw new Exception($"Isotropic margin for {genStructure.StructureId} is invalid (value = {IsotropicMargin}), skipping structure...");
                        return;
                    }
                }
            }));
        }

        private async Task ApplyInstruction(AsymmetricMargin marginInstruction)
        {
            bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
            {
                double leftmargin = 0;
                double rightmargin = 0;
                double antmargin = 0;
                double postmargin = 0;
                double infmargin = 0;
                double supmargin = 0;
                if (!string.IsNullOrEmpty(marginInstruction.RightMargin))
                {
                    if (!double.TryParse(marginInstruction.RightMargin, out rightmargin))
                    {
                        string warning = "Right margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        Helpers.SeriLog.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                }
                if (!string.IsNullOrEmpty(marginInstruction.LeftMargin))
                {
                    if (!double.TryParse(marginInstruction.LeftMargin, out leftmargin))
                    {
                        string warning = "Left margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        Helpers.SeriLog.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                }
                if (!string.IsNullOrEmpty(marginInstruction.AntMargin))
                {
                    if (!double.TryParse(marginInstruction.AntMargin, out antmargin))
                    {
                        string warning = "Anterior margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        Helpers.SeriLog.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                }
                if (!string.IsNullOrEmpty(marginInstruction.PostMargin))
                {
                    if (!double.TryParse(marginInstruction.PostMargin, out postmargin))
                    {
                        string warning = "Posterior margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        Helpers.SeriLog.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                }
                if (!string.IsNullOrEmpty(marginInstruction.InfMargin))
                {
                    if (!double.TryParse(marginInstruction.InfMargin, out infmargin))
                    {
                        string warning = "Inferior margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        Helpers.SeriLog.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                }
                if (!string.IsNullOrEmpty(marginInstruction.SupMargin))
                {
                    if (!double.TryParse(marginInstruction.SupMargin, out supmargin))
                    {
                        string warning = "Superior margin for " + genStructure.StructureId + " is invalid, using default (0) margin...";
                        Helpers.SeriLog.AddWarning(warning);
                        _warnings.Add(warning);
                    }
                }
                var AAM = Helpers.OrientationInvariantMargins.getAxisAlignedMargins(S.Image.ImagingOrientation, rightmargin, antmargin, infmargin, leftmargin, postmargin, supmargin);
                generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.AsymmetricMargin(AAM);
            }));
        }
        private async Task ApplyInstruction(Crop cropInstruction)
        {
            bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
            {
                TemplateStructure TargetStructure = templateStructures.FirstOrDefault(x => string.Equals(x.EclipseStructureId, cropInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                Structure EclipseTarget = GetTargetStructure(S, TargetStructure);
                CheckForHRConversion(TargetStructure, EclipseTarget);
                if (TargetStructure == null)
                {
                    var warning = $"Target of CROP operation [{cropInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is null/empty, skipping instruction...";
                    _warnings.Add(warning);
                    return;
                }
                else if (EclipseTarget.IsEmpty)
                {
                    var warning = $"Target of CROP operation [{cropInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is empty, skipping instruction...";
                    _warnings.Add(warning);
                    return;
                }
                //Determine crop offset parameters
                double isotropicOffset = 0;
                double leftOffset = 0;
                double rightOffset = 0;
                double antOffset = 0;
                double postOffset = 0;
                double infOffset = 0;
                double supOffset = 0;
                var logMsg = $"Applying crop for {genStructure.StructureId} from {EclipseTarget.Id}";
                Helpers.SeriLog.AddLog(logMsg);
                if (!string.IsNullOrEmpty(cropInstruction.IsotropicOffset))
                {
                    if (double.TryParse(cropInstruction.IsotropicOffset, out isotropicOffset))
                    {
                        logMsg = $"Using isotropic crop margins for {genStructure.StructureId}...";
                        Helpers.SeriLog.AddLog(logMsg);
                        if (isotropicOffset > -50 && isotropicOffset < 50)
                        {
                            if (cropInstruction.InternalCrop)
                            {
                                generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.And(EclipseTarget.SegmentVolume.Margin(-isotropicOffset));
                            }
                            else
                            {
                                generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.Sub(EclipseTarget.SegmentVolume.Margin(isotropicOffset));
                            }
                            return;
                        }
                        else
                        {
                            var warning = $"Isotropic crop margin for {genStructure.StructureId} exceeds Eclipse limits, aborting...";
                            _warnings.Add(warning);
                        }
                        return;
                    }
                }
                else
                {
                    // Anisotropic crop margins
                    logMsg = $"Using anisotropic crop margins for {genStructure.StructureId}...";
                    Helpers.SeriLog.AddLog(logMsg);
                    if (!string.IsNullOrEmpty(cropInstruction.RightOffset))
                    {
                        if (!double.TryParse(cropInstruction.RightOffset, out rightOffset))
                        {
                            var warning = $"Right crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                            _warnings.Add(warning);
                        }
                    }
                    if (!string.IsNullOrEmpty(cropInstruction.LeftOffset))
                    {
                        if (!double.TryParse(cropInstruction.LeftOffset, out leftOffset))
                        {
                            var warning = $"Left crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                            _warnings.Add(warning);
                        }
                    }
                    if (!string.IsNullOrEmpty(cropInstruction.AntOffset))
                    {
                        if (!double.TryParse(cropInstruction.AntOffset, out antOffset))
                        {
                            var warning = $"Anterior crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                            _warnings.Add(warning);
                        }
                    }
                    if (!string.IsNullOrEmpty(cropInstruction.PostOffset))
                    {
                        if (!double.TryParse(cropInstruction.PostOffset, out postOffset))
                        {
                            var warning = $"Posterior crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                            _warnings.Add(warning);
                        }
                    }
                    if (!string.IsNullOrEmpty(cropInstruction.InfOffset))
                    {
                        if (!double.TryParse(cropInstruction.InfOffset, out infOffset))
                        {
                            var warning = $"Inferior crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                            _warnings.Add(warning);
                        }
                    }
                    if (!string.IsNullOrEmpty(cropInstruction.SupOffset))
                    {
                        if (!double.TryParse(cropInstruction.SupOffset, out supOffset))
                        {
                            var warning = $"Superior crop margin for {genStructure.StructureId} is invalid, using default (0) offset...";
                            _warnings.Add(warning);
                        }
                    }
                    var AAM = Helpers.OrientationInvariantMargins.getAxisAlignedMargins(S.Image.ImagingOrientation, rightOffset, antOffset, infOffset, leftOffset, postOffset, supOffset);
                    if (cropInstruction.InternalCrop)
                    {
                        generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.And(EclipseTarget.SegmentVolume.AsymmetricMargin(AAM));
                    }
                    else
                    {
                        generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.Sub(EclipseTarget.SegmentVolume.AsymmetricMargin(AAM));
                    }
                }
                logMsg = $"Applied crop margins for {genStructure.StructureId}...";
                Helpers.SeriLog.AddLog(logMsg);
                S.RemoveStructure(EclipseTarget);
            }));
        }

        private void CheckForHRConversion(TemplateStructure templateTarget, Structure eclipseTarget)
        {
            if (eclipseTarget.IsHighResolution && !generatedEclipseStructure.IsHighResolution)
            {
                if (!genStructure.isHighResolution) // 
                {
                    // log if this structure wasn't designated as needing to be high resolution
                    var warning = $"Generated structure {genStructure.StructureId} is not templated as high-resolution, but must be converted because one of its inputs ({templateTarget.TemplateStructureId}) is.";
                    _warnings.Add(warning);
                }
                generatedEclipseStructure.ConvertToHighResolution();
            }
        }

        private async Task ApplyInstruction(And andInstruction)
        {
            bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
            {
                TemplateStructure TargetStructure = templateStructures.FirstOrDefault(x => string.Equals(x.EclipseStructureId, andInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                Structure EclipseTarget = GetTargetStructure(S, TargetStructure);
                CheckForHRConversion(TargetStructure, EclipseTarget);
                if (TargetStructure == null)
                {
                    var warning = $"Target of AND operation [{andInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is null/empty, skipping instruction...";
                    _warnings.Add(warning);
                    return;
                }
                else if (EclipseTarget.IsEmpty)
                {
                    var warning = $"Target of AND operation [{andInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is empty, clearing generated structure...";
                    _warnings.Add(warning);
                }
                generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.And(EclipseTarget.SegmentVolume);
                S.RemoveStructure(EclipseTarget);
            }));
        }
        private async Task ApplyInstruction(Sub subInstruction)
        {
            bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
            {
                TemplateStructure TargetStructure = templateStructures.FirstOrDefault(x => string.Equals(x.EclipseStructureId, subInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                Structure EclipseTarget = GetTargetStructure(S, TargetStructure);
                CheckForHRConversion(TargetStructure, EclipseTarget);
                if (TargetStructure == null)
                {
                    var warning = $"Target of SUB operation [{subInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is null/empty, skipping instruction...";
                    _warnings.Add(warning);
                    return;
                }
                else if (EclipseTarget.IsEmpty)
                {
                    var warning = $"Target of SUB operation [{subInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is empty, skipping instruction...";
                    _warnings.Add(warning);
                    return;
                }
                generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.Sub(EclipseTarget.SegmentVolume);
                S.RemoveStructure(EclipseTarget);
            }));
        }
        private async Task ApplyInstruction(SubFrom subFromInstruction)
        {
            bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
            {
                TemplateStructure TargetStructure = templateStructures.FirstOrDefault(x => string.Equals(x.EclipseStructureId, subFromInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                Structure EclipseTarget = GetTargetStructure(S, TargetStructure);
                CheckForHRConversion(TargetStructure, EclipseTarget);
                if (TargetStructure == null)
                {
                    var warning = $"Target of AND operation [{subFromInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is null/empty, skipping instruction...";
                    _warnings.Add(warning);
                    return;
                }
                else if (EclipseTarget.IsEmpty)
                {
                    var warning = $"Target of AND operation [{subFromInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is empty, clearing generated structure...";
                    _warnings.Add(warning);
                }
                generatedEclipseStructure.SegmentVolume = EclipseTarget.SegmentVolume.Sub(generatedEclipseStructure.SegmentVolume);
                S.RemoveStructure(EclipseTarget);
            }));
        }

        private async Task ApplyInstruction(Or orInstruction)
        {
            bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
            {
                TemplateStructure TargetStructure = templateStructures.FirstOrDefault(x => string.Equals(x.EclipseStructureId, orInstruction.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                Structure EclipseTarget = GetTargetStructure(S, TargetStructure);
                CheckForHRConversion(TargetStructure, EclipseTarget);
                if (TargetStructure == null)
                {
                    var warning = $"Target of OR operation [{orInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is null/empty, skipping instruction...";
                    _warnings.Add(warning);
                    return;
                }
                else if (EclipseTarget.IsEmpty)
                {
                    var warning = $"Target of OR operation [{orInstruction.TemplateStructureId}] for structure {genStructure.StructureId} is empty, skipping instruction...";
                    _warnings.Add(warning);
                    return;
                }
                generatedEclipseStructure.SegmentVolume = generatedEclipseStructure.SegmentVolume.Or(EclipseTarget.SegmentVolume);
                S.RemoveStructure(EclipseTarget);
            }));
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
                            generatedEclipseStructure = S.AddStructure(DT.ToString(), genStructure.StructureId);
                        else
                        {
                            _warnings.Add($"Unable to create structure {genStructure.StructureId}...");
                            throw new Exception($"Unable to create structure {genStructure.StructureId}...");
                        }
                    }

                }));
                foreach (var inst in genStructure.Instructions.Items)
                {
                    Copy copyInstruction = inst as Copy;
                    if (copyInstruction != null)
                    {
                        await ApplyInstruction(copyInstruction);
                        continue;
                    }
                    Margin marginInstruction = inst as Margin;
                    if (marginInstruction != null)
                    {
                        await ApplyInstruction(marginInstruction);
                        continue;
                    }
                    AsymmetricMargin asymmMarginInstruction = inst as AsymmetricMargin;
                    if (asymmMarginInstruction != null)
                    {
                        await ApplyInstruction(asymmMarginInstruction);
                        continue;
                    }
                    Crop cropInstruction = inst as Crop;
                    if (cropInstruction != null)
                    {
                        await ApplyInstruction(cropInstruction);
                        continue;
                    }
                    And andInstruction = inst as And;
                    if (andInstruction != null)
                    {
                        await ApplyInstruction(andInstruction);
                        continue;
                    }
                    Or orInstruction = inst as Or;
                    if (orInstruction != null)
                    {
                        await ApplyInstruction(orInstruction);
                        continue;
                    }
                    Sub subInstruction = inst as Sub;
                    if (subInstruction != null)
                    {
                        await ApplyInstruction(subInstruction);
                        continue;
                    }
                    SubFrom subFromInstruction = inst as SubFrom;
                    if (subFromInstruction != null)
                    {
                        await ApplyInstruction(subFromInstruction);
                        continue;
                    }
                }
                await ConvertToHighResolution();

            }
            catch (Exception ex)
            {
                _warnings.Add($"Error generating structure {genStructure.StructureId}, aborting...");
            }
        }

        private async Task ConvertToHighResolution()
        {
            bool Done = await Task.Run(() => ew.AsyncRunStructureContext((p, S, ui) =>
            {
                if (genStructure.isHighResolution && !generatedEclipseStructure.IsHighResolution)
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
