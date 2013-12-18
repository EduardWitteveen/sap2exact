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
            sap2exact.SapSDK.SDK sdk = new SapSDK.SDK(
                Properties.Settings.Default.sdk_sap_server,
                Properties.Settings.Default.sdk_sap_instance,
                Properties.Settings.Default.sdk_sap_user,
                Properties.Settings.Default.sdk_sap_password
            );
            var importer = new Database2Domain(sapconnection, sdk);

            var data = importer.ReadEindArtikelData();
            //var data = importer.ReadEindArtikelData("02002Z26");            
            //var data = importer.ReadEindArtikelData("PE570000");
            //var data = importer.ReadEindArtikelData("93380D10");
            //var data = importer.ReadEindArtikelData("01005D06");
            //var data = importer.ReadEindArtikelData("42760X99");
            //var data = importer.ReadEindArtikelData("81110X99");
            //var data = importer.ReadEindArtikelData("14009Z25");
            //var data = importer.ReadEindArtikelData("01050D15");
            //var data = importer.ReadEindArtikelData("51541D15");
            
            sapconnection.Close();
            sdk.Dispose();

            //var data = new Domain.ExportData();             
            //Hibernator serializer = new Hibernator();
            //serializer.Save(data);            
            //// read from access
            //data = serializer.Load();

            // -----------------------------------------------------------------------
            // HACK: tekstregels onderaan ( pos + 1000)
            // HACK: exporteren artikelcode met notities
            // -----------------------------------------------------------------------
            DataTable log = new DataTable();
            log.Columns.Add("artikel", typeof(string));
            log.Columns.Add("text", typeof(string));
            foreach(Domain.BaseArtikel baseartikel in data.AlleArtikelen.Values) {
                var samengesteldartikel = baseartikel as Domain.BaseSamengesteldArtikel;
                if (samengesteldartikel != null)
                {
                    foreach(Domain.Stuklijst stuklijst in samengesteldartikel.Stuklijsten) {
                        foreach (Domain.StuklijstRegel regel in stuklijst.StuklijstRegels)
                        {
                            var tekstregel = regel.Artikel as Domain.TekstArtikel;
                            if (tekstregel != null)
                            {
                                // tekstregel
                                // onderaan plaatsen
                                regel.Volgnummer += 1000;
                                // en exporteren             
                                DataRow logrow = log.NewRow();
                                logrow["artikel"] = Domain2Xml.CreateSapCode(samengesteldartikel);
                                logrow["text"] = tekstregel.Tekst;
                                log.Rows.Add(logrow);
                            }
                        }
                    }
                }
            }
            var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add(log, "log");
            var exportfile = new System.IO.FileInfo("notities.xlsx");
            if (exportfile.Exists) exportfile.Delete();
            workbook.SaveAs(exportfile.FullName);
            Output.Info("notities in:" + exportfile.FullName);

            Output.Info("eindartikelen:" + data.EindArtikelen.Count);
            Output.Info("recepturen:" + data.ReceptuurArtikelen.Count);
            Output.Info("verpakkingen:" + data.VerpakkingsArtikelen.Count);
            Output.Info("grondstoffen:" + data.GrondstofArtikelen.Count);
            Output.Info("ingredienten:" + data.IngredientArtikelen.Count);

            // write the xml
            var exporter = new Domain2Xml();
            exporter.WriteData(data);

            Output.Info("Press any key to continue...");
            Console.In.Read();

            // dispose everything
            Output.Dispose();
        }
    }
}

