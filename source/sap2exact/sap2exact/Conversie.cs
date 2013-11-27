using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact
{

    public class Conversie
    {
        public class ArtikelGroepInformatie
        {
            public String Omschrijving;
            public String BasisArtikelgroepCode;
            public String BasisArtikelgroepOmschrijving;
            public String BasisProductgroepCode;
            public String BasisProductgroepOmschrijving;
            public String BasisSoortartikelCode;
            public String BasisSoortartikelOmschrijving;

            public String FinancieleArtikelgroepNummer;
            public String FinancieleArtikelgroepCode;

            public String FinancieleOmzet;
            public String FinancieleVoorraad;	
            public String FinancieleKostprijsverkopen;
            public String FinancieleKortingsrekening;
        }

        DataTable artikelgroep = new DataTable();

        private void GetData() {
            OleDbConnection conn = new OleDbConnection(Properties.Settings.Default.connection_string_artikelgroepconversie);
            conn.Open();
            DataTable tables = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            foreach (DataRow row in tables.Rows)
            {
                System.Diagnostics.Debug.WriteLine("Conversie for:" + row[2]);
            }
            OleDbDataAdapter daexcel = new OleDbDataAdapter("SELECT * FROM [artikelgroep$]", conn);

            daexcel.Fill(artikelgroep);
            conn.Close();
        }

        public Conversie() {
            GetData();
        }

        public ArtikelGroepInformatie GetArtikelGroepInformatie(Domain.BaseArtikel artikel)
        {
            string typenaam = artikel.GetType().Name;
            string materiaalcode = artikel.MateriaalCode;

            var list = 
                from row in artikelgroep.AsEnumerable()
                where row.Field<string>("Vergelijk_ArtikelType") == typenaam
                orderby row.Field<string>("Vergelijk_Begin") == null ? 0 : row.Field<string>("Vergelijk_Begin").Length descending
                select row;
            if (list.Count() < 1)
            {
                GetData();
                throw new Exception("nothing found for typenaam:" + typenaam );
            }
            // TODO: sorteren op de lengte
            foreach(DataRow row in list) {
                String matcher = row["Vergelijk_Begin"].ToString();
                if (matcher == materiaalcode.Substring(0, matcher.Length))
                {
                    ArtikelGroepInformatie agi = new ArtikelGroepInformatie();
                    agi.Omschrijving = row["Omschrijving"].ToString();
                    agi.BasisArtikelgroepCode = row["Gewenste_Basis_Artikelgroep_Code"].ToString();
                    agi.BasisArtikelgroepOmschrijving = row["Gewenste_Basis_Artikelgroep_Omschrijving"].ToString();
                    agi.BasisProductgroepCode = row["Gewenste_Basis_Productgroep_Code"].ToString();
                    agi.BasisProductgroepOmschrijving = row["Gewenste_Basis_Productgroep_Omschrijving"].ToString();
                    agi.BasisSoortartikelCode = row["Gewenste_Basis_Soortartikel_Code"].ToString();
                    agi.BasisSoortartikelOmschrijving = row["Gewenste_Basis_Soortartikel_Omschrijving"].ToString();

                    agi.FinancieleArtikelgroepNummer = row["Gewenste_Financiele_Artikelgroep_Nummer"].ToString();
                    agi.FinancieleArtikelgroepCode = row["Gewenste_Financiele_Artikelgroep_Code"].ToString();

                    agi.FinancieleOmzet = row["Gewenste_Financiele_Omzet"].ToString();
                    agi.FinancieleVoorraad = row["Gewenste_Financiele_Voorraad"].ToString();
                    agi.FinancieleKostprijsverkopen = row["Gewenste_Financiele_Kostprijsverkopen"].ToString();
                    agi.FinancieleKortingsrekening = row["Gewenste_Financiele_Kortingsrekening"].ToString();
                    return agi;
                }
            }
            GetData();
            throw new Exception("nothing found for typenaam:" + typenaam + " with materiaalcode:" + materiaalcode);
        }
    }
}
