using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLib.Utilities
{
    public static class ExceptionReporter
    {
        public static void WriteReport(Exception exception, String path)
        {
            if (exception == null) return;
            StringBuilder pathFilename = new StringBuilder();
            pathFilename.Append(path);
            pathFilename.Append(@"\_ERRORLOG_");
            pathFilename.Append(DateTime.Now.Year).Append(DateTime.Now.Month).Append(DateTime.Now.Day);
            pathFilename.Append("-");
            pathFilename.Append(DateTime.Now.Hour).Append(DateTime.Now.Minute).Append(DateTime.Now.Second);
            pathFilename.Append("-");
            pathFilename.Append(DateTime.Now.Millisecond);
            pathFilename.Append(".txt");

            StringBuilder report = new StringBuilder();
            report.Append("OS Version: ").Append(Environment.OSVersion).Append(Environment.NewLine);
            report.Append("Runtime Version: ").Append(Environment.Version).Append(Environment.NewLine).Append(Environment.NewLine);
            report.Append("Message: ").Append(Environment.NewLine).Append(exception.Message).Append(Environment.NewLine).Append(Environment.NewLine);
            report.Append("Stacktrace: ").Append(Environment.NewLine).Append(exception.StackTrace).Append(Environment.NewLine).Append(Environment.NewLine);
            report.Append("Details: ").Append(Environment.NewLine).Append(exception.ToString()).Append(Environment.NewLine);

            using (StreamWriter writer = new StreamWriter(pathFilename.ToString(), true))
            {
                writer.WriteLine(report.ToString());
            }
        }
    }
}
