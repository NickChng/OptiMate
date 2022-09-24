using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimate
{
    public enum WarningLevels
    {
        [Description("Script Error")] Failure,
        [Description("Error: No GTVs or CTVen structures found")] FailureNoGTVs,
        [Description("CTV(s) created")] NoWarningCTVs,
        [Description("PTV generation completed")] NoWarningPTVs,
        [Description("CTVs created, but cropping created subvolumes")] SubvolumeWarningCTVs,
        [Description("CTVp has subvolume(s)")] SubvolumeWarningCTVp,
        [Description("CTVn has subvolume(s)")] SubvolumeWarningCTVn,
        [Description("CTVpn has subvolume(s)")] SubvolumeWarningCTVpn,
        [Description("GTVp has small subvolume, please remove before running script")] SubvolumeWarningGTVp,
        [Description("GTVn has small subvolume, please remove before running script")] SubvolumeWarningGTVn,
        [Description("PTV created but CTVen may have small subvolume, please review")] SubvolumeWarningCTVen,

    }
}
