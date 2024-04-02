using OptiMate;
using OptiMate.ViewModels;
using Prism.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using VMS.TPS.Common.Model.API;
using OptiMate.Logging;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace OptiMate.Models
{

    
    public partial class MainModel
    {

        private string _publicTemplatePath;
        private string _usersTemplatePath;
        public List<string> ValidationErrors { get; private set; } = new List<string>();
        public string LogPath { get; private set; }

        public string CurrentUser
        {
            get
            {
                if (_ew != null)
                    return _ew.UserId;
                else
                    return "NotInitialized";
            }
        }
        private EsapiWorker _ew = null;
        private IEventAggregator _ea = null;
        private OptiMateTemplate _template = null;

        private List<string> _availableStructureIds = new List<string>();
        public string StructureSetId { get; private set; }
        public MainModel(EsapiWorker ew)
        {
            _ew = ew;
        }

        public async void Initialize()
        {
            InitializePaths();
            SeriLogModel.Initialize(LogPath, _ew.UserId);
            await InitializeEclipseObjects();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            _ea.GetEvent<NewTemplateNameSpecified>().Subscribe(OnNewTemplateNameSpecified);
            _ea.GetEvent<ModelInitializedEvent>().Publish();
        }


        private void InitializePaths()
        {

            var AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            XmlSerializer Ser = new XmlSerializer(typeof(OptiMateConfiguration));
            var configFile = Path.Combine(AssemblyPath, "OptiMateConfig.xml");
            try
            {
                using (StreamReader config = new StreamReader(configFile))
                {
                    var OMConfig = (OptiMateConfiguration)Ser.Deserialize(config);
                    _publicTemplatePath = OMConfig.Paths.PublicTemplatePath;
                    _usersTemplatePath = OMConfig.Paths.UsersTemplatePath;
                    LogPath = OMConfig.Paths.LogPath;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Unable to read configuration file {0}\r\n\r\nDetails: {1}", configFile, ex.InnerException);
                MessageBox.Show(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        private async Task InitializeEclipseObjects()
        {
            bool Done = await Task.Run(() => _ew.AsyncRunStructureContext((p, ss, ui) =>
            {
                p.BeginModifications();
                _availableStructureIds = ss.Structures.Select(x => x.Id).ToList();
                StructureSetId = ss.Id;
                // one time initialization
                isStructureEmpty = new Dictionary<string, bool>();
                foreach (var s in ss.Structures)
                {
                    isStructureEmpty.Add(s.Id, s.IsEmpty);
                }
            }));
        }

        public List<string> GetEclipseStructureIds(string thisGenStructureId = "")
        {
            return new List<string>(_availableStructureIds);
        }

        public List<string> GetGeneratedStructureIds()
        {
            return new List<string>(_template.GeneratedStructures.Select(x => x.StructureId));
        }
        public List<string> GetAvailableTemplateTargetIds(string thisGenStructureId = "")
        {
            var availableStructures = _template.TemplateStructures.Select(x => x.TemplateStructureId).ToList();
            availableStructures.AddRange(_template.GeneratedStructures.Take(_template.GeneratedStructures.Select(x => x.StructureId).ToList().IndexOf(thisGenStructureId)).Select(x => x.StructureId));
            return availableStructures;
        }

        internal async Task<List<string>> GenerateStructures()
        {
            int index = 0;
            List<string> completionWarnings = new List<string>();
            foreach (var genStructure in _template.GeneratedStructures)
            {
                _ea.GetEvent<StructureGeneratingEvent>().Publish(new StructureGeneratingEventInfo { Structure = genStructure, IndexInQueue = index, TotalToGenerate = _template.GeneratedStructures.Count() });
                List<TemplateStructure> augmentedList = GetAugmentedTemplateStructures(genStructure.StructureId);
                var structureModel = new GenerateStructureModel(_ew, _ea, genStructure, augmentedList);
                await structureModel.GenerateStructure();
                completionWarnings.AddRange(structureModel.GetCompletionWarnings());
                _ea.GetEvent<StructureGeneratedEvent>().Publish(new StructureGeneratedEventInfo { Structure = genStructure, IndexInQueue = index++, TotalToGenerate = _template.GeneratedStructures.Count() });
            }
           return completionWarnings;
        }

        private List<TemplateStructure> GetAugmentedTemplateStructures(string structureId)
        {
            var augmentedList = _template.TemplateStructures.ToList();
            foreach (var genStructure in _template.GeneratedStructures.Take(_template.GeneratedStructures.Select(x => x.StructureId).ToList().IndexOf(structureId)))
            {
                augmentedList.Add(new TemplateStructure() { TemplateStructureId = genStructure.StructureId, EclipseStructureId = genStructure.StructureId });
            }
            return augmentedList;
        }

        public List<string> GetTemplateStructureIds()
        {
            return new List<string>(_template.TemplateStructures.Select(x => x.TemplateStructureId));
        }
        internal void SetEventAggregator(IEventAggregator ea)
        {
            _ea = ea;
        }

        internal OptiMateTemplate LoadTemplate(TemplatePointer value)
        {
            XmlSerializer Ser = new XmlSerializer(typeof(OptiMateTemplate));
            if (value != null)
            {
                try
                {
                    using (StreamReader templateData = new StreamReader(value.TemplatePath))
                    {
                        _template = (OptiMateTemplate)Ser.Deserialize(templateData);
                    }
                }
                catch (Exception ex)
                {
                    string error = $"Unable to read template file: {value.TemplatePath}";
                    SeriLogModel.AddError(error, ex);
                    ValidationErrors.Add(error);
                    return null;
                }
                try
                {
                    if (ValidateTemplate())
                    {
                        SeriLogModel.AddLog($"Template [{value.TemplateDisplayName}] validated");
                        return _template;
                    }
                    else
                    {
                        SeriLogModel.AddLog($"Template [{value.TemplateDisplayName}] invalid");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    string error = $"Unable to validate template file: {value.TemplatePath}";
                    SeriLogModel.AddError(error, ex);
                    ValidationErrors.Add(error);
                    return null;
                }
            }
            else
            {
                SeriLogModel.AddLog($"Attempt to load null TemplatePointer");
                throw new Exception("Attempt to load null TemplatePointer");
            }
        }

        private bool ValidateTemplate()
        {
            bool valid = true;
            ValidationErrors.Clear();
            foreach (var genStructure in _template.GeneratedStructures)
            {
                if (!IsValidEclipseStructureId(genStructure.StructureId))
                {
                    valid = false;
                    ValidationErrors.Add($"{genStructure.StructureId} is not a valid Eclipse struct");
                }
                foreach (var instruction in genStructure.Instructions.Items)
                {
                    switch (instruction)
                    {
                        case Or inst:
                            if (!IsValidReferenceStructure(genStructure.StructureId, inst.TemplateStructureId))
                            {
                                valid = false;
                                ValidationErrors.Add($"Generated structure {genStructure.StructureId} references a Template Structure that could not be found: {inst.TemplateStructureId}");
                            }
                            break;
                        case And inst:
                            if (!IsValidReferenceStructure(genStructure.StructureId, inst.TemplateStructureId))
                            {
                                valid = false;
                                ValidationErrors.Add($"Generated structure {genStructure.StructureId} references a Template Structure that could not be found: {inst.TemplateStructureId}");
                            }
                            break;
                        case Sub inst:
                            if (!IsValidReferenceStructure(genStructure.StructureId, inst.TemplateStructureId))
                            {
                                valid = false;
                                ValidationErrors.Add($"Generated structure {genStructure.StructureId} references a Template Structure that could not be found: {inst.TemplateStructureId}");
                            }
                            break;
                        case Crop inst:
                            if (!IsValidReferenceStructure(genStructure.StructureId, inst.TemplateStructureId))
                            {
                                valid = false;
                                ValidationErrors.Add($"Generated structure {genStructure.StructureId} references a Template Structure that could not be found: {inst.TemplateStructureId}");
                            }
                            break;
                        case SubFrom inst:
                            if (!IsValidReferenceStructure(genStructure.StructureId, inst.TemplateStructureId))
                            {
                                valid = false;
                                ValidationErrors.Add($"Generated structure {genStructure.StructureId} references a Template Structure that could not be found: {inst.TemplateStructureId}");
                            }
                            break;
                    }
                }
            }
            if (!AreTemplateStructuresUnique())
            {
                valid = false;
                ValidationErrors.Add("Template structure names are not unique.");
            }
            return valid;
        }

        private bool AreTemplateStructuresUnique()
        {
            return _template.TemplateStructures.Select(x => x.TemplateStructureId).Distinct().Count() == _template.TemplateStructures.Count();
        }

        private bool IsValidEclipseStructureId(string eclipseStructureId)
        {
            return (eclipseStructureId.Count() > 0 && eclipseStructureId.Count() <= 16);
        }
      
        private bool IsValidReferenceStructure(string genStructureId, string templateStructureId)
        {
            var augmentedList = GetAugmentedTemplateStructures(genStructureId);
            if (augmentedList.Any(x => string.Equals(x.TemplateStructureId, templateStructureId, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string getNewTemplateStructureId()
        {
            int count = 1;
            string baseId = "NewTS";
            while (!IsNewTemplateStructureIdValid(baseId + count))
            {
                count++;
            }
            return baseId + count;
        }
        private string getNewGenStructureId()
        {
            int count = 1;
            string baseId = "NewGS";
            while (!IsNewGeneratedStructureIdValid(baseId + count))
            {
                count++;
            }
            return baseId + count;
        }
        internal TemplateStructure AddTemplateStructure()
        {
            var newTemplateStructure = new TemplateStructure()
            {
                TemplateStructureId = getNewTemplateStructureId(),
                Alias = new string[] {}
            };
            var templateList = _template.TemplateStructures.ToList();
            templateList.Add(newTemplateStructure);
            _template.TemplateStructures = templateList.ToArray();
            _ea.GetEvent<NewTemplateStructureEvent>().Publish(new NewTemplateStructureEventInfo { Structure = newTemplateStructure });
            return newTemplateStructure;
        }

        internal void RemoveTemplateStructure(string templateStructureId)
        {
            var templateStructures = _template.TemplateStructures.ToList();
            var removedStructure = templateStructures.FirstOrDefault(x => x.TemplateStructureId == templateStructureId);
            RemoveAllReferencesToTemplateStructure(removedStructure);
            templateStructures.Remove(removedStructure);
            _template.TemplateStructures = templateStructures.ToArray();
            _ea.GetEvent<RemovedTemplateStructureEvent>().Publish(new RemovedTemplateStructureEventInfo { RemovedStructure = removedStructure });
        }

        private void RemoveAllReferencesToTemplateStructure(TemplateStructure removedStructure)
        {
            foreach (GeneratedStructure genStructure in _template.GeneratedStructures)
            {
                var instructionItems = genStructure.Instructions.Items.ToList();
                foreach (Instruction instruction in instructionItems)
                {
                    switch (instruction)
                    {
                        case Or or:
                            if (or.TemplateStructureId == removedStructure.TemplateStructureId)
                            {
                                RemoveInstruction(genStructure.StructureId, instruction);
                            }
                            break;
                        case And and:
                            if (and.TemplateStructureId == removedStructure.TemplateStructureId)
                            {
                                RemoveInstruction(genStructure.StructureId, instruction);
                            }
                            break;
                        case Crop crop:
                            if (crop.TemplateStructureId == removedStructure.TemplateStructureId)
                            {
                                RemoveInstruction(genStructure.StructureId, instruction);
                            }
                            break;
                        case SubFrom subfrom:
                            if (subfrom.TemplateStructureId == removedStructure.TemplateStructureId)
                            {
                                RemoveInstruction(genStructure.StructureId, instruction);
                            }
                            break;
                    }
                }
            }
        }

        internal void RemoveInstruction(string structureId, Instruction instruction)
        {
            var genStructure = _template.GeneratedStructures.FirstOrDefault(x => x.StructureId == structureId);
            var instructionItems = genStructure.Instructions.Items.ToList();
            instructionItems.Remove(instruction);
            _template.GeneratedStructures.FirstOrDefault(x => x.StructureId == structureId).Instructions.Items = instructionItems.ToArray();
            _ea.GetEvent<RemovedInstructionEvent>().Publish(new InstructionRemovedEventInfo { Structure = genStructure, RemovedInstruction = instruction });
        }

        internal int GetInstructionNumber(string structureId, Instruction instruction)
        {
            var genStructure = _template.GeneratedStructures.FirstOrDefault(x => x.StructureId == structureId);
            var temp = genStructure.Instructions.Items.ToList().IndexOf(instruction);
            return temp;
        }

        internal Instruction AddInstruction(GeneratedStructure generatedStructure, OperatorTypes selectedOperator, int index)
        {
            var newInstruction = CreateInstruction(selectedOperator);
            var instructionItems = generatedStructure.Instructions.Items.ToList();
            instructionItems.Insert(index, newInstruction);
            generatedStructure.Instructions.Items = instructionItems.ToArray();
            return newInstruction;
        }

        internal Instruction ReplaceInstruction(GeneratedStructure parentGeneratedStructure, Instruction instruction, OperatorTypes selectedOperator)
        {
            var genStructure = _template.GeneratedStructures.FirstOrDefault(x => x.StructureId == parentGeneratedStructure.StructureId);
            var instructionItems = genStructure.Instructions.Items.ToList();
            var index = instructionItems.IndexOf(instruction);
            instructionItems.Remove(instruction);
            var newInstruction = CreateInstruction(selectedOperator);
            instructionItems.Insert(index, newInstruction);
            genStructure.Instructions.Items = instructionItems.ToArray();
            return newInstruction;
        }

        private Instruction CreateInstruction(OperatorTypes selectedOperator)
        {

            switch (selectedOperator)
            {
                case OperatorTypes.and:
                    return new And();
                case OperatorTypes.or:
                    return new Or();
                case OperatorTypes.asymmetricMargin:
                    return new AsymmetricMargin();
                case OperatorTypes.sub:
                    return new Sub();
                case OperatorTypes.crop:
                    return new Crop();
                case OperatorTypes.margin:
                    return new Margin();
                case OperatorTypes.convertDose:
                    return new ConvertDose();
                case OperatorTypes.subfrom:
                    return new SubFrom();
                case OperatorTypes.convertResolution:
                    return new ConvertResolution();
                case OperatorTypes.asymmetricCrop:
                    return new AsymmetricCrop();
                default:
                    SeriLogModel.AddLog("Adding new default instruction (Or)...");
                    return new Or();
            }
        }

        private Dictionary<string, bool> isStructureEmpty = null;
        internal bool IsEmpty(string eclipseStructureId)
        {

            if (isStructureEmpty.ContainsKey(eclipseStructureId))
            {
                return isStructureEmpty[eclipseStructureId];
            }
            else
            {
                return true;
            }
        }

        internal GeneratedStructure AddGeneratedStructure()
        {
            var newGeneratedStructure = new GeneratedStructure()
            {
                StructureId = getNewGenStructureId(),
                Instructions = new GeneratedStructureInstructions() { Items = new Instruction[] { new Or() } }
            };
            var genStructures = _template.GeneratedStructures.ToList();
            genStructures.Add(newGeneratedStructure);
            _template.GeneratedStructures = genStructures.ToArray();
            _ea.GetEvent<NewGeneratedStructureEvent>().Publish(new NewGeneratedStructureEventInfo { NewStructure = newGeneratedStructure });
            return newGeneratedStructure;
        }

        internal void RemoveGeneratedStructure(string structureId)
        {
            var genStructures = _template.GeneratedStructures.ToList();
            genStructures.Remove(genStructures.FirstOrDefault(x => x.StructureId == structureId));
            _template.GeneratedStructures = genStructures.ToArray();
            _ea.GetEvent<RemovedGeneratedStructureEvent>().Publish(new RemovedGeneratedStructureEventInfo { RemovedStructureId = structureId });
        }

        private bool IsGeneratedStructureIdValid(string value)
        {
            if (value.Length <= 16 
                && value.Length > 0 
                && _template.GeneratedStructures.Select(x => x.StructureId).Count(y=> string.Equals(y, value, StringComparison.OrdinalIgnoreCase)) <= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        internal bool IsNewGeneratedStructureIdValid(string value)
        {
            if (value.Length <= 16
                && value.Length > 0
                && !_template.GeneratedStructures.Select(x => x.StructureId).Contains(value, StringComparer.OrdinalIgnoreCase)
                && !_template.TemplateStructures.Select(x => x.TemplateStructureId).Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool IsNewTemplateStructureIdValid(string value)
        {
            if (value.Length <= 16
                && value.Length > 0
                && !_template.GeneratedStructures.Select(x => x.StructureId).Contains(value, StringComparer.OrdinalIgnoreCase)
                && !_template.TemplateStructures.Select(x => x.TemplateStructureId).Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal IEnumerable<TemplatePointer> GetTemplates()
        {
            var TemplatePointers = new List<TemplatePointer>();
            try
            {
                XmlSerializer Ser = new XmlSerializer(typeof(OptiMateTemplate));

                foreach (var file in Directory.GetFiles(_publicTemplatePath, "*.xml"))
                {
                    using (StreamReader protocol = new StreamReader(file))
                    {
                        try
                        {
                            var OMProtocol = (OptiMateTemplate)Ser.Deserialize(protocol);
                            TemplatePointers.Add(new TemplatePointer() { TemplateDisplayName = OMProtocol.TemplateDisplayName, TemplatePath = file });
                        }
                        catch (Exception ex)
                        {
                            SeriLogModel.AddLog(string.Format("Unable to read protocol file: {0}\r\n\r\nDetails: {1}", file, ex.InnerException));
                            MessageBox.Show(string.Format("Unable to read protocol file {0}\r\n\r\nDetails: {1}", file, ex.InnerException));

                        }
                    }
                }
                foreach (var file in Directory.GetFiles(GetUserTemplatePath()))
                {
                    using (StreamReader protocol = new StreamReader(file))
                    {
                        try
                        {
                            var OMProtocol = (OptiMateTemplate)Ser.Deserialize(protocol);
                            TemplatePointers.Add(new TemplatePointer() { TemplateDisplayName = $"(p) {OMProtocol.TemplateDisplayName}", TemplatePath = file });
                        }
                        catch (Exception ex)
                        {
                            SeriLogModel.AddLog(string.Format("Unable to read protocol file: {0}\r\n\r\nDetails: {1}", file, ex.InnerException));
                            MessageBox.Show(string.Format("Unable to read protocol file {0}\r\n\r\nDetails: {1}", file, ex.InnerException));

                        }
                    }
                }
                return TemplatePointers;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                return null;
            }
        }

        internal string GetUserTemplatePath()
        {
            string userPath = Path.Combine(_usersTemplatePath, Helpers.MakeStringPathSafe(CurrentUser));
            if (!Directory.Exists(userPath))
            {
                Directory.CreateDirectory(userPath);
            }
            return Path.Combine(_usersTemplatePath, Helpers.MakeStringPathSafe(CurrentUser));
        }

        internal SaveToPersonalResult SaveTemplateToPersonal(string newTemplateId)
        {
            var dir = GetUserTemplatePath();
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = dir;
            saveFileDialog.FileName = Helpers.MakeStringPathSafe(newTemplateId + ".xml");
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    writeTemplate(newTemplateId, saveFileDialog.FileName);
                    _ea.GetEvent<TemplateSavedEvent>().Publish();
                    return SaveToPersonalResult.Success;
                }
                catch (Exception ex)
                {
                    SeriLogModel.AddError("Failed to save user's template", ex);
                    return SaveToPersonalResult.Failure;
                }
            }
            else
            {
                return SaveToPersonalResult.Cancelled;
            }
        }

        private void OnNewTemplateNameSpecified(string templateName)
        {
            SaveTemplateToPersonal(templateName);
        }

        private bool writeTemplate(string newTemplateId, string fileName)
        {
            var originalTemplateName = _template.TemplateDisplayName;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(OptiMateTemplate));
                using (TextWriter writer = new StreamWriter(fileName))
                {
                    _template.TemplateDisplayName = newTemplateId;
                    ser.Serialize(writer, _template);
                    _template.TemplateDisplayName = originalTemplateName;
                }
                return true;
            }
            catch (Exception ex)
            {
                _template.TemplateDisplayName = originalTemplateName;
                SeriLogModel.AddError("Failed to save user's template", ex);
                return false;
            }
        }

        internal void ReorderTemplateStructures(int a, int b)
        {
            var TemplateStructures = new ObservableCollection<TemplateStructure>(_template.TemplateStructures);
            TemplateStructures.Move(a, b);
            _template.TemplateStructures = TemplateStructures.ToArray();
        }

        internal void ReorderGeneratedStructures(int a, int b)
        {
            var GeneratedStructures = new ObservableCollection<GeneratedStructure>(_template.GeneratedStructures);
            GeneratedStructures.Move(a, b);
            _template.GeneratedStructures = GeneratedStructures.ToArray();
            _ea.GetEvent<GeneratedStructureOrderChangedEvent>().Publish();
        }

        internal void ReorderTemplateStructureAliases(string templateStructureId, int a, int b)
        {
            var templateStructure = _template.TemplateStructures.FirstOrDefault(x => x.TemplateStructureId == templateStructureId);
            if (templateStructure == null)
            {
                SeriLogModel.AddError("Failed to reorder template structure aliases", new Exception("Template structure not found"));
            }
            else
            {
                var aliases = new ObservableCollection<string>(templateStructure.Alias);
                aliases.Move(a, b);
                templateStructure.Alias = aliases.ToArray();
            }
        }

        internal void AddNewTemplateStructureAlias(string templateStructureId, string newAlias)
        {
            var ts = _template.TemplateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, templateStructureId, StringComparison.OrdinalIgnoreCase));
            if (ts != null)
            {
                if (ts.Alias == null)
                    ts.Alias = new string[] { newAlias };
                else if (!ts.Alias.Contains(newAlias, StringComparer.OrdinalIgnoreCase))
                    ts.Alias = ts.Alias.Concat(new string[] { newAlias }).ToArray();
            }
        }

        internal bool IsAliasValid(string templateStructureId, string value)
        {
            var ts = _template.TemplateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, templateStructureId, StringComparison.OrdinalIgnoreCase));
            if (ts != null)
            {
                if (ts.Alias != null && ts.Alias.Contains(value, StringComparer.OrdinalIgnoreCase))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
                return false;
        }

        internal void RemoveTemplateStructureAlias(string templateStructureId, string alias)
        {
            var ts = _template.TemplateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, templateStructureId, StringComparison.OrdinalIgnoreCase));
            if (ts != null)
            {
                var aliases = ts.Alias.ToList();
                aliases.Remove(alias);
                ts.Alias = aliases.ToArray();
            }
        }

        internal List<string> GetTemplateStructureAliases(string templateStructureId)
        {
            var ts = _template.TemplateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, templateStructureId, StringComparison.OrdinalIgnoreCase));
            if (ts != null)
            {
                return ts.Alias == null ? new List<string>() : ts.Alias.ToList();
            }
            else
            {
                return null;
            }
        }

        internal void RenameTemplateStructure(string templateStructureId, string value)
        {
            var ts = _template.TemplateStructures.FirstOrDefault(x => string.Equals(x.TemplateStructureId, templateStructureId, StringComparison.OrdinalIgnoreCase));
            if (ts != null && IsNewTemplateStructureIdValid(value))
            {
                var eventArgs = new TemplateStructureIdChangedEventInfo() { OldId = ts.TemplateStructureId, NewId = value };
                string oldId = ts.TemplateStructureId;
                ts.TemplateStructureId = value;
                _ea.GetEvent<TemplateStructureIdChangedEvent>().Publish(eventArgs);
            }
        }

        internal void RenameGeneratedStructure(string structureId, string newStructureId)
        {
            if (IsGeneratedStructureIdValid(newStructureId))
            {
                var gs = _template.GeneratedStructures.FirstOrDefault(x => string.Equals(x.StructureId, structureId, StringComparison.OrdinalIgnoreCase));
                if (gs != null)
                {
                    gs.StructureId = newStructureId;
                    _ea.GetEvent<GeneratedStructureIdChangedEvent>().Publish(new GeneratedStructureIdChangedEventInfo() { OldId = structureId, NewId = newStructureId });
                }
            }
        }
    }
}

