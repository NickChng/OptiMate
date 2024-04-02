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
using OptiMate.ViewModels;

namespace OptiMate
{

    public static class Helpers
    {
        public static string MakeStringPathSafe(string user)
        {
            string pathSafeUser = user;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                pathSafeUser = pathSafeUser.Replace(c, '_');
            }
            return pathSafeUser;
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

        public static string OperatorName(OperatorTypes operatorType)
        {
            switch (operatorType)
            {
                case OperatorTypes.convertResolution:
                    return "High Res.";
                case OperatorTypes.margin:
                    return "Margin";
                case OperatorTypes.asymmetricMargin:
                    return "Asym.Margin";
                case OperatorTypes.or:
                    return "Or";
                case OperatorTypes.and:
                    return "And";
                case OperatorTypes.crop:
                    return "Crop";
                case OperatorTypes.asymmetricCrop:
                    return "Asym.Crop";
                case OperatorTypes.sub:
                    return "Sub";
                case OperatorTypes.subfrom:
                    return "Sub From";
                case OperatorTypes.convertDose:
                    return "Convert Dose";
                default:
                    return "Unknown";
            }
        }

        public static string CompactForm(this string alias)
        {
            return alias.Replace("_", "").Replace(" ", "").Replace("-", "");
        }


        public static class OrientationInvariantMargins
        {
            public static AxisAlignedMargins getAxisAlignedMargins(PatientOrientation patientOrientation, MarginTypes marginType, double rightMargin, double antMargin, double infMargin, double leftMargin, double postMargin, double supMargin)
            {
                StructureMarginGeometry geometry;
                if (marginType == MarginTypes.Inner)
                {
                    geometry = StructureMarginGeometry.Inner;
                }
                else
                {
                    geometry = StructureMarginGeometry.Outer;
                }
                switch (patientOrientation)
                {
                    case PatientOrientation.HeadFirstSupine:
                        return new AxisAlignedMargins(geometry, rightMargin, antMargin, infMargin, leftMargin, postMargin, supMargin);
                    case PatientOrientation.HeadFirstProne:
                        return new AxisAlignedMargins(geometry, leftMargin, postMargin, infMargin, rightMargin, antMargin, supMargin);
                    case PatientOrientation.FeetFirstSupine:
                        return new AxisAlignedMargins(geometry, leftMargin, antMargin, supMargin, rightMargin, postMargin, infMargin);
                    case PatientOrientation.FeetFirstProne:
                        return new AxisAlignedMargins(geometry, rightMargin, postMargin, supMargin, leftMargin, antMargin, infMargin);
                    default:
                        throw new Exception("This orientation is not currently supported");
                }
            }
        }
    }
}



