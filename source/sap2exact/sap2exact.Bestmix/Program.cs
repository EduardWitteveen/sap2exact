using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact.Bestmix
{
    class Program
    {
        static sap2exact.SapSDK.SapDatabaseConnection sapconnection;
        static sap2exact.SapSDK.SDK sapsdk;
        static System.Data.SqlClient.SqlConnection exactconnection;

        //        static Dictionary<string, string> skip =  new Dictionary<string, string>();
        static List<BestmixRegel> results = new List<BestmixRegel>();

        static void Main(string[] args)
        {
            // . as decimal seperator
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            // read the sap data            
            sapconnection = new sap2exact.SapSDK.SapDatabaseConnection(Properties.Settings.Default.connection_string_sap);
            sapconnection.Open();

            sapsdk = new SapSDK.SDK(
                Properties.Settings.Default.sdk_sap_server,
                Properties.Settings.Default.sdk_sap_instance,
                Properties.Settings.Default.sdk_sap_user,
                Properties.Settings.Default.sdk_sap_password
            );

            exactconnection = new System.Data.SqlClient.SqlConnection()
            {
                ConnectionString = Properties.Settings.Default.connection_string_exact
            };
            exactconnection.Open();

            var exactitemscommand = new System.Data.SqlClient.SqlCommand()
            {
                CommandText = @"
                    SELECT 
                        itemcode,
                        description
                    FROM [100].[dbo].Items 
                    WHERE Class_03 = 10",
                // WHERE itemcode= 'GR010100'",
                Connection = exactconnection
            };

            using (var reader = exactitemscommand.ExecuteReader())
            {
                {
                    while (reader.Read())
                    {

                        var values = new object[reader.FieldCount];
                        reader.GetValues(values);
                        string exact_itemcode = (string)values[0];
                        string exact_description = (string)values[1];

                        BestmixRegel regel = new BestmixRegel()
                        {
                            grondstofcode = exact_itemcode,
                            grondstofomschrijving = exact_description
                        };
                        GetCharacteristics(regel, 100, exact_itemcode);
                        GetIngredients(regel, 100, exact_itemcode);
                    }
                }
            }
            exactconnection.Close();
            sapsdk.Dispose();
            sapconnection.Close();

            System.IO.File.WriteAllText(@"C:\exact importeren artikelen\source\sap2exact\sap2exact.Bestmix\bestmix.csv", ToCsv(";", results));
        }

        private static void GetIngredients(BestmixRegel referentieregel, int mandt, string exact_itemcode)
        {
            /*
            FROM AUSP
            LEFT OUTER JOIN INOB
                ON INOB.MANDT = AUSP.MANDT
                AND INOB.OBTAB = 'MARA'
                AND INOB.OBJEK = AUSP.OBJEK 
            */
            var bomsql = @"
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
    STPO.SORTF AS STPO_SORTF,
    '------',
    STPO.POTX1 AS STPO_POTX1,
    STPO.POTX2 AS STPO_POTX2,
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
    AND  STPO.STLKN = STAS.STLKN
WHERE MAST.MANDT= :mandt
AND MAST.MATNR = :matnr
ORDER BY MAST.STLAN, MAST.STLAL, STPO.POSNR";
            var bomtable = sapconnection.QueryTable(bomsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", exact_itemcode } });

            if (bomtable.Rows.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("NO CHILDS FOUND FOR: '" + exact_itemcode + "'");
                return;
            }
            foreach (System.Data.DataRow bomrow in bomtable.Rows)
            {
                System.Diagnostics.Debug.Assert(bomrow["STKO_LKENZ"].ToString() == "");
                System.Diagnostics.Debug.Assert(bomrow["STAS_LKENZ"].ToString() == "");
                System.Diagnostics.Debug.Assert(bomrow["STPO_LKENZ"].ToString() == "");

                var matnr = Convert.ToString(bomrow["STPO_idnrk"]);
                var posnr = Convert.ToString(bomrow["STPO_posnr"]);

                // textregel
                /*
                string text = sapsdk.GetLongText(
                        Convert.ToString(bomrow["STPO_MANDT"]),
                        Convert.ToString(bomrow["STPO_STLTY"]),
                        Convert.ToString(bomrow["STPO_STLNR"]),
                        Convert.ToString(bomrow["STPO_STLKN"]),
                        Convert.ToString(bomrow["STPO_STPOZ"])
                );
                */

                BestmixRegel regel = new BestmixRegel()
                {
                    grondstofcode = referentieregel.grondstofcode,
                    grondstofomschrijving = referentieregel.grondstofomschrijving,

                    variantcode = Convert.ToString(bomrow["MAST_STLAL"]),
                    variantomschrijving = Convert.ToString(bomrow["STKO_STKTX"]),

                    ingredientposnr = posnr,
                    ingredientcode = matnr,
                };

                // taal fix: done
                var descriptionsql = @"
                -- Material ArtikelOmschrijvingen
                -- http://www.stechno.net/sap-tables.html?view=saptable&id=MAKT
                SELECT *
                FROM MAKT
                WHERE MAKT.MANDT = :mandt
                AND MAKT.MATNR = :matnr
                ";

                System.Data.DataTable result = sapconnection.QueryTable(descriptionsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", matnr } });
                foreach (System.Data.DataRow descriptionrow in result.Rows)
                {
                    regel.ingredienttaalcode = Convert.ToString(descriptionrow["spras"]);
                    regel.ingredientomschrijving = Convert.ToString(descriptionrow["maktx"]);
                    GetCharacteristics(regel, mandt, matnr);
                }
            }
        }

        private static void GetCharacteristics(BestmixRegel referentieregel, int mandt, string matnr)
        {
            /*
                        if (skip.ContainsKey(matnr))
                        {
                            System.Diagnostics.Debug.WriteLine("\tSKIPPING: '" + matnr + "'");
                            return;
                        }
                        else
                        {
                            skip.Add(matnr, matnr);
                        }
            */
            // taal fix: done
            var characteristicssql = @"
SELECT
    AUSP.MANDT,
    CABNT.SPRAS,
    AUSP.OBJEK,
    AUSP.ATINN,
    AUSP.ATWRT,
    AUSP.ATFLV,
    CABN.MSEHI,    
    CABN.ATNAM,
    CABNT.ATBEZ,
    CAWN.ATZHL,
    CAWNT.SPRAS,
    CAWNT.ATWTB
FROM AUSP
LEFT OUTER JOIN CABN 
    ON CABN.MANDT = AUSP.MANDT
    AND CABN.ATINN =  AUSP.ATINN
LEFT OUTER JOIN CABNT  
    ON CABNT.MANDT = AUSP.MANDT
    AND CABNT.ATINN =  AUSP.ATINN
LEFT OUTER JOIN CAWN
    ON CAWN.MANDT = AUSP.MANDT
    AND CAWN.ATINN =  AUSP.ATINN
    AND CAWN.ATWRT =  AUSP.ATWRT
LEFT OUTER JOIN CAWNT 
    ON CAWNT.MANDT = AUSP.MANDT
    AND CAWNT.ATINN= AUSP.ATINN
    AND CAWNT.ATZHL =  CAWN.ATZHL
    AND CAWNT.SPRAS = CABNT.SPRAS
WHERE AUSP.MANDT = :mandt
AND AUSP.OBJEK = :matnr
";
            System.Data.DataTable characteristicstable = null;
//            if (taal != null)
//            {
//                characteristicssql += @"
//                AND CABNT.SPRAS = :spras
//                ORDER BY ATNAM, CABNT.SPRAS";
//                characteristicstable  = sapconnection.QueryTable(characteristicssql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", matnr }, { ":spras", referentieregel.taalcode } }); 
//            }
//            else
//            {
                characteristicssql += @"
                ORDER BY ATNAM, CABNT.SPRAS";
                characteristicstable  = sapconnection.QueryTable(characteristicssql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", matnr }}); 
            //}

            if (characteristicstable.Rows.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("NO CHARACTERISTICS FOUND FOR: '" + matnr + "'");
                return;
            }
            foreach (System.Data.DataRow characteristicsrow in characteristicstable.Rows)
            {
                BestmixRegel regel = new BestmixRegel()
                {
                    grondstofcode = referentieregel.grondstofcode,
                    grondstofomschrijving = referentieregel.grondstofomschrijving,

                    variantcode = referentieregel.variantcode,
                    variantomschrijving = referentieregel.variantomschrijving,

                    ingredientposnr = referentieregel.ingredientposnr,
                    ingredienttaalcode = referentieregel.ingredienttaalcode,
                    ingredientcode = referentieregel.ingredientcode,
                    ingredientomschrijving = referentieregel.ingredientomschrijving,

                    klassetaalcode = Convert.ToString(characteristicsrow["spras"]),                    
                    klassecode = Convert.ToString(characteristicsrow["atnam"]),
                    sleutelomschrijving = Convert.ToString(characteristicsrow["atbez"]),
                    waardecode = Convert.ToString(characteristicsrow["atwrt"]),
                    waardeomschrijving = Convert.ToString(characteristicsrow["atwtb"])
                };
                if (regel.waardecode == "" && regel.waardeomschrijving == "")
                {
                    regel.waardecode = Convert.ToString(characteristicsrow["atflv"]);
                    regel.waardeomschrijving = Convert.ToString(characteristicsrow["msehi"]);
                }
                results.Add(regel);
            }
        }
        public static string ToCsv<T>(string separator, IEnumerable<T> objectlist)
        {
            Type t = typeof(T);
            FieldInfo[] fields = t.GetFields();

            string header = String.Join(separator, fields.Select(f => f.Name).ToArray());

            StringBuilder csvdata = new StringBuilder();
            csvdata.AppendLine(header);

            foreach (var o in objectlist)
                csvdata.AppendLine(ToCsvFields(separator, fields, o));

            return csvdata.ToString();
        }

        public static string ToCsvFields(string separator, FieldInfo[] fields, object o)
        {
            StringBuilder linie = new StringBuilder();

            foreach (var f in fields)
            {
                if (linie.Length > 0)
                    linie.Append(separator);

                var x = f.GetValue(o);

                if (x != null)
                    linie.Append(x.ToString());
            }

            return linie.ToString();
        }
    }
}
