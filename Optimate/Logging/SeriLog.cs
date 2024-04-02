using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptiMate.Logging
{
    public static class SeriLogModel
    {
        private static ILogger _modelLogger;
        internal static void Initialize(string logPath, string user = "RunFromLauncher")
        {
           
            var filePath = Path.Combine(logPath, $"OptiMateModelLog_{Helpers.MakeStringPathSafe(user)}_{DateTime.Now.ToString("dd-MMM-yyyy")}.txt");
            _modelLogger = new LoggerConfiguration().WriteTo.File(filePath, Serilog.Events.LogEventLevel.Information,
                "{Timestamp:dd-MMM-yyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}").CreateLogger();
        }

        internal static void AddLog(string log_info)
        {
            _modelLogger.Information(log_info);

        }
        internal static void AddWarning(string log_info)
        {
            _modelLogger.Warning(log_info);
        }
        internal static void AddError(string log_info, Exception ex = null)
        {
            if (ex == null)
                _modelLogger.Error(log_info);
            else
                _modelLogger.Error(ex, log_info);
        }
        internal static void AddFatal(string log_info, Exception ex)
        {
            Log.Fatal(ex, log_info);
        }
    }

    public static class SeriLogUI
    {
        private static ILogger _uiLogger;
        internal static void Initialize(string logPath, string user = "RunFromLauncher")
        {
            var filePath = Path.Combine(logPath, $"OptiMateUIlLog_{Helpers.MakeStringPathSafe(user)}_{DateTime.Now.ToString("dd-MMM-yyyy")}.txt");
            _uiLogger = new LoggerConfiguration().WriteTo.File(filePath, Serilog.Events.LogEventLevel.Information,
                "{Timestamp:dd-MMM-yyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}").CreateLogger();
        }
        internal static void AddLog(string log_info)
        {
            _uiLogger.Information(log_info);

        }
        internal static void AddWarning(string log_info)
        {
            _uiLogger.Warning(log_info);
        }
        internal static void AddError(string log_info, Exception ex = null)
        {
            if (ex == null)
                _uiLogger.Error(log_info);
            else
                _uiLogger.Error(ex, log_info);
        }
        internal static void AddFatal(string log_info, Exception ex)
        {
            _uiLogger.Fatal(ex, log_info);
        }
    }

   
}
