using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact.Artikelgroepen
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
    MATKL AS artikelgroep,
    MATNR AS artikelsap
FROM MARAV
";
            connection.Export2Excel("artikelgroepen", sql);

            //////////////////////////////////////
            sql = @"
SELECT 
    MATKL AS artikelgroep,
    MATNR AS artikelsap
FROM MARAV
WHERE NOT 
(
    MATNR LIKE  MATKL || '%'
    OR 
    MATNR LIKE  '0000000000' || MATKL || '%'
    OR MATKL = 'VERVALLEN'
)
";
            connection.Export2Excel("afwijking-artikelgroepen", sql);
            ///////////////////////////////


            connection.Close();
        }
    }
}
