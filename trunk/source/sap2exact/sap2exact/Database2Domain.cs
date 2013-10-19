using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace access2exact
{
    public class Database2Domain
    {
        private Domain.ExportData data;

        private MaxDB.Data.MaxDBConnection sapconnection;

        public Database2Domain(MaxDB.Data.MaxDBConnection sapconnection)
        {
            // TODO: Complete member initialization
            this.sapconnection = sapconnection;
        }

        private DateTime ConvertSapDate(object value, string where)
        {
            if (Convert.ToString(value) == "00000000")
            {
                Output.Error("Could not convert the date for: " + where);
                return DateTime.Now;
            }
            var str = Convert.ToString(value);
            int year = Convert.ToInt32(str.Substring(0, 4));
            int month =  Convert.ToInt32(str.Substring(4, 2));
            int day  = Convert.ToInt32(str.Substring(6, 2));            
            return new DateTime(year,month,day);
        }

        private DataRow QueryRow(string sql, Dictionary<string, object> parameters = null)
        {
            var table = QueryTable(sql, parameters);
            if (table.Rows.Count > 1)
            {
                throw new Exception("found more than 1 record for sql:" + sql);
            }
            else if (table.Rows.Count == 0)
            {
                return null;
            }
            return table.Rows[0];
        }

        private DataTable QueryTable(string sql, Dictionary<string, object> parameters = null)
        {
            var cmd = new MaxDB.Data.MaxDBCommand(sql, sapconnection);
            cmd.CommandType = System.Data.CommandType.Text;
            if (parameters != null)
            {
                foreach (string key in parameters.Keys)
                {
                    cmd.Parameters.Add(key, parameters[key]);
                }
            }
            var adapter = new MaxDB.Data.MaxDBDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public void Export2Excel(string name, string sql, Dictionary<string, object> parameters = null)
        {
            Output.Info("Exporting: " + name + "\n\t" + sql);

            // query
            var cmd = new MaxDB.Data.MaxDBCommand(sql, sapconnection);
            cmd.CommandType = System.Data.CommandType.Text;
            if (parameters != null)
            {
                foreach (string key in parameters.Keys)
                {
                    cmd.Parameters.Add(key, parameters[key]);
                }
            }
            var reader = cmd.ExecuteReader();
            var table = new DataTable();
            table.Load(reader);

            // export to excel
            var exportfile = new System.IO.FileInfo(name + ".xlsx");
            if (exportfile.Exists) exportfile.Delete();
            XLWorkbook workbook = new XLWorkbook();
            workbook.Worksheets.Add(table, "data");
            var worksheet = workbook.Worksheets.Add("sql");
            worksheet.Cell("A1").Value = sql;
            workbook.SaveAs(exportfile.FullName);
        }


        public Domain.ExportData ReadEindArtikelData(String artikelnummer = null)
        {
            data = new Domain.ExportData();
            if (artikelnummer == null)
            {
                var artikelsql = @"
                    -- AUFK - Order master data
                    -- http://www.stechno.net/sap-tables.html?view=saptable&id=AUFK
                    -- AFPO: Order item
                    -- http://www.stechno.net/sap-tables.html?view=saptable&id=AFPO
                    -- MARA: General Material Data
                    -- http://www.stechno.net/sap-tables.html?view=saptable&id=MARA
                    SELECT DISTINCT
	                    MARA.MANDT,
	                    MARA.MATNR
                    FROM MARA
                    INNER JOIN AFPO
	                    ON AFPO.MATNR = MARA.MATNR
	                    AND AFPO.MANDT  = MARA.MANDT
                    INNER JOIN AUFK
	                    ON AUFK.AUFNR = AFPO.AUFNR
	                    AND AUFK.MANDT  = AFPO.MANDT 
                    WHERE
	                    NOT MARA.MATKL = 'VERVALLEN' 
                        AND MARA.MTART = 'FERT'
                        -- AND MARA.MATNR LIKE '33024%' 
	                    AND
	                    (
		                    AUFK.ERDAT > 20120000 
	                    OR 
		                    AUFK.AEDAT > 20120000 
	                    ) 
	                    AND MARA.MANDT = 100
                ";
                var artikeltable = QueryTable(artikelsql);

                foreach (DataRow artikelrow in artikeltable.Rows)
                {
                    int matdt = Convert.ToInt32(artikelrow["mandt"]);
                    string matnr = Convert.ToString(artikelrow["matnr"]);

                    int huidigeregel = artikeltable.Rows.IndexOf(artikelrow);
                    int totaalregels = artikeltable.Rows.Count;
                    Output.Info("[ " + (int)((100.0 / totaalregels) * huidigeregel) + "% ] START:" + matnr);

                    if (data.Retrieve(matnr) == null)
                    {
                        data.Add(ReadArtikelData(matdt, matnr, 1));
                    }
                    else
                    {
                        //System.Diagnostics.Debug.Assert(false);
                        Output.Error("Eindartikel fout, was al ingeladen: " + matnr);
                    }
                }
            }
            else
            {
                data.Add(ReadArtikelData(100, artikelnummer, 1));                
            }
            return data;
        }

        private Domain.BaseArtikel ReadArtikelData(int mandt, string matnr, int ident)
        {
            Domain.BaseArtikel artikel;

            // http://www.erpgenie.com/sap/abap/tables_mm.htm

            #region artikeltype
            var artikelsql = @"
                -- General Material Data
                -- http://www.stechno.net/sap-tables.html?view=saptable&id=MARA
                SELECT *
                FROM MARA
                WHERE MARA.MANDT = :mandt
                AND MARA.MATNR = :matnr
            ";
            var artikelrow = QueryRow(artikelsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", matnr } });
            var mtart = artikelrow["mtart"].ToString();
            switch (mtart)
            {
                case "FERT":
                    artikel = new Domain.EindArtikel();
                    #region artikel belasting
                    var belastingsql = @"
                            -- Tax Classification for Material
                            -- http://www.stechno.net/sap-tables.html?view=saptable&id=MLAN
                            SELECT *
                            FROM MLAN
                            WHERE MLAN.MANDT = :mandt
                            AND MLAN.MATNR = :matnr
                        ";
                    var belastingrow = QueryRow(belastingsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", matnr } });
                    if (belastingrow != null)
                    {
                        Debug.Assert("NL" == Convert.ToString(belastingrow["aland"]));
                        var taxm1 = Convert.ToInt32(belastingrow["taxm1"]);
                        artikel.PrijsBelastingCategorie = taxm1;  //wat betekend wat?
                    }
                    else
                    {
                        Output.Error("NO TAX FOR ARTICLE:" + matnr);
                    }

                    #endregion artikel belasting                    
                    break;
                case "HALB":
                    artikel = new Domain.ReceptuurArtikel();
                    artikel.PrijsBelastingCategorie = 2;
                    break;
                case "ZROH":
                case "ROH":
                    artikel = new Domain.GrondstofArtikel();
                    artikel.PrijsBelastingCategorie = 2;
                    break;
                case "INGR":
                    artikel = new Domain.IngredientArtikel();
                    artikel.PrijsBelastingCategorie = 2;
                    break;
                case "VERP":
                case "LEER":
                    // https://help.sap.com/saphelp_45b/helpdata/en/ff/515afd49d811d182b80000e829fbfe/content.htm
                    artikel = new Domain.VerpakkingsArtikel();
                    artikel.PrijsBelastingCategorie = 4;
                    break;
                default:
                    string type = Convert.ToString(artikelrow["bom_artikelsoort"]);
                    throw new NotImplementedException("unknown type:" + type);
            }
            artikel.Code = Convert.ToString(artikelrow["matnr"]);
            artikel.TimeStamp = ConvertSapDate(artikelrow["laeda"], "artikel:" + artikel.Code);

            #endregion artikeltype

            // verpakkingen = 0, al het andere 1 in gewicht
            artikel.PrijsGewichtNetto = artikel.GetType() == typeof(Domain.VerpakkingsArtikel) ? 0.0 : 1.0;

            #region artikel verpakking
            var verpakkingsql = @"
                -- Units of Measure for Material
                -- http://www.stechno.net/sap-tables.html?view=saptable&id=MARM
                SELECT *
                FROM MARM
                WHERE MARM.MANDT = :mandt
                AND MARM.MATNR = :matnr
                AND MARM.MEINH = :meins
            ";
            var meins = Convert.ToString(artikelrow["meins"]);
            var verpakkingrow = QueryRow(verpakkingsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", matnr }, { ":meins", meins } });
            //artikel.VerkoopGewichtEenheid = Convert.ToString(artikelrow["GEWEI"]);
            string verpakkingtype = Convert.ToString(verpakkingrow["MEINH"]);
            if (verpakkingtype == "DS")
            {
                verpakkingtype = "doos";
            }
            else if (verpakkingtype == "KG")
            {
                verpakkingtype = "kg";
            }
            else if (verpakkingtype == "ST")
            {
                verpakkingtype = "stuk";
            }
            else 
            {
                verpakkingtype = "kg";
            }
            artikel.VerkoopVerpakking  = verpakkingtype;
            artikel.VerkoopAantalNetto = Convert.ToDouble(artikelrow["NTGEW"]);
            artikel.VerkoopAantalBruto = Convert.ToDouble(artikelrow["BRGEW"]);
            artikel.HoudbaarheidInDagen = Convert.ToInt32(artikelrow["MHDHB"]);            

            //artikel.VerkoopEenheid = Convert.ToString(artikelrow["GEWEI"]);
            //artikel.VerpakkingEenheid = Convert.ToString(verpakkingrow["GEWEI"]);
            //artikel.VerpakkingBruto = Convert.ToDouble(verpakkingrow["BRGEW"]);
            //artikel.VerpakkingNetto = Convert.ToDouble(artikelrow["NTGEW"]);
            #endregion artikel verpakking

            #region artikel intrastat
            var intrastatsql = @"
                            -- INTRASTAT Receipt/Dispatch
                            -- http://www.stechno.net/sap-tables.html?view=saptable&id=VEIAV
                            SELECT *
                            FROM VEIAV
                            WHERE VEIAV.MANDT = :mandt1
                            AND VEIAV.MATNR = :matnr1
                            AND (DATUMJAHR * 1000000) + (DATUMMONA * 10000) + (ARRIVDEPA * 100) + LFDNRVEIA  = 
                            (
                                SELECT 
                                    MAX(
                                        (DATUMJAHR * 1000000) + (DATUMMONA * 10000) + (ARRIVDEPA * 100) + LFDNRVEIA
                                    )
                                FROM VEIAV
                                WHERE VEIAV.MANDT = :mandt2
                                AND VEIAV.MATNR = :matnr2
                            )                    
                        ";

            var intrastatrow = QueryRow(intrastatsql, new Dictionary<string, object>() { { ":mandt1", mandt }, { ":matnr1", matnr }, { ":mandt2", mandt }, { ":matnr2", matnr } });
            if (intrastatrow != null)
            {
                artikel.Intrastat = Convert.ToString(intrastatrow["STATWAREN"]);
            }
            #endregion artikel belasting

            #region artikel price
            // TODO: speciale subclasse sales article?
            if (artikel.GetType() != typeof(Domain.IngredientArtikel))
            {
                /*
                var descriptionsql = @"
                    -- Plant Data for Material
                    -- http://www.stechno.net/sap-tables.html?view=saptable&id=MARC
                    SELECT *
                    FROM MARC
                    WHERE MAKT.MANDT = :mandt
                    AND MAKT.MATNR = :matnr
                ";
                */
                var pricesql = @"
                        -- Material Valuation
                        -- http://www.stechno.net/sap-tables.html?view=saptable&id=MBEW
                        SELECT *
                        FROM MBEW
                        WHERE MBEW.MANDT = :mandt
                        AND MBEW.MATNR = :matnr
                        AND MBEW.BWKEY = :bwkey
                    ";

                var pricerow = QueryRow(pricesql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", matnr }, { ":bwkey", "0001" } });
                if (pricerow != null)
                {
                    artikel.PrijsKost = Convert.ToDouble(pricerow["stprs"]);
                }
                else
                {
                    artikel.PrijsKost = 0.0;
                    Output.Error("GEEN PRIJS VOOR:" + matnr);
                }
                // Fix de niet 1 problemen:
                // TODO: double check!!
                artikel.PrijsKost = artikel.PrijsKost / artikel.VerkoopAantalNetto;
                artikel.PrijsVerkoop = 0;
                if (artikel.GetType() == typeof(Domain.VerpakkingsArtikel))
                {
                    // verpakkingen in stuks, de rest in kg
                    artikel.PrijsEenheid = "stuks";
                }
                else
                {
                    artikel.PrijsEenheid = "kg";
                }
            }
            #endregion artikel price            

            #region artikel description
            var descriptionsql = @"
                -- Material Descriptions
                -- http://www.stechno.net/sap-tables.html?view=saptable&id=MAKT
                SELECT *
                FROM MAKT
                WHERE MAKT.MANDT = :mandt
                AND MAKT.MATNR = :matnr
            ";
            var descriptiontable = QueryTable(descriptionsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", matnr } });
            foreach (DataRow descriptionrow in descriptiontable.Rows)
            {
                string taal = Convert.ToString(descriptionrow["spras"]);
                string description = Convert.ToString(descriptionrow["maktx"]);

                if (artikel.Description == null || taal == "N")
                {
                    artikel.Description = description;
                }
                switch (taal) {
                    case "N":   // nederlands
                        artikel.Descriptions.Add(0, description);
                        break;
                    case "E":   // engels
                        artikel.Descriptions.Add(1, description);
                        break;
                    case "D":   // duits
                        artikel.Descriptions.Add(2, description);
                        break;
                    case "F":   // frans
                        artikel.Descriptions.Add(3, description);
                        break;
                    case "I":   // italiaans
                        // TODO: kan niet opgeslagen worden in exact!!
                        artikel.Descriptions.Add(4, description);
                        break;
                    default:
                        throw new NotImplementedException("unknown langues:" + taal);
                }
            }
            #endregion artikel description

            string output = "";
            for (int i = 0; i < ident; i++) output += "\t";
            output +=artikel.Code + " " + mtart;
            for (int i = 0; i < ident; i++) output +="\t";
            output +=" (" + artikel.Description + ")";
            Output.Info(output);


            #region child data
            if (typeof(Domain.BaseSamengesteldArtikel).IsAssignableFrom(artikel.GetType())) {
                artikel = ReadChildData(mandt, (Domain.BaseSamengesteldArtikel) artikel, ident);
            }
            #endregion child data

            return artikel;
        }

        private Domain.BaseSamengesteldArtikel ReadChildData(int  mandt, Domain.BaseSamengesteldArtikel artikel, int ident)
        {
            if (ident > 10)
            {
                System.Diagnostics.Debug.Assert(false);
                Output.Error("RECURSIEVE FOUT!!" + artikel.Code);
                return artikel;
            }
            // http://scn.sap.com/thread/75996
//            var bomsql = @"
//                -- Material to BOM Link
//                -- http://www.stechno.net/sap-tables.html?view=saptable&id=MAST
//
//                -- BOM Header
//                -- http://www.stechno.net/sap-tables.html?view=saptable&id=STKO
//
//                -- BOMs - Item Selection
//                -- http://www.stechno.net/sap-tables.html?view=saptable&id=STAS
//
//                -- BOM item
//                -- http://www.stechno.net/sap-tables.html?view=saptable&id=STPO
//                SELECT
//                    MAST.MATNR AS MAST_MATNR,
//                    MAST.STLNR AS MAST_STLNR,
//                    MAST.STLAL AS MAST_STLAL,    -- Alternative BOM
//
//                    STKO.STLTY AS STKO_STLTY,
//                    STKO.STLNR AS STKO_STLNR,
//                    STKO.STLAL AS STKO_STLAL,
//                    STKO.STKTX AS STKO_STKTX,
//                    STKO.DATUV AS STKO_DATUV,
//
//                    STAS.STLTY AS STAS_STLTY,
//                    STAS.STLNR AS STAS_STLNR,
//                    STAS.STLKN AS STAS_STLKN,
//    
//                    STPO.IDNRK AS STPO_IDNRK,
//                    STPO.POSNR AS STPO_POSNR,
//                    STPO.MENGE AS STPO_MENGE
//                FROM MAST
//                INNER JOIN STKO
//	                ON STKO.LKENZ = ''
//	                AND STKO.LOEKZ = ''
// 	                AND STKO.MANDT = MAST.MANDT 
//	                AND STKO.STLNR = MAST.STLNR
//	                AND STKO.STLAL = MAST.STLAL
//	                AND STKO.STKOZ NOT IN 
//	                (
//		                /* ALLEEN DE LAATSTE! */
//		                SELECT vorige_stko.VGKZL
//		                FROM STKO vorige_stko
//		                WHERE vorige_stko.STLNR = MAST.STLNR
//		                AND vorige_stko.STLAL = MAST.STLAL                    
//	                )
//                INNER JOIN STAS
//	                ON STAS.LKENZ = ''
//	                AND STAS.MANDT = STKO.MANDT 
//	                AND STAS.STLTY = STKO.STLTY
//	                AND STAS.STLNR = STKO.STLNR
// 	                AND STAS.STLAL = STKO.STLAL 
//                    /* STASZ = */
// 	                AND STAS.DATUV = 
//                    (
//                        /* ALLEEN DATUM HOOGSTE ! */
//		                SELECT MAX(laatste_stas.DATUV)
//		                FROM STAS laatste_stas
//    	                WHERE laatste_stas.LKENZ = ''
//	                    AND laatste_stas.MANDT = STKO.MANDT
//	                    AND laatste_stas.STLTY = STKO.STLTY
//	                    AND laatste_stas.STLNR = STKO.STLNR
// 	                    AND laatste_stas.STLAL = STKO.STLAL 
//                )
//                INNER JOIN STPO
//	                ON  STPO.LKENZ = ''
// 	                AND STPO.MANDT = STAS.MANDT 
//	                AND STPO.STLTY = STAS.STLTY
//	                AND STPO.STLNR = STAS.STLNR
//	                AND STPO.STLKN = STAS.STLKN
//	                AND STPO.STPOZ NOT IN 
//	                (
//		                /* ALLEEN DE LAATSTE! */
//		                SELECT vorige_stpo.VGPZL
//		                FROM STPO vorige_stpo
//		                WHERE vorige_stpo.STLTY = STAS.STLTY
//		                AND vorige_stpo.STLNR = STAS.STLNR
//	                )  
//                WHERE MAST.MANDT = :mandt
//                AND MAST.MATNR =  :matnr
//                ORDER BY MAST_STLAL, STPO_POSNR
//            ";
//            var bomsql = @"
//                -- Material to BOM Link
//                -- http://www.stechno.net/sap-tables.html?view=saptable&id=MAST
//                -- BOM Header
//                -- http://www.stechno.net/sap-tables.html?view=saptable&id=STKO
//                -- BOMs - Item Selection
//                -- http://www.stechno.net/sap-tables.html?view=saptable&id=STAS
//                -- BOM item
//                -- http://www.stechno.net/sap-tables.html?view=saptable&id=STPO
//                SELECT
//                    MAST.MATNR AS MAST_MATNR,
//                    MAST.STLNR AS MAST_STLNR,
//                    MAST.STLAL AS MAST_STLAL,    -- Alternative BOM
//
//                    STKO.STLTY AS STKO_STLTY,
//                    STKO.STLNR AS STKO_STLNR,
//                    STKO.STLAL AS STKO_STLAL,
//                    STKO.STKTX AS STKO_STKTX,
//                    STKO.DATUV AS STKO_DATUV,
//                    STKO.BMENG AS STKO_BMENG,
//                    
//                    STAS.STLTY AS STAS_STLTY,
//                    STAS.STLNR AS STAS_STLNR,
//                    STAS.STLKN    AS STAS_STLKN,
//                        
//                    STPO.IDNRK AS STPO_IDNRK,
//                    STPO.POSNR AS STPO_POSNR,
//                    STPO.MENGE AS STPO_MENGE
//                FROM MAST
//                INNER JOIN STKO
//	                ON STKO.LKENZ = ''
//	                AND STKO.LOEKZ = ''
// 	                AND STKO.MANDT = MAST.MANDT 
//	                AND STKO.STLNR = MAST.STLNR
//	                AND STKO.STLAL = MAST.STLAL
//	                AND STKO.STKOZ NOT IN 
//	                (
//		                /* ALLEEN DE LAATSTE! */
//		                SELECT vorige_stko.VGKZL
//		                FROM STKO vorige_stko
//		                WHERE vorige_stko.STLNR = MAST.STLNR
//		                AND vorige_stko.STLAL = MAST.STLAL                    
//	                )
//                 INNER JOIN STAS
//                    ON STAS.LKENZ = ''
//                    AND STAS.MANDT = STKO.MANDT 
//                    AND STAS.STLTY =STKO.STLTY
//                    AND STAS.STLNR = STKO.STLNR
//                    AND STAS.STLAL = STKO.STLAL
//                INNER JOIN STPO
//	                ON  STPO.LKENZ = ''
// 	                AND STPO.MANDT = STAS.MANDT 
//	                AND STPO.STLTY = STAS.STLTY
//	                AND STPO.STLNR = STAS.STLNR
//	                AND STPO.STLKN = STAS.STLKN                 
//	                AND STPO.STPOZ NOT IN 
//	                (
//		                /* ALLEEN DE LAATSTE! */
//		                SELECT vorige_stpo.VGPZL
//		                FROM STPO vorige_stpo
//		                WHERE vorige_stpo.STLTY = STKO.STLTY
//		                AND vorige_stpo.STLNR = STKO.STLNR
//	                )    
//                WHERE MAST.MANDT = :mandt
//                AND MAST.MATNR =  :matnr
//                AND STPO.AEDAT = '00000000'
//                ORDER BY MAST_STLAL, STPO_POSNR
//            ";
//            var bomsql = @"
//SELECT 
//    MAST.STLAN AS MAST_STLAN,
//    MAST.STLAL AS MAST_STLAL,
//    STKO.STKTX AS STKO_STKTX,
//    STKO.DATUV AS STKO_DATUV,
//    STKO.BMENG AS STKO_BMENG,
//    STPO.*
//FROM MAST
//JOIN STKO 
//    ON STKO.MANDT  = MAST.MANDT
//    AND STKO.STLNR = MAST.STLNR
//    AND STKO.STLAL = MAST.STLAL
//    AND STKO.LKENZ = ''
//    AND STKO.LOEKZ = ''
//    AND NOT STKO.STKOZ  IN
//    (
//        SELECT PREV_STKO.VGKZL
//        FROM STKO PREV_STKO
//        WHERE PREV_STKO.MANDT = MAST.MANDT 
//        AND PREV_STKO.STLNR= MAST.STLNR        
//    )
//JOIN STAS
//    ON STAS.MANDT  = MAST.MANDT
//    AND STAS.STLNR = MAST.STLNR
//    AND STAS.STLAL = MAST.STLAL
//JOIN STPO
//    ON STPO.MANDT = MAST.MANDT 
//    AND STPO.STLNR= MAST.STLNR
//    AND STPO.STLKN = STAS.STLKN
//    AND STPO.LKENZ = ''
//    AND NOT STPO.STPOZ IN
//    (
//        SELECT PREV_STPO.VGPZL
//        FROM STPO PREV_STPO
//        WHERE PREV_STPO.MANDT = MAST.MANDT 
//        AND PREV_STPO.STLNR= MAST.STLNR        
//    )
//WHERE MAST.MANDT= :mandt
//AND MAST.WERKS= '0001'
//AND MAST.MATNR = :matnr
//ORDER BY MAST.STLAN, MAST.STLAL, STPO.POSNR
//            ";
//            var bomsql = @"
//SELECT 
//    MAST.STLAN AS MAST_STLAN,
//    MAST.STLAL AS MAST_STLAL,
//    MAST.STLNR AS MAST_STLNR,
//    STKO.STKTX AS STKO_STKTX,
//    STKO.DATUV AS STKO_DATUV,
//    STKO.BMENG AS STKO_BMENG,
//    STAS.AEDAT AS STAS_AEDAT,
//    STPO.AEDAT AS STPO_AEDAT,
//    STPO.MENGE AS STPO_MENGE,
//    STPO.POSNR AS STPO_POSNR,
//    STPO.IDNRK AS STPO_IDNRK
//FROM MAST
//JOIN STKO 
//    ON STKO.MANDT  = MAST.MANDT
//    AND STKO.STLNR = MAST.STLNR
//    AND STKO.STLAL = MAST.STLAL
//    AND STKO.LKENZ = ''
//    AND STKO.LOEKZ = ''
//    AND NOT STKO.STKOZ  IN
//    (
//        SELECT PREV_STKO.VGKZL
//        FROM STKO PREV_STKO
//        WHERE PREV_STKO.MANDT = MAST.MANDT 
//        AND PREV_STKO.STLNR= MAST.STLNR        
//    )
//JOIN STAS
//    ON STAS.MANDT = MAST.MANDT
//    AND STAS.STLNR= MAST.STLNR
//    AND STAS.STLAL = MAST.STLAL
//    AND STAS.LKENZ = ''
//JOIN STPO
//    ON STPO.MANDT = MAST.MANDT 
//    AND STPO.STLNR= MAST.STLNR
//    AND STPO.LKENZ = ''
//    AND NOT STPO.STPOZ IN
//    (
//        SELECT PREV_STPO.VGPZL
//        FROM STPO PREV_STPO
//        WHERE PREV_STPO.MANDT = MAST.MANDT 
//        AND PREV_STPO.STLNR= MAST.STLNR        
//    )
//    AND  STPO.STLKN = STAS.STLKN
//    AND  
//    (
//        STPO.AEDAT = '00000000'
//        OR '00000000' NOT IN 
//        (
//            SELECT AEDAT_STPO.AEDAT
//            FROM STPO AEDAT_STPO
//            WHERE AEDAT_STPO.MANDT = MAST.MANDT 
//            AND AEDAT_STPO.STLNR= MAST.STLNR                
//        ) 
//    )
//WHERE MAST.MANDT= :mandt
//AND MAST.WERKS= '0001'
//AND MAST.MATNR = :matnr
//ORDER BY MAST.STLAN, MAST.STLAL, STPO.POSNR
//            ";
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
    STPO.POSNR AS STPO_POSNR,
    STPO.IDNRK AS STPO_IDNRK,
    '------',
    STKO.LKENZ AS STKO_LKENZ,
    STKO.STKOZ AS STKO_STKOZ,
    STAS.LKENZ AS STAS_LKENZ,
    STAS.STASZ AS STAS_STASZ,
    STPO.LKENZ  AS STPO_LKENZ,
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
WHERE MAST.MANDT= :mandt
AND MAST.WERKS= '0001'
AND MAST.MATNR = :matnr
ORDER BY MAST.STLAN, MAST.STLAL, STPO.POSNR";
            /*
            '01010D15' = 10 t/m 50
            '33024D13' = 10 t/m 60
            '33024X99' = 10 t/m 110
            '42760X99' = 40 t/m 90
            '81110X99' = 170 t/m 300
            '42760X99' moet hebben GR226150
            '01010D15' moet hebben EM15Z610 
            */


            //            Export2Excel(@"C:\exact importeren artikelen\export\stuklijst-" + artikel.Code + ".xls", bomsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", artikel.Code } });
            var bomtable = QueryTable(bomsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", artikel.Code } });

            Domain.Stuklijst stuklijst = null;
            foreach (DataRow bomrow in bomtable.Rows)
            {
                System.Diagnostics.Debug.Assert(bomrow["STKO_LKENZ"].ToString() == "");
                System.Diagnostics.Debug.Assert(bomrow["STAS_LKENZ"].ToString() == "");
                System.Diagnostics.Debug.Assert(bomrow["STPO_LKENZ"].ToString() == "");

                int stuklijstcode = Convert.ToInt32(bomrow["mast_stlal"]);
                string stuklijstnaam = Convert.ToString(bomrow["stko_stktx"]);                
                if (stuklijst == null || stuklijst.StuklijstVersion != stuklijstcode)
                {
                    if (stuklijst != null && stuklijst.StuklijstRegels.Count > 0)
                    {
                        artikel.Stuklijsten.Add(stuklijst);
                    }
                    stuklijst = new Domain.Stuklijst();
                    stuklijst.StuklijstVersion = stuklijstcode;
                    stuklijst.StuklijstNaam = stuklijstnaam;
                    stuklijst.StuklijstTotaalAantal = -1;

                    stuklijst.StuklijstDatum = ConvertSapDate(bomrow["STKO_DATUV"], "artikel:" + artikel.Code + " stuklijstcode:" + stuklijstcode);
                    stuklijst.StuklijstTotaalAantal = Convert.ToDouble(bomrow["STKO_BMENG"]);
                }

                string matnr = Convert.ToString(bomrow["STPO_idnrk"]);
                if (matnr.Length > 0)
                {                    
                    var receptuurregel = new Domain.StuklijstRegel();
                    var posnr = Convert.ToString(bomrow["STPO_posnr"]);
                    int found = 0;
                    if (!Int32.TryParse(posnr, out found))
                    {
                        foreach(char c in posnr.ToCharArray()) {
                            if(char.IsDigit(c)) {
                                found = found * 10;
                                found += Convert.ToInt32(c.ToString());
                            }
                        }
                        Output.Error("invalid pos-nr in arikel:" + artikel.Code + " value: " + posnr + " assuming:" + found);
                    }
                    receptuurregel.Volgnummer = found;
                    receptuurregel.ReceptuurRegelAantal = Convert.ToDouble(bomrow["STPO_menge"]);

                    Domain.BaseArtikel childartikel = data.Retrieve(matnr);
                    if (childartikel == null)
                    {
                        childartikel = ReadArtikelData(mandt, matnr, ident + 1);
                        if (data.Retrieve(matnr) == null)
                        {
                            data.Add(childartikel);
                        }
                        else
                        {
                            Output.Error("Recursive usage of article:" + matnr + " in samengesteld-artikel:" + artikel.Code);
                        }
                    }
                    // geen ingredienten meenemen!
                    if (childartikel.GetType() != typeof(Domain.IngredientArtikel))
                    {
                        receptuurregel.Artikel = childartikel;
                        //TODO: HACK HACK NET LINE!!!!
                        stuklijst.StuklijstRegelsAdd(receptuurregel);
                    }
                }
            }
            if (stuklijst != null && stuklijst.StuklijstRegels.Count > 0)
            {
                artikel.Stuklijsten.Add(stuklijst);
            }
            

            // calculeer de stuklijst totaal aantallen
            // TODO: niet in SAP?
            foreach(Domain.Stuklijst sl in artikel.Stuklijsten) {
                double totaalaantal = 0;
                foreach(Domain.StuklijstRegel slr in sl.StuklijstRegels) {
                    double aantal = slr.ReceptuurRegelAantal * slr.Artikel.PrijsGewichtNetto;
                    totaalaantal += aantal;
                }
                double berekendaantal = sl.StuklijstTotaalAantal * artikel.VerkoopAantalNetto;
                double factortoegelaten =  0.001;    //  0.1%
                if (
                    totaalaantal > (berekendaantal + (berekendaantal * factortoegelaten)) 
                    ||
                    totaalaantal < (berekendaantal - (berekendaantal * factortoegelaten))
                    )
                {
                    Output.Error("Ongeldig stuklijst totaal voor:" + artikel.Code + " verwacht: " + totaalaantal + " berekend:" + totaalaantal + " x " + artikel.VerkoopAantalNetto + " = " + berekendaantal);
                }
                sl.StuklijstTotaalAantal = totaalaantal;
            }
            return artikel;
        }

        private void WriteError(string p)
        {
            throw new NotImplementedException();
        }
    }
}
