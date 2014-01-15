using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact.Leveranciers
{
    class Program
    {
        static void Main(string[] args)
        {
            // . as decimal seperator
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            SapDatabaseConnection connection = new SapDatabaseConnection(Properties.Settings.Default.connection_string_sap);
            connection.Open();
/*
            var sql = @"
SELECT
    marav.matnr,
    marav.spras,    
    marav.maktx,
    marav.brgew,
    marav.ntgew,
    marav.gewei,
    marav.volum,    
    marav.voleh,
    eina.lifnr,
    eina.meins,
    eina.umrez,
    eina.telf1,
    eina.urzla,
    eina.lmein,
    eine.waers,
    eine.datlb,
    eine.netpr,
    eine.peinh,
    eine.bprme,    
    eine.prdat,        
    eine.effpr,
    lfa1.land1,
    lfa1.name1,    
    lfa1.name2,
    lfa1.pfach,
    lfa1.pstl2,
    lfa1.pstlz,
    lfa1.stras,
    lfa1.telf1,
    lfa1.telfx,
    lfa1.stceg,
    lfa1.stceg,
    '----------------' AS marav,
    marav.*,
    '----------------' AS eina,
    eina .*,    
    '----------------' AS eine,
    eine .*,  
    '----------------' AS lfa1,
    lfa1.*   
FROM marav
JOIN eina 
    ON eina.matnr = marav.matnr
JOIN eine
    ON eine.infnr = eina.infnr
JOIN lfa1
    ON lfa1.lifnr = eina.lifnr
ORDER BY marav.matnr
";
                        //connection.Export2Excel("leveranciers-artikel-prijs", sql);
                        connection.Export2Csv("leveranciers-artikel-prijs", sql);
*/
            var sql = @"
SELECT
    VBAP.VBELN,
    VBAP.POSNR,
    VBAP.MATNR,
    '43' || VBAP.MATNR AS exactnr,
    VBAP.ARKTX,
    VBAP.ZIEME,
    VBAP.MEINS,
    VBAP.NETWR,
    VBAP.WAERK,
    VBAP.KWMENG,
    VBAP.LSMENG,
    VBAP.KBMENG,
    VBAP.KLMENG,
    VBAP.VRKME,
    VBAP.BRGEW,
    VBAP.NTGEW,
    VBAP.GEWEI,
    VBAP.ERDAT,
    VBAP.NETPR,
    VBAP.KPEIN,
    VBAP.KMEIN,
    VBAP.WAVWR,
    VBAP.KZWI1,
    VBAP.KZWI2,
    VBAP.KZWI3,    
    VBAP.KZWI4,
    VBAP.KZWI5,
    VBAP.KZWI6,
    VBAP.AEDAT,
    VBAP.CMPRE,
    VBAP.CMPRE_FLT,
    VBAP.MWSBP,
    VBAP.KOSTL,
    VBEP.EDATU,
    '----------------' AS DATUM,
    YEAR(VBEP.EDATU) AS JAAR,
    MONTH(VBEP.EDATU) AS MAAND,
    DAY(VBEP.EDATU) AS DAG,
    '----------------' AS VBAP,
    VBAP.*,
    '----------------' AS VBEP,
    VBEP.*
FROM VBAP
JOIN VBEP
ON VBEP.VBELN = VBAP.VBELN
AND VBEP.POSNR = VBAP.POSNR
WHERE VBEP.EDATU >= 20120101
ORDER BY VBEP.EDATU DESC
";
            connection.Export2Csv("sales", sql);
            connection.Close();
        }
    }
}
