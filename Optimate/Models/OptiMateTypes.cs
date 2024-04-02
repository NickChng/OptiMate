using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptiMate.Models
{

    public enum SaveToPersonalResult
    {
        Success,
        Failure,
        Cancelled
    }
    public enum InstructionCompletionStatus
    {
        Pending,
        Completed,
        CompletedWithWarning,
        Failed
    }
    public enum DICOMTypes
    {
        CONTROL,
        AVOIDANCE,
        CAVITY,
        CONTRAST_AGENT,
        CTV,
        EXTERNAL,
        GTV,
        IRRAD_VOLUME,
        ORGAN,
        PTV,
        TREATED_VOLUME,
        SUPPORT,
        FIXATION,
        DOSE_REGION
    }
  
}
