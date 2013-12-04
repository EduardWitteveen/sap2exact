using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using RFCCONNECTORLib;
using System.Data;


namespace sap2exact.SapSDK
{
    public class SDK : IDisposable
    {
        private RfcSession rfcsession = new RfcSession();
        public SDK(string server, string instance, string user, string password)
        {
            
            rfcsession.RfcSystemData.ConnectString = "ASHOST=" + server + " SYSNR=" + instance;
            rfcsession.LogonData.User = user;
            rfcsession.LogonData.Password = password;
            rfcsession.LogonData.Language = "EN";

            // connect and check error
            rfcsession.Connect();

            if (rfcsession.Error) throw new Exception(rfcsession.ErrorInfo.Message);
        }

        public string[] CallParameters(string function)
        {
            List<string> result = new List<string>();

            FunctionCall fc = rfcsession.ImportCall(function);
            foreach (RfcParameter p in fc.Importing)
            {
                System.Diagnostics.Debug.WriteLine("function: '" + function + "' with parameter: '" + p.name + "'");
                result.Add(p.name);
            }
            return result.ToArray();
        }

        public string Call(string function, Dictionary<string, object> parameters) 
        {
            FunctionCall fc = rfcsession.ImportCall(function);
            // print the functions
            //foreach (RfcParameter p in fc.Importing)
            //{
            //    System.Diagnostics.Debug.WriteLine("function: '" + function + "' with parameter: '" + p.name + "'");
            //}
            foreach (string parametername in parameters.Keys)
            {
                fc.Importing[parametername].value = parameters[parametername];
            }

            System.Diagnostics.Debug.Assert(!rfcsession.Error);

            rfcsession.CallFunction(fc);
            if (rfcsession.Error)
            {
                string message = rfcsession.ErrorInfo.Message;
                throw new Exception(message);
            }

            if (fc.Tables.Count != 1)
            {
                throw new Exception("function: " + function + " : did not count 1 table, count:" + fc.Tables.Count);
            }

            DataSet result = new DataSet();
            foreach(RfcParameter table in fc.Tables) {
                DataTable dt = new DataTable(table.name);

                foreach (RfcFields row in table.Rows)
                {
                    int i = 42;
                }
            }
            return "appelmoes";
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
                rfcsession.Disconnect();
            }
            // get rid of unmanaged resources
        }
    }
}
