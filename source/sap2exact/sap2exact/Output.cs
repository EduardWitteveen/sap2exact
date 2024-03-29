﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact
{
    public static class Output
    {
        private class InfoLog : IDisposable
        {
            private FileInfo infolog;
            private StreamWriter writer;
            public InfoLog()
            {
                infolog = new FileInfo("info.txt");
                if (infolog.Exists) infolog.Delete();
                writer = infolog.AppendText();
            }
            public void Write(string message)
            {
                writer.WriteLine("[" + DateTime.Now.Ticks + "] " + message);
            }
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // get rid of managed resources

                    // make sure we flush
                    writer.Close();
                }
                // get rid of unmanaged resources
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
            infolog.Write(message);
            System.Diagnostics.Debug.WriteLine("[OUTPUT INFO] " + message);
            Console.Out.WriteLine(message);
        }

        public static void Error(string message)
        {
            errorlog.Write(message);
            System.Diagnostics.Debug.WriteLine("[OUTPUT ERROR] " + message);
            Console.Error.WriteLine(message);
        }

        public static void Dispose()
        {
            infolog.Dispose();
        }
    }
}
