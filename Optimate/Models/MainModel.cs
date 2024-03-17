using Optimate;
using Optimate.ViewModels;
using OptiMate.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using VMS.TPS.Common.Model.API;

namespace OptiMate.Models
{

    public struct StructureGeneratedEventInfo
    {
        public GeneratedStructure Structure;
        public int IndexInQueue;
        public int TotalToGenerate;
        public List<string> Warnings;
    }
    public struct InstructionRemovedEventInfo
    {
        public GeneratedStructure Structure;
        public Instruction RemovedInstruction;
    }
    public struct InstructionAddedEventInfo
    {
        public GeneratedStructure Structure;
        public Instruction AddedInstruction;
    }
    public struct NewTemplateStructureEventInfo
    {
        public TemplateStructure Structure;
    }
    public struct RemovedTemplateStructureEventInfo
    {
        public TemplateStructure RemovedStructure;
    }
    public struct RemovedGeneratedStructureEventInfo
    {
        public string RemovedStructureId;
    }
    public struct NewGeneratedStructureEventInfo
    {
        public GeneratedStructure NewStructure;
    }

    public class RemovedInstructionEvent : PubSubEvent<InstructionRemovedEventInfo> { }
    public class ModelInitializedEvent : PubSubEvent { }
    public class AddedInstructionEvent : PubSubEvent<InstructionAddedEventInfo> { }
    public class StructureGeneratedEvent : PubSubEvent<StructureGeneratedEventInfo> { }
    public class NewTemplateStructureEvent : PubSubEvent<NewTemplateStructureEventInfo> { }
    public class RemovedTemplateStructureEvent : PubSubEvent<RemovedTemplateStructureEventInfo> { }
    public class RemovedGeneratedStructureEvent : PubSubEvent<RemovedGeneratedStructureEventInfo> { }
    public class NewGeneratedStructureEvent : PubSubEvent<NewGeneratedStructureEventInfo> { }
    public class MainModel
    {
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
            _ea.GetEvent<ModelInitializedEvent>().Publish();
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

        internal async Task<(bool, List<string>)> GenerateStructures()
        {
            int index = 0;
            List<string> completionWarnings = new List<string>();
            foreach (var genStructure in _template.GeneratedStructures)
            {
                List<TemplateStructure> augmentedList = GetAugmentedTemplateStructures(genStructure.StructureId);
                var structureModel = new GenerateStructureModel(_ew, _ea, genStructure, augmentedList);
                await structureModel.GenerateStructure();
                completionWarnings.AddRange(structureModel.GetCompletionWarnings());
                _ea.GetEvent<StructureGeneratedEvent>().Publish(new StructureGeneratedEventInfo { Structure = genStructure, IndexInQueue = index++, TotalToGenerate = _template.GeneratedStructures.Count() });
            }
            if (completionWarnings.Count > 0)
            {
                return (true, completionWarnings);
            }
            else
            {
                return (false, completionWarnings);
            }
        }

        private List<TemplateStructure> GetAugmentedTemplateStructures(string structureId)
        {
            var augmentedList = _template.TemplateStructures.ToList();
            foreach (var genStructure in _template.GeneratedStructures.Take(_template.GeneratedStructures.Select(x => x.StructureId).ToList().IndexOf(structureId)))
            {
                augmentedList.Add(new TemplateStructure() { TemplateStructureId = genStructure.StructureId, EclipseStructureId = genStructure.StructureId});
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
                    Helpers.SeriLog.AddError(string.Format("Unable to read protocol file: {0}", value.TemplatePath, ex));
                    MessageBox.Show(string.Format("Unable to read/interpret protocol file {0}, see log for details.", value.TemplatePath));
                }

            }
            else
            {
                _template = null;
            }
            if (_template != null)
            {
                Helpers.SeriLog.AddLog($"Protocol [{value.TemplateDisplayName}] selected");
                return _template;
            }
            else return null;

        }

        private string getNewTemplateStructureId()
        {
            int count = 1;
            string baseId = "NewTS";
            while (!IsTemplateStructureIdValid(baseId + count))
            {
                count++;
            }
            return baseId + count;
        }
        private string getNewGenStructureId()
        {
            int count = 1;
            string baseId = "NewGS";
            while (!IsGeneratedStructureIdValid(baseId + count))
            {
                count++;
            }
            return baseId + count;
        }
        internal TemplateStructure AddTemplateStructure()
        {
            var newTemplateStructure = new TemplateStructure()
            {
                TemplateStructureId = getNewTemplateStructureId()
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
            templateStructures.Remove(removedStructure);
            _template.TemplateStructures = templateStructures.ToArray();
            _ea.GetEvent<RemovedTemplateStructureEvent>().Publish(new RemovedTemplateStructureEventInfo { RemovedStructure = removedStructure });
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
                default:
                    return new Instruction();
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
                Instructions = new GeneratedStructureInstructions() { Items = new Instruction[] {new Copy() } }
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

        internal bool IsGeneratedStructureIdValid(string value)
        {
            if (value.Length <=16 
                && value.Length>0 
                && !_template.GeneratedStructures.Select(x=>x.StructureId).Contains(value, StringComparer.OrdinalIgnoreCase)
                && !_template.TemplateStructures.Select(x=>x.TemplateStructureId).Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool IsTemplateStructureIdValid(string value)
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
    }
}

