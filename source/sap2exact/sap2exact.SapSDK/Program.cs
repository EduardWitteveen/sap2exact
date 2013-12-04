using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact.SapSDK
{
    public class Program
    {
        static void Main(string[] args)
        {
            const string server = "dmc08";
            const string instance = "08";
            const string user = "steensma";
            const string password = "steensma";

            secept.RfcConnector.RfcSession rfcsession = new secept.RfcConnector.RfcSession();
            // enable tracing for client
            rfcsession.Option["trace.file"] = new System.IO.FileInfo("sap-rpc.log").FullName;

            rfcsession.LicenseData.Owner = "(unregistered DEMO version)";
            rfcsession.LicenseData.key = "1EX4839S13W620TBRZ44NRE4ALBCVJH";

            // connection settings
            rfcsession.RfcSystemData.ConnectString = "ASHOST=" + server + " SYSNR=" + instance;
            rfcsession.LogonData.User = user;
            rfcsession.LogonData.Password = password;
            rfcsession.LogonData.Language = "EN";
            // connect
            rfcsession.Connect();
            if (rfcsession.Error) throw new Exception(rfcsession.ErrorInfo.Message);
                        
            //  SELECT *
            //  FROM STXL
            //  WHERE TDNAME = '100M000024700000000500000010'
            //  |----------|-----------|-----------|-----------------------------------|-----------|-----------|-----------|-----------|-----------|
            //  |   MANDT  |    RLID   |   TDOBJECT |   TDNAME                          |   TDID    |   TDSPRAS |   SRTF2   |   CLUSTR  |   CLUSTD  |
            //  |----------|-----------|-----------|-----------------------------------|-----------|-----------|-----------|-----------|-----------|
            //  |   100    |    TX     |   BOM      |   100M000024700000000500000010    |   MPO     |   N       |   0       |   288     |   ÿ €     |
            //  |----------|-----------|-----------|-----------------------------------|-----------|-----------|-----------|-----------|-----------|            
            // get the functionhandle
            secept.RfcConnector.FunctionCall fc = rfcsession.ImportCall("RFC_READ_TEXT", true);
            System.Diagnostics.Debug.Assert(!rfcsession.Error);
            // enter the parameters
            secept.RfcConnector.RfcFields newrow = fc.Tables["TEXT_LINES"].Rows.AddRow();
            newrow["TDOBJECT"].value = "BOM";
            string tdname = "100M000024700000000500000010";
            System.Diagnostics.Debug.Assert(tdname.Length == 28);
            newrow["TDNAME"].value = tdname;
            newrow["TDID"].value = "MPO";
            newrow["TDSPRAS"].value = "N";
            // call the function
            rfcsession.CallFunction(fc);

            if (rfcsession.Error)
            {
                rfcsession.Disconnect();
                throw new Exception(rfcsession.ErrorInfo.Message);
            }
            StringBuilder result = new StringBuilder();
            foreach (secept.RfcConnector.RfcFields row in fc.Tables["TEXT_LINES"].Rows) {
                string tdline = row["TDLINE"].value;
                result.AppendLine(tdline);                
            }
            System.Diagnostics.Debug.WriteLine("result:" + result);
            rfcsession.Disconnect();
        }
    }
}
