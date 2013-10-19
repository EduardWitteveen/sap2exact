using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace access2exact
{
    public static class Output
    {
        private class ErrorInfo
        {
            private FileInfo errorlog;
            public ErrorInfo()
            {
                errorlog = new FileInfo("error.txt");
                if (errorlog.Exists) errorlog.Delete();
            }
            public void Write(string message) {
                var writer = errorlog.AppendText();
                writer.WriteLine(message);
                writer.Close();
            }
            
        }
        private static ErrorInfo errorinfo = new ErrorInfo();

        public static void Info(string message)
        {
            System.Diagnostics.Debug.WriteLine("[OUTPUT INFO] " + message);
            Console.Out.WriteLine(message);
        }

        public static void Error(string message)
        {
            errorinfo.Write(message);
            System.Diagnostics.Debug.WriteLine("[OUTPUT ERROR] " + message);
            Console.Error.WriteLine(message);
        }
    }
}
