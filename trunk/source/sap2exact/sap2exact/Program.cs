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
            //var data = importer.ReadEindArtikelData("42760X99");
            //var data = importer.ReadEindArtikelData("81110X99");
            //var data = importer.ReadEindArtikelData("14009Z25");
            //var data = importer.ReadEindArtikelData("01050D15");            
            sapconnection.Close();
            sdk.Dispose();

            //var data = new Domain.ExportData();             
            //Hibernator serializer = new Hibernator();
            //serializer.Save(data);            
            //// read from access
            //data = serializer.Load();

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

