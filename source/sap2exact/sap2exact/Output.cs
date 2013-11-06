using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact
{
    public static class Output
    {
        private class InfoLog
        {
            private FileInfo infolog;
            public InfoLog()
            {
                infolog = new FileInfo("info.txt");
                if (infolog.Exists) infolog.Delete();
            }
            public void Write(string message)
            {
                var writer = infolog.AppendText();
                writer.WriteLine("[" + DateTime.Now.Ticks + "] " + message);
                writer.Close();
            }

        }
        private class ErrorLog
        {
            private FileInfo errorlog;
            public ErrorLog()
            {
                errorlog = new FileInfo("error.txt");
                if (errorlog.Exists) errorlog.Delete();
            }
            public void Write(string message) {
                var writer = errorlog.AppendText();
                writer.WriteLine("[" + DateTime.Now.Ticks + "] " + message);
                writer.Close();
            }
            
        }
        private static ErrorLog errorlog = new ErrorLog();
        private static InfoLog infolog = new InfoLog();

        public static void Info(string message)
        {
            errorlog.Write(message);
            System.Diagnostics.Debug.WriteLine("[OUTPUT INFO] " + message);
            Console.Out.WriteLine(message);
        }

        public static void Error(string message)
        {
            errorlog.Write(message);
            System.Diagnostics.Debug.WriteLine("[OUTPUT ERROR] " + message);
            Console.Error.WriteLine(message);
        }
    }
}
