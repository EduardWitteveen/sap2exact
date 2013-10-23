using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Data.OleDb;
using System.Data;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace sap2exact
{
    public class Program
    {
        static void Main(string[] args)
        {
            // . as decimal seperator
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            var csb = new MaxDB.Data.MaxDBConnectionStringBuilder(Properties.Settings.Default.connection_string_sap);
            Output.Info("SAP Connectionstring: " + csb.ConnectionString);
            var sapconnection = new MaxDB.Data.MaxDBConnection(csb.ConnectionString);

            // read the sap data
            sapconnection.Open();
            var importer = new Database2Domain(sapconnection);
            var data = importer.ReadEindArtikelData();
            //var data = importer.ReadEindArtikelData("42760X99");
            //var data = importer.ReadEindArtikelData("72205X99");

            // var data = importer.ReadEindArtikelData("33024D13");
            //var data = importer.ReadEindArtikelData("81110X99");
            //var data = importer.ReadEindArtikelData("31010X99");
            //var data = importer.ReadEindArtikelData("23035X99");
            //var data = importer.ReadEindArtikelData("33035X99");    
            //var data = importer.ReadEindArtikelData("81100Z11");    
            //var data = importer.ReadEindArtikelData("95818Z10");
    
            /*
            importer.Export2Excel(@"C:\exact importeren artikelen\export\mara", "SELECT * FROM MARA");
            importer.Export2Excel(@"C:\exact importeren artikelen\export\makt", "SELECT * FROM MAKT");
            importer.Export2Excel(@"C:\exact importeren artikelen\export\mast", "SELECT * FROM MAST");
            importer.Export2Excel(@"C:\exact importeren artikelen\export\stko", "SELECT * FROM STKO");
            importer.Export2Excel(@"C:\exact importeren artikelen\export\stas", "SELECT * FROM STAS");
            importer.Export2Excel(@"C:\exact importeren artikelen\export\stpo", "SELECT * FROM STPO");
            */
            // var data = importer.ReadEindArtikelData("01049Z10");
            // var data = importer.ReadEindArtikelData("HF213000");
            // var data = importer.ReadEindArtikelData("64152X99");
            // var data = importer.ReadEindArtikelData("81110X99");
            sapconnection.Close();

/*
            var data = new Domain.ExportData();             
            Hibernator serializer = new Hibernator();
            serializer.Save(data);            
            // read from access
            data = serializer.Load();
 */

            Output.Info("eindartikelen:" + data.EindArtikelen.Count);
            Output.Info("recepturen:" + data.ReceptuurArtikelen.Count);
            Output.Info("verpakkingen:" + data.VerpakkingsArtikelen.Count);
            Output.Info("grondstoffen:" + data.GrondstofArtikelen.Count);
            Output.Info("ingredienten:" + data.IngredientArtikelen.Count);

            // write the xml
            var exporter = new Domain2Xml();
            exporter.WriteData(data);

            Console.In.Read();
        }
/*
        private static Domain.ExportData RemoveFactorFromRecepturen(Domain.ExportData data)
        {

            foreach (Domain.BaseArtikel artikel in data.AlleArtikelen.Values.ToList())
            {
                var bsa = artikel as Domain.BaseSamengesteldArtikel;
                if (bsa != null)
                {
                    foreach (Domain.Stuklijst sl in ((Domain.BaseSamengesteldArtikel)artikel).Stuklijsten)
                    {
                        foreach (Domain.StuklijstRegel slr in sl.StuklijstRegels)
                        {
                            if (slr.ReceptuurEenheidFactor < 1)
                            {
                                // Dit moet wel een KN zijn, dus spoelwater ontbreekt hierin!
                                System.Diagnostics.Debug.Assert(slr.ReceptuurEenheid == "KN");

                                var weekreceptuur = new Domain.PhantomArtikel();
                                weekreceptuur.MateriaalCode = "HF" + slr.Artikel.MateriaalCode;
                                weekreceptuur.ArtikelOmschrijving = "Geweekte " + slr.Artikel.ArtikelOmschrijving;
                                weekreceptuur.ExactGewensteBelastingCategorie = slr.Artikel.ExactGewensteBelastingCategorie;
                                weekreceptuur.ExactGewensteNettoGewicht = slr.Artikel.ExactGewensteNettoGewicht;
                                weekreceptuur.BasishoeveelheidEenheid = slr.Artikel.BasishoeveelheidEenheid;
                                weekreceptuur.Gewichtseenheid = slr.Artikel.Gewichtseenheid;
                                //weekreceptuur.ExactGewensteNettoGewicht = slr.Artikel.ExactGewensteNettoGewicht;
                                //weekreceptuur.ExactGewensteBelastingCategorie = slr.Artikel.ExactGewensteBelastingCategorie;
                                weekreceptuur.NettoGewicht = slr.Artikel.NettoGewicht;


                                const int AANTAL_IN_RECEPTUUR = 100;
                                var stuklijstmateriaal = new Domain.StuklijstRegel();
                                stuklijstmateriaal.Volgnummer = 10;
                                stuklijstmateriaal.Artikel  = slr.Artikel;
                                stuklijstmateriaal.ReceptuurEenheid = slr.ReceptuurEenheid;
                                stuklijstmateriaal.ReceptuurRegelAantal = AANTAL_IN_RECEPTUUR * slr.ReceptuurEenheidFactor;
                                stuklijstmateriaal.ReceptuurEenheidFactor = 1;

                                var stuklijstwater = new Domain.StuklijstRegel();
                                stuklijstwater.Volgnummer = 20;
                                stuklijstwater.Artikel = new Domain.WeekWater();
                                stuklijstwater.ReceptuurEenheid = slr.ReceptuurEenheid;
                                stuklijstwater.ReceptuurRegelAantal = AANTAL_IN_RECEPTUUR - stuklijstmateriaal.ReceptuurRegelAantal;
                                stuklijstwater.ReceptuurEenheidFactor = 1;

                                var stuklijst = new Domain.Stuklijst();
                                stuklijst.StuklijstTotaalAantal = AANTAL_IN_RECEPTUUR;
                                stuklijst.StuklijstNaam = "SAP2EXACT: droog naar nat";
                                stuklijst.StuklijstVersion = 1;
                                stuklijst.StuklijstRegelsAdd(stuklijstmateriaal);
                                stuklijst.StuklijstRegelsAdd(stuklijstwater);
                                weekreceptuur.Stuklijsten.Add(stuklijst);

                                slr.Artikel = weekreceptuur;
                                data.Add(weekreceptuur);
                            }
                            else if (slr.ReceptuurEenheidFactor > 1)
                            {
                                slr.Artikel = slr.Artikel;
                            }
                        }
                    }
                }
            }
            return data;
        }
 */
    }
}

