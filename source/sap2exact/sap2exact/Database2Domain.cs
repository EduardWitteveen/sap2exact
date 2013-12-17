using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact
{
    public class Database2Domain
    {
        private Domain.ExportData data;

        private MaxDB.Data.MaxDBConnection sapconnection;
        private sap2exact.SapSDK.SDK sdk;

        public Database2Domain(MaxDB.Data.MaxDBConnection sapconnection, sap2exact.SapSDK.SDK sdk)
        {
            // TODO: Complete member initialization
            this.sapconnection = sapconnection;
            this.sdk = sdk;
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
            /*
            General Material Data     MARA
            Material Descriptions     MAKT
            Plant Data for Material     MARC
            Material Valuation     MBEW
            Storage Location Data for Material     MARD
            Units of Measure for Material     MARM
            Sales Data for Material     MVKE
            Forecast Parameters     MPOP
            Planning Data     MPGD_MASS
            Tax Classification for Material     MLAN
            Material Data for Each Warehouse Number     MLGN
            Material Data for Each Storage Type     MLGT
             */

            data = new Domain.ExportData();
            if (artikelnummer == null)
            {
                var artikelsql = @"
SELECT 
    MANDT,
    MATNR,
    MTART,
    MAX(LAATSTEDATUM) AS LAATSTEDATUM
FROM
(
    SELECT 
        MARA.MANDT,
        MARA.MATNR,
        MARA.MTART,
        MAX(AUFK.ERDAT) AS LAATSTEDATUM
    FROM MARA
    INNER JOIN AFPO
        ON AFPO.MATNR = MARA.MATNR
        AND AFPO.MANDT  = MARA.MANDT
    INNER JOIN AUFK
        ON AUFK.AUFNR = AFPO.AUFNR
        AND AUFK.MANDT  = AFPO.MANDT 
    WHERE NOT MARA.MATKL = 'VERVALLEN' 
    AND NOT MARA.MTART IN 
    (
        'HALB',
        'ZROH',
        'ROH',
        'INGR',
        'VERP',
        'LEER'
    )
    AND
    (
        AUFK.ERDAT >= 20120000 
        OR 
        AUFK.AEDAT >= 20120000 
    ) 
    AND MARA.MANDT = 100
    GROUP BY MARA.MANDT, MARA.MATNR, MARA.MTART
    UNION
    SELECT
        MARA.MANDT,
        MARA.MATNR,
        MARA.MTART,
        MAX(S031.SPMON) * 100 AS LAATSTEDATUM
    FROM MARA
    INNER JOIN S031
        ON S031.MATNR = MARA.MATNR
        AND S031.MANDT  = MARA.MANDT
    WHERE NOT MARA.MATKL = 'VERVALLEN' 
    AND NOT MARA.MTART IN 
    (
        'HALB',
        'ZROH',
        'ROH',
        'INGR',
        'VERP',
        'LEER'
    )
    AND S031.SPMON >= 201200
    AND MARA.MANDT = 100
    GROUP BY MARA.MANDT,  MARA.MATNR,  MARA.MTART
) RECENT
GROUP BY MANDT,  MATNR,  MTART
                ";
                var artikeltable = QueryTable(artikelsql);
                var startTime = DateTime.Now;
                foreach (DataRow artikelrow in artikeltable.Rows)
                {
                    int matdt = Convert.ToInt32(artikelrow["mandt"]);
                    string matnr = Convert.ToString(artikelrow["matnr"]);

                    int huidigeregel = artikeltable.Rows.IndexOf(artikelrow) + 1;   // +1   ==>   x/0  :-)
                    int totaalregels = artikeltable.Rows.Count;
                    var expected = TimeSpan.FromTicks((DateTime.Now.Subtract(startTime).Ticks / huidigeregel * (totaalregels - huidigeregel)));
                    var totaal = TimeSpan.FromTicks((DateTime.Now.Subtract(startTime).Ticks / huidigeregel * totaalregels));
                    Output.Info("[ " + (int)((100.0 / totaalregels) * huidigeregel) + "%  EXPECTED: " + expected + " TOTAL: " + totaal + " ETA: " + DateTime.Now.Add(expected) + "]");

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
            if (artikelrow == null)
            {
                throw new Exception("artikel:" + matnr + " is niet gevonden!");
            }
            var mtart = artikelrow["mtart"].ToString();
            switch (mtart)
            {
                case "FERT":
                case "HAWA":
                case "HIBE":
                    artikel = new Domain.EindArtikel();
                    #region artikel belasting
                    var belastingsql = @"
                            -- Tax Classification for Material
                            -- http://www.stechno.net/sap-tables.html?view=saptable&id=MLAN
                            SELECT *
                            FROM MLAN
                            WHERE MLAN.MANDT = :mandt
                            AND MLAN.MATNR = :matnr
                            AND NOT MLAN.TAXM1 = ''
                        ";
                    var belastingrow = QueryRow(belastingsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", matnr } });
                    if (belastingrow != null)
                    {
                        Debug.Assert("NL" == Convert.ToString(belastingrow["aland"]));
                        var taxm1 = Convert.ToInt32(belastingrow["taxm1"]);
                        artikel.ExactGewensteBelastingCategorie = taxm1;  //wat betekend wat?
                    }
                    else
                    {
                        Output.Error("NO TAX FOR ARTICLE:" + matnr);
                    }

                    #endregion artikel belasting                    
                    break;
                case "HALB":
                    artikel = new Domain.ReceptuurArtikel();
                    artikel.ExactGewensteBelastingCategorie = 2;
                    break;
                case "ZROH":
                case "ROH":
                    artikel = new Domain.GrondstofArtikel();
                    artikel.ExactGewensteBelastingCategorie = 2;
                    break;
                case "INGR":
                    artikel = new Domain.IngredientArtikel();
                    artikel.ExactGewensteBelastingCategorie = 2;
                    break;
                case "VERP":
                case "LEER":
                    // https://help.sap.com/saphelp_45b/helpdata/en/ff/515afd49d811d182b80000e829fbfe/content.htm
                    artikel = new Domain.VerpakkingsArtikel();
                    artikel.ExactGewensteBelastingCategorie = 4;
                    break;
                default:
                    //string type = Convert.ToString(artikelrow["bom_artikelsoort"]);
                    throw new NotImplementedException("unknown type:" + mtart);
            }
            artikel.MateriaalCode = Convert.ToString(artikelrow["matnr"]);
            artikel.TimeStamp = ConvertSapDate(artikelrow["laeda"], "artikel:" + artikel.MateriaalCode);

            #endregion artikeltype

            // verpakkingen = 0, al het andere 1 in gewicht
            artikel.ExactGewensteNettoGewicht = artikel.GetType() == typeof(Domain.VerpakkingsArtikel) ? 0.0 : 1.0;

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
            artikel.BasishoeveelheidEenheid = Convert.ToString(verpakkingrow["MEINH"]);
            artikel.NettoGewicht = Convert.ToDouble(artikelrow["NTGEW"]);
            artikel.BruttoGewicht = Convert.ToDouble(artikelrow["BRGEW"]);
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
                    artikel.KostPrijs = Convert.ToDouble(pricerow["stprs"]);
                }
                else
                {
                    artikel.KostPrijs = 0.0;
                    Output.Error("GEEN PRIJS VOOR:" + matnr);
                }
                // SAP2EXACT: aantalconversie
                if (artikel.GetType() == typeof(Domain.VerpakkingsArtikel))
                {
                    artikel.KostPrijs = artikel.KostPrijs / artikel.NettoGewicht;
                }
                else
                {
                    artikel.KostPrijs = artikel.KostPrijs / artikel.NettoGewicht;
                }
                artikel.Gewichtseenheid = Convert.ToString(artikelrow["GEWEI"]);
            }
            #endregion artikel price            

            #region artikel description
            var descriptionsql = @"
                -- Material ArtikelOmschrijvingen
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

                if (artikel.ArtikelOmschrijving == null || taal == "N")
                {
                    artikel.ArtikelOmschrijving = description;
                }
                switch (taal) {
                    case "N":   // nederlands
                        artikel.ArtikelOmschrijvingen.Add(0, description);
                        break;
                    case "E":   // engels
                        artikel.ArtikelOmschrijvingen.Add(1, description);
                        break;
                    case "D":   // duits
                        artikel.ArtikelOmschrijvingen.Add(2, description);
                        break;
                    case "F":   // frans
                        artikel.ArtikelOmschrijvingen.Add(3, description);
                        break;
                    case "I":   // italiaans
                        // TODO: kan niet opgeslagen worden in exact!!
                        artikel.ArtikelOmschrijvingen.Add(4, description);
                        break;
                    default:
                        throw new NotImplementedException("unknown langues:" + taal);
                }
            }
            #endregion artikel description

            #region artikel eenheid
            var eenheidsql = @"
                SELECT 
                    UMREN,
                    MEINH,
                    UMREZ,
                    BRGEW,
                    GEWEI
                FROM MARM
                WHERE MANDT = :mandt
                AND MATNR = :matnr
            ";
            var eenheidtable = QueryTable(eenheidsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", matnr } });
            foreach (DataRow eenheidrow in eenheidtable.Rows)
            {
                Domain.HoeveelheidsEenheid he = new Domain.HoeveelheidsEenheid();
                he.naarEenheid = Convert.ToString(eenheidrow["MEINH"]).ToUpper();
                he.vanEenheid = Convert.ToString(verpakkingrow["GEWEI"]).ToUpper();
                he.factor = Convert.ToDouble(eenheidrow["UMREZ"]) / Convert.ToDouble(eenheidrow["UMREN"]);
    
                artikel.HoeveelheidsEenheden.Add(he);
            }

            #endregion artikel eenheid



            string output = "";
            for (int i = 0; i < ident; i++) output += "\t";
            output +=artikel.MateriaalCode + " " + mtart;
            for (int i = 0; i < ident; i++) output +="\t";
            output +=" (" + artikel.ArtikelOmschrijving + ")";
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
                Output.Error("RECURSIEVE FOUT!!" + artikel.MateriaalCode);
                return artikel;
            }
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


//            Export2Excel(@"C:\exact importeren artikelen\export\stuklijst-" + artikel.MateriaalCode + ".xls", bomsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", artikel.MateriaalCode } });
            var bomtable = QueryTable(bomsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", artikel.MateriaalCode } });

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

                    stuklijst.StuklijstDatum = ConvertSapDate(bomrow["STKO_DATUV"], "artikel:" + artikel.MateriaalCode + " stuklijstcode:" + stuklijstcode);
                    stuklijst.StuklijstTotaalAantal = Convert.ToDouble(bomrow["STKO_BMENG"]);
                }

                var matnr = Convert.ToString(bomrow["STPO_idnrk"]);
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
                    Output.Error("invalid pos-nr in arikel:" + artikel.MateriaalCode + " value: " + posnr + " assuming:" + found);
                }
                foreach(Domain.StuklijstRegel slr in stuklijst.StuklijstRegels) {
                    if (slr.Volgnummer == found)
                    {
                        Output.Error("invalid pos-nr in arikel:" + artikel.MateriaalCode + " regel#: " + slr.Volgnummer + " for: " + slr.Artikel.MateriaalCode + " on same line as : " + matnr);
                        found++;
                    }
                }
                receptuurregel.Volgnummer = found;
                Domain.BaseArtikel childartikel = null;
                if (matnr.Length == 0)
                {
                    // textregel
                    string text = sdk.GetLongText(
                            Convert.ToString(bomrow["STPO_MANDT"]),
                            Convert.ToString(bomrow["STPO_STLTY"]),
                            Convert.ToString(bomrow["STPO_STLNR"]),
                            Convert.ToString(bomrow["STPO_STLKN"]),
                            Convert.ToString(bomrow["STPO_STPOZ"])
                    );
                    childartikel = new Domain.TekstArtikel(text);
                }
                else
                {
                    childartikel = data.Retrieve(matnr);
                }
                if (childartikel == null)
                {
                    childartikel = ReadArtikelData(mandt, matnr, ident + 1);
                    if (data.Retrieve(matnr) == null)
                    {
                        data.Add(childartikel);
                    }
                    else
                    {
                        Output.Error("Recursive usage of article:" + matnr + " in samengesteld-artikel:" + artikel.MateriaalCode);
                    }
                }
                receptuurregel.ReceptuurRegelAantal = Convert.ToDouble(bomrow["STPO_menge"]);
                receptuurregel.ReceptuurEenheid = Convert.ToString(bomrow["STPO_meins"]);
                receptuurregel.ReceptuurSortBegrip = Convert.ToString(bomrow["STPO_SORTF"]);

                if (childartikel.GetType() != typeof(Domain.TekstArtikel))
                {
                    // SAP2EXACT: aantalconversie
                    receptuurregel.ReceptuurEenheidFactor =
                        childartikel.ConversieFactor(childartikel.Gewichtseenheid, receptuurregel.ReceptuurEenheid);
                    receptuurregel.ReceptuurEenheidConversie =
                        "van:" + childartikel.Gewichtseenheid + " naar:" + receptuurregel.ReceptuurEenheidFactor + " factor:" + receptuurregel.ReceptuurEenheidFactor;
                }

                // geen ingredientenmeenemen!
                if (childartikel.GetType() != typeof(Domain.IngredientArtikel))
                {
                    if (childartikel.GetType() == typeof(Domain.TekstArtikel))
                    {
                        // tekstregel
                        receptuurregel.Artikel = childartikel;
                        stuklijst.StuklijstRegels.Add(receptuurregel);
                    }
                    else if (receptuurregel.ReceptuurEenheidFactor == 1)
                    {
                        // gewoon artikel
                        receptuurregel.Artikel = childartikel;
                        stuklijst.StuklijstRegels.Add(receptuurregel);
                    }
                    else if (receptuurregel.ReceptuurEenheidFactor > 1)
                    {
                        // artikel met afval
                        receptuurregel.Artikel = childartikel;
                        stuklijst.StuklijstRegels.Add(receptuurregel);
                        stuklijst.StuklijstRegels.Add(Domain.AfvalArtikel.CreateStuklijstRegel(receptuurregel, receptuurregel.ReceptuurEenheidFactor));
                    }
                    else if (receptuurregel.ReceptuurEenheidFactor < 1)
                    {
                        // artikel met spoelwater
                        if (receptuurregel.ReceptuurEenheid == "KN")
                        {
                            // maak hiervoor een PhantomArikel aan
                            // Dit moet wel een KN zijn, dus spoelwater ontbreekt hierin!
                            var phantomartikel = new Domain.PhantomArtikel(childartikel, receptuurregel.ReceptuurEenheidFactor);                            
                            if (data.AlleArtikelen.ContainsKey(phantomartikel.MateriaalCode))
                            {
                                Domain.PhantomArtikel pa = (Domain.PhantomArtikel)data.AlleArtikelen[phantomartikel.MateriaalCode];
                                phantomartikel = pa;
                            }
                            else
                            {
                                data.Add(phantomartikel);
                            }
                            receptuurregel.Artikel = phantomartikel;
                            stuklijst.StuklijstRegels.Add(receptuurregel);
                        }
                        else
                        {
                            Output.Error("Factor fout in artikel:" + artikel.MateriaalCode + " voor: " + childartikel.MateriaalCode + ", we negeren de factor:" + receptuurregel.ReceptuurEenheidFactor);
                            receptuurregel.Artikel = childartikel;
                            stuklijst.StuklijstRegels.Add(receptuurregel);
                        }
                    }
                    else
                    {
                        Output.Error("Deze code mag nooit bereikt worden: ReceptuurEenheidFactor:" + receptuurregel.ReceptuurEenheidFactor);
                        throw new NotImplementedException("Deze code mag nooit bereikt worden: ReceptuurEenheidFactor:" + receptuurregel.ReceptuurEenheidFactor);
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
                    if (slr.Artikel.GetType() != typeof(Domain.AfvalArtikel))
                    {
                        double aantal = slr.ReceptuurRegelAantal * slr.Artikel.ExactGewensteNettoGewicht;
                        totaalaantal += aantal;
                    }
                }
                double berekendaantal = sl.StuklijstTotaalAantal * artikel.NettoGewicht;
                double factortoegelaten =  0.001;    //  0.1%
                if (
                    totaalaantal > (berekendaantal + (berekendaantal * factortoegelaten)) 
                    ||
                    totaalaantal < (berekendaantal - (berekendaantal * factortoegelaten))
                    )
                {
                    Output.Error("Ongeldig stuklijst totaal voor: " + artikel.MateriaalCode + " berekend: " + totaalaantal + " verwacht: " + sl.StuklijstTotaalAantal + " x " + artikel.NettoGewicht + " = " + berekendaantal);
                }
                sl.StuklijstTotaalAantal = totaalaantal;
                
                // ook nog een goed nummer geven.....
                int versienummer = 1;
                do
                {
                    foreach (Domain.Stuklijst slvergelijk in artikel.Stuklijsten)
                    {
                        if (slvergelijk != sl)
                        {
                            if (versienummer == slvergelijk.StuklijstVersion)
                            {
                                versienummer++;
                                continue;
                            }
                        }
                    }
                    sl.StuklijstVersion = versienummer;
                    break;
                }
#pragma warning disable
                while (true);
#pragma warning restore
            }
            return artikel;
        }
    }
}
