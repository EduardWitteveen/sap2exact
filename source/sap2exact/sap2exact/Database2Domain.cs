﻿using System;
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
        /*
        const string OPVULLING = "SAP________";
        static string CreateItemCode(string nummer)
        {
            while (nummer[0] == '0') nummer = nummer.Substring(1);
            return OPVULLING.Substring(0, 12 - nummer.Trim().Length - 1) + "_" + nummer.Trim();
        }
        static string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        static int CreateSeqeuenceNumber(string nummer)
        {
            string result = "";
            foreach (char c in nummer)
            {
                if (Char.IsNumber(c)) result += c;
            }
            return Int32.Parse(result);
        }
        */
        private MaxDB.Data.MaxDBConnection sapconnection;

        public Database2Domain(MaxDB.Data.MaxDBConnection sapconnection)
        {
            // TODO: Complete member initialization
            this.sapconnection = sapconnection;
        }

        private DateTime ConvertSapDate(object value)
        {
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
            /*
                    var sapbomcommand = new MaxDB.Data.MaxDBCommand(queries.EmbeddedResource.GetString("queries.stuklijst.sql"), sapconnection);
                    sapbomcommand.CommandType = System.Data.CommandType.Text;
                    sapbomcommand.Parameters.Add(new MaxDB.Data.MaxDBParameter(":artikelnummer", artikelcode));
                    var sapbomadapter = new MaxDB.Data.MaxDBDataAdapter(sapbomcommand);
                    var sapbomtable = new DataTable();
                    sapbomadapter.Fill(sapbomtable);

             */
            //var reader = cmd.ExecuteReader();
            //var table = new DataTable();
            //table.Load(reader);
            var adapter = new MaxDB.Data.MaxDBDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public Domain.ExportData ReadAllArtikelData()
        {
            data = new Domain.ExportData();

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
                Console.Out.WriteLine("[ " + (int) ((100.0 / totaalregels) * huidigeregel)  + "% ] START:" + matnr);

                data.Add(ReadArtikelData(matdt, matnr, 1));
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
                    Debug.Assert("NL" == Convert.ToString(belastingrow["aland"]));
                    var taxm1 = Convert.ToInt32(belastingrow["taxm1"]);
                    artikel.PrijsBelastingCategorie = taxm1;  //wat betekend wat?
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
                    artikel = new Domain.VerpakkingsArtikel();
                    artikel.PrijsBelastingCategorie = 4;
                    break;
                default:
                    string type = Convert.ToString(artikelrow["bom_artikelsoort"]);
                    throw new NotImplementedException("unknown type:" + type);
            }
            artikel.Code = Convert.ToString(artikelrow["matnr"]);
            string timestamp = Convert.ToString(artikelrow["laeda"]);
            artikel.TimeStamp = timestamp.Substring(0, 4) + "-" + timestamp.Substring(4, 2) + "-" + timestamp.Substring(6, 2);

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
                artikel.PrijsKost = Convert.ToDouble(pricerow["stprs"]);
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

            for (int i = 0; i < ident; i++) Console.Out.Write("\t");
            Console.Out.WriteLine(artikel.Code + " " + mtart);
            for (int i = 0; i < ident; i++) Console.Out.Write("\t");
            Console.Out.WriteLine(" (" + artikel.Description + ")");

            #region child data
            if (typeof(Domain.BaseSamengesteldArtikel).IsAssignableFrom(artikel.GetType())) {
                artikel = ReadChildData(mandt, (Domain.BaseSamengesteldArtikel) artikel, ident);
            }
            #endregion child data

            return artikel;
        }

        private Domain.BaseSamengesteldArtikel ReadChildData(int  mandt, Domain.BaseSamengesteldArtikel artikel, int ident)
        {
            var bomsql = @"
                -- Material to BOM Link
                -- http://www.stechno.net/sap-tables.html?view=saptable&id=MAST

                -- BOM Header
                -- http://www.stechno.net/sap-tables.html?view=saptable&id=STKO

                -- BOMs - Item Selection
                -- http://www.stechno.net/sap-tables.html?view=saptable&id=STAS

                -- BOM item
                -- http://www.stechno.net/sap-tables.html?view=saptable&id=STPO
                SELECT
                    MAST.MATNR AS MAST_MATNR,
                    MAST.STLNR AS MAST_STLNR,
                    MAST.STLAL AS MAST_STLAL,    -- Alternative BOM

                    STKO.STLTY AS STKO_STLTY,
                    STKO.STLNR AS STKO_STLNR,
                    STKO.STLAL AS STKO_STLAL,
                    STKO.STKTX AS STKO_STKTX,
                    STKO.DATUV AS STKO_DATUV,

                    STAS.STLTY AS STAS_STLTY,
                    STAS.STLNR AS STAS_STLNR,
                    STAS.STLKN AS STAS_STLKN,
    
                    STPO.IDNRK AS STPO_IDNRK,
                    STPO.POSNR AS STPO_POSNR,
                    STPO.MENGE AS STPO_MENGE
                FROM MAST
                INNER JOIN STKO
	                ON STKO.LKENZ = ''
	                AND STKO.LOEKZ = ''
 	                AND STKO.MANDT = MAST.MANDT 
	                AND STKO.STLNR = MAST.STLNR
	                AND STKO.STLAL = MAST.STLAL
	                AND STKO.STKOZ NOT IN 
	                (
		                /* ALLEEN DE LAATSTE! */
		                SELECT vorige_stko.VGKZL
		                FROM STKO vorige_stko
		                WHERE vorige_stko.STLNR = MAST.STLNR
		                AND vorige_stko.STLAL = MAST.STLAL                    
	                )
                INNER JOIN STAS
	                ON STAS.LKENZ = ''
	                AND STAS.MANDT = STKO.MANDT 
	                AND STAS.STLTY = STKO.STLTY
	                AND STAS.STLNR = STKO.STLNR
 	                AND STAS.STLAL = STKO.STLAL 
                    /* STASZ = */
                INNER JOIN STPO
	                ON  STPO.LKENZ = ''
 	                AND STPO.MANDT = STAS.MANDT 
	                AND STPO.STLTY = STAS.STLTY
	                AND STPO.STLNR = STAS.STLNR
	                AND STPO.STLKN = STAS.STLKN
	                AND STPO.STPOZ NOT IN 
	                (
		                /* ALLEEN DE LAATSTE! */
		                SELECT vorige_stpo.VGPZL
		                FROM STPO vorige_stpo
		                WHERE vorige_stpo.STLTY = STAS.STLTY
		                AND vorige_stpo.STLNR = STAS.STLNR
	                )  
                WHERE MAST.MANDT = :mandt
                AND MAST.MATNR =  :matnr
                ORDER BY MAST_STLAL, STPO_POSNR
            ";
            var bomtable = QueryTable(bomsql, new Dictionary<string, object>() { { ":mandt", mandt }, { ":matnr", artikel.Code } });

            Domain.Stuklijst stuklijst = null;
            foreach (DataRow bomrow in bomtable.Rows)
            {
                int stuklijstcode = Convert.ToInt32(bomrow["mast_stlal"]);
                string stuklijstnaam = Convert.ToString(bomrow["stko_stktx"]);                
                if (stuklijst == null || stuklijst.StuklijstVersion != stuklijstcode)
                {
                    stuklijst = new Domain.Stuklijst();
                    stuklijst.StuklijstVersion = stuklijstcode;
                    stuklijst.StuklijstNaam = stuklijstnaam;
                    stuklijst.StuklijstTotaalAantal = -1;

                    stuklijst.StuklijstDatum = ConvertSapDate(bomrow["STKO_DATUV"]);
                    artikel.Stuklijsten.Add(stuklijst);
                }

                string matnr = Convert.ToString(bomrow["stpo_idnrk"]);
                if (matnr.Length > 0)
                {
                    var receptuurregel = new Domain.StuklijstRegel();
                    receptuurregel.Volgnummer = Convert.ToInt32(bomrow["stpo_posnr"]);
                    receptuurregel.ReceptuurRegelAantal = Convert.ToDouble(bomrow["stpo_menge"]);
                    Domain.BaseArtikel childartikel = ReadArtikelData(mandt, matnr, ident + 1);
                    // geen ingredienten meenemen!
                    if (childartikel.GetType() != typeof(Domain.IngredientArtikel))
                    {
                        receptuurregel.Artikel = childartikel;
                        data.Add(childartikel);
                        stuklijst.StuklijstRegels.Add(receptuurregel);
                    }
                }
            }    
            // calculeer de stuklijst totaal aantallen
            // TODO: niet in SAP?
            foreach(Domain.Stuklijst sl in artikel.Stuklijsten) {
                sl.StuklijstTotaalAantal = 0;
                foreach(Domain.StuklijstRegel slr in sl.StuklijstRegels) {
                    double aantal = slr.ReceptuurRegelAantal * slr.Artikel.PrijsGewichtNetto;
                    sl.StuklijstTotaalAantal += aantal;
                }
            }        

            return artikel;
        }
    }
}
