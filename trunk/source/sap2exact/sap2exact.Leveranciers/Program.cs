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
            connection.Export2Excel("leveranciers-artikel-prijs", sql);

            connection.Close();
        }
    }
}
