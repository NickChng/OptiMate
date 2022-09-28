using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Reflection;
using System.ComponentModel;
using Serilog;
using System.Diagnostics;

namespace Optimate
{

    public static class Helpers
    {
        public static class SeriLog
        {
            public static void Initialize(string user = "RunFromLauncher")
            {
                var SessionTimeStart = DateTime.Now;
                var AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var directory = Path.Combine(AssemblyPath, @"Logs");
                var logpath = Path.Combine(directory, string.Format(@"log_{0}_{1}_{2}.txt", SessionTimeStart.ToString("dd-MMM-yyyy"), SessionTimeStart.ToString("hh-mm-ss"), user.Replace(@"\", @"_")));
                Log.Logger = new LoggerConfiguration().WriteTo.File(logpath, Serilog.Events.LogEventLevel.Information,
                    "{Timestamp:dd-MMM-yyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}").CreateLogger();
            }
            public static void AddLog(string log_info)
            {
                Log.Information(log_info);

            }
            public static void AddWarning(string log_info)
            {
                Log.Warning(log_info);
            }
            public static void AddError(string log_info, Exception ex = null)
            {
                if (ex == null)
                    Log.Error(log_info);
                else
                    Log.Error(ex, log_info);
            }
            public static void AddFatal(string log_info, Exception ex)
            {
                Log.Fatal(ex, log_info);
            }
        }
        public static class LevenshteinDistance
        {
            /// <summary>
            /// Compute the distance between two strings.
            /// </summary>
            public static int Compute(string s, string t)
            {
                int n = s.Length;
                int m = t.Length;
                int[,] d = new int[n + 1, m + 1];

                // Step 1
                if (n == 0)
                {
                    return m;
                }

                if (m == 0)
                {
                    return n;
                }

                // Step 2
                for (int i = 0; i <= n; d[i, 0] = i++)
                {
                }

                for (int j = 0; j <= m; d[0, j] = j++)
                {
                }

                // Step 3
                for (int i = 1; i <= n; i++)
                {
                    //Step 4
                    for (int j = 1; j <= m; j++)
                    {
                        // Step 5
                        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                        // Step 6
                        d[i, j] = Math.Min(
                            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                            d[i - 1, j - 1] + cost);
                    }
                }
                // Step 7
                return d[n, m];
            }

        }

        public static class OrientationInvariantMargins
        {
            public static AxisAlignedMargins getAxisAlignedMargins(PatientOrientation patientOrientation, double rightMargin, double antMargin, double infMargin, double leftMargin, double postMargin, double supMargin)
            {
                switch (patientOrientation)
                {
                    case PatientOrientation.HeadFirstSupine:
                        return new AxisAlignedMargins(StructureMarginGeometry.Outer, rightMargin, antMargin, infMargin, leftMargin, postMargin, supMargin);
                    case PatientOrientation.HeadFirstProne:
                        return new AxisAlignedMargins(StructureMarginGeometry.Outer, leftMargin, postMargin, infMargin, rightMargin, antMargin, supMargin);
                    case PatientOrientation.FeetFirstSupine:
                        return new AxisAlignedMargins(StructureMarginGeometry.Outer, leftMargin, antMargin, supMargin, rightMargin, postMargin, infMargin);
                    case PatientOrientation.FeetFirstProne:
                        return new AxisAlignedMargins(StructureMarginGeometry.Outer, rightMargin, postMargin, supMargin, leftMargin, antMargin, infMargin);
                    default:
                        throw new Exception("This orientation is not currently supported");
                }
            }
        }
    }


}
