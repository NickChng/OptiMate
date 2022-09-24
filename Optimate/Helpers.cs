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

namespace VMS.TPS
{

    public static class Helpers
    {
        public static class Logger
        {

            private static DateTime SessionTimeStart;

            private static string logpath = @"\\spvaimapcn\data$\VarianScripting\Optimate\Logs\";

            public static string user = "";

            public static string PID = "";

            public static void Initialize()
            {
                var AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                logpath = Path.Combine(AssemblyPath, @"Logs");
                SessionTimeStart = DateTime.Now;
            }
            public static void AddLog(string log_entry)
            {
                string path = Path.Combine(logpath, string.Format(@"log_{0}_{1}_{2}_{3}.txt", SessionTimeStart.ToString("dd-MMM-yyyy"), SessionTimeStart.ToString("hh-mm"), user.Replace(@"\", @"_"), PID));
                using (var data = new StreamWriter(path, true))
                {
                    data.WriteLine(log_entry);
                }
            }
        }
    }


}
