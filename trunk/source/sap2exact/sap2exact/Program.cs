﻿using System;
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
            //var data = importer.ReadEindArtikelData();
            var data = importer.ReadEindArtikelData("33024D13");
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
    }
}

