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

namespace access2exact
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
            Console.WriteLine("SAP Connectionstring: " + csb.ConnectionString);
            var sapconnection = new MaxDB.Data.MaxDBConnection(csb.ConnectionString);


            // read the sap data
            sapconnection.Open();
            var importer = new Database2Domain(sapconnection);
            var data = importer.ReadAllArtikelData();
            sapconnection.Close();

            /*
            var data = new Domain.ExportData();
            Hibernator serializer = new Hibernator();
            
            // write to access
            serializer.Save(data);
            // read from access
            data = serializer.Load();
            */
            // write the xml
            var exporter = new Domain2Xml();
            exporter.WriteData(data);

        }
    }
}