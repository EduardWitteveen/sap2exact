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
            /*
            SELECT 
                MAST.STLAN AS MAST_STLAN,
                MAST.STLAL AS MAST_STLAL,
                MAST.STLNR AS MAST_STLNR,
                STKO.STKTX AS STKO_STKTX,
                STKO.DATUV AS STKO_DATUV,
                STKO.BMENG AS STKO_BMENG,
                STAS.STLKN AS STAS_STLKN,
                STAS.AEDAT AS STAS_AEDAT,
                STAS.STVKN AS STAS_STVKN,
                STPO.AEDAT AS STPO_AEDAT,
                STPO.MENGE AS STPO_MENGE,
                STPO.MEINS AS STPO_MEINS,
                STPO.POSNR AS STPO_POSNR,
                STPO.IDNRK AS STPO_IDNRK,
                '------',
                STKO.LKENZ AS STKO_LKENZ,
                STKO.STKOZ AS STKO_STKOZ,
                STAS.LKENZ AS STAS_LKENZ,
                STAS.STASZ AS STAS_STASZ,
                STPO.LKENZ  AS STPO_LKENZ,
                STPO.STPOZ AS STPO_STPOZ,
                '------',
                STPO.POTX1 AS STPO_POTX1,
                STPO.POTX2 AS STPO_POTX2 , 
                '------',    
                STPO.MANDT AS STPO_MANDT,
                STPO.STLTY AS STPO_STLTY,
                STPO.STLNR AS STPO_STLNR,
                STPO.STLKN AS STPO_STLKN,
                STPO.STPOZ AS STPO_STPOZ
            FROM MAST
            JOIN STKO 
                ON STKO.MANDT  = MAST.MANDT
                AND STKO.STLNR = MAST.STLNR
                AND STKO.STLAL = MAST.STLAL
                AND NOT STKO.STKOZ  IN
                (
                    SELECT PREV_STKO.VGKZL
                    FROM STKO PREV_STKO
                    WHERE PREV_STKO.MANDT = MAST.MANDT 
                    AND PREV_STKO.STLNR= MAST.STLNR        
                )
            JOIN STAS
                ON STAS.MANDT = MAST.MANDT
                AND STAS.STLNR= MAST.STLNR
                AND STAS.STLAL = MAST.STLAL
    
                AND NOT STAS.STLKN IN 
                (
                    SELECT DEL_STAS.STLKN
                    FROM STAS DEL_STAS
                    WHERE DEL_STAS.MANDT = MAST.MANDT
                    AND DEL_STAS.STLNR= MAST.STLNR
                    AND DEL_STAS.STLAL = MAST.STLAL
                    AND DEL_STAS.LKENZ = 'X'        
                )
            JOIN STPO
                ON STPO.MANDT = MAST.MANDT 
                AND STPO.STLNR= MAST.STLNR
                AND NOT STPO.STPOZ IN
                (
                    SELECT PREV_STPO.VGPZL
                    FROM STPO PREV_STPO
                    WHERE PREV_STPO.MANDT = MAST.MANDT 
                    AND PREV_STPO.STLNR= MAST.STLNR        
                )
                AND  STPO.STLKN = STAS.STLKN
            WHERE MAST.MANDT= 100
            AND MAST.WERKS= '0001'
            AND MAST.MATNR = '01050D15'
            ORDER BY MAST.STLAN, MAST.STLAL, STPO.POSNR

            SELECT *
            FROM STXL
            WHERE TDNAME = '100M000024700000000500000010'
            */
            const string server = "dmc08";
            const string instance = "08";
            const string user = "steensma";
            const string password = "steensma";

            secept.RfcConnector.RfcSession rfcsession = new secept.RfcConnector.RfcSession();
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
            secept.RfcConnector.FunctionCall fc = rfcsession.ImportCall("READ_TEXT");
            System.Diagnostics.Debug.Assert(!rfcsession.Error);            
            // set the parameters
            fc.Importing["CLIENT"].value = 100;
            fc.Importing["ARCHIVE_HANDLE"].value = 0;
            fc.Importing["OBJECT"].value = "BOM";
            fc.Importing["ID"].value = "MPO";
            fc.Importing["NAME"].value = "100M000024700000000500000010";
            fc.Importing["LANGUAGE"].value = "N";
            // call the function
            rfcsession.CallFunction(fc);

            if (rfcsession.Error)
            {
                //  ^^^ FAILS HERE!!!!!
                //  with message: "Missing parameter with PERFORM." 
                rfcsession.Disconnect();
                throw new Exception(rfcsession.ErrorInfo.Message);
            }            
            if (fc.Tables.Count != 1)
            {
                rfcsession.Disconnect();
                throw new Exception("did not count 1 table, count:" + fc.Tables.Count);
            }

            foreach (secept.RfcConnector.RfcParameter table in fc.Tables)
            {
                System.Diagnostics.Debug.WriteLine("table:" + table.name);
                foreach (secept.RfcConnector.RfcFields row in table.Rows)
                {
                    System.Diagnostics.Debug.WriteLine("row:" + row.Count);
                }
            }
            rfcsession.Disconnect();
        }
    }
}
