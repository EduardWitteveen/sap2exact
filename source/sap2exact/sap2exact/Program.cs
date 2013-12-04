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

            sap2exact.SapSDK.SDK sdk = new SapSDK.SDK(
                Properties.Settings.Default.sdk_sap_server,
                Properties.Settings.Default.sdk_sap_instance,
                Properties.Settings.Default.sdk_sap_user,
                Properties.Settings.Default.sdk_sap_password
            );
            /*
                1. You will get the short text from STPO-POTX1 -- First line of the Long Text and STPO-POTX2 -- Second line of the Long Text
                2. Long text
                    Object : BOM
                    ID     : MPO
                    Lang   : EN
                    Name   : Concatenate 
                            STPO-MANDT   800
                            STPO-STLTY    M
                            STPO-STLNR    00002863
                            STPO-STLKN    2
                            STPO-STPOZ    4
                           ie.  800M000028630000000200000004     
             */
            /*
            function: 'READ_TEXT' with parameter: 'ARCHIVE_HANDLE'
            function: 'READ_TEXT' with parameter: 'CLIENT'
            function: 'READ_TEXT' with parameter: 'ID'
            function: 'READ_TEXT' with parameter: 'LANGUAGE'
            function: 'READ_TEXT' with parameter: 'LOCAL_CAT'
            function: 'READ_TEXT' with parameter: 'NAME'
            function: 'READ_TEXT' with parameter: 'OBJECT'
            */
            // http://help.sap.com/saphelp_nw70/helpdata/en/d6/0db8c8494511d182b70000e829fbfe/content.htm
/*            sdk.Call("READ_TEXT", new Dictionary<string, object>() {                
                //  CLIENT
                //      Specify the client under which the text is stored. If you omit this parameter, the system uses the current client as default.
                //      Reference field:	SY-MANDT
                //      Default value:	SY-MANDT
                {"CLIENT", 100},
                //  OBJECT
                //      Enter the name of the text object to which the text is allocated. Table TTXOB contains the valid objects.
                //      Reference field:	THEAD-TDOBJECT
                {"OBJECT", "BOM"},
                //  NAME
                //      Enter the name of the text module. The name may be up to 70 characters long. Its internal structure depends on the text object used.
                //      Reference field:	THEAD-TDNAME
                {"NAME", "100M000024700000000500000010"},
                //  ID
                //      Enter the text ID of the text module. Table TTXID contains the valid text IDs, depending on the text object.
                //      Reference field:	THEAD-TDID
                {"ID", "MPO"},  //??
                //  LANGUAGE
                //      Enter the language key of the text module. The system accepts only languages that are defined in table T002.
                //      Reference field:	THEAD-TDSPRAS
                {"LANGUAGE","NL"},
                //  ARCHIVE_HANDLE
                //      If you want to read the text from the archive, you must enter a handle here. 
                //      The system uses it to access the archive. You can create the handle using the function module ACHIVE_OPEN_FOR_READ.
                //      The value '0' indicates that you do not want to read the text from the archive.
                //      Reference field:	SY-TABIX
                //      Default value:	0
                {"ARCHIVE_HANDLE", 0}
                //      LOCAL_CAT - Text catalog local
                //      {"LOCAL_CAT", "value"},
            });
 */
            /* OBJECT, NAME, ID, and LANGUAGE */
            /*
                SELECT *
                FROM STXL
                WHERE TDNAME = '100M000024700000000500000010'
             */
            sdk.Call("READ_TEXT", new Dictionary<string, object>() {                
                {"OBJECT", "BOM"},
                {"NAME", "100M000024700000000500000010"},
                {"ID", "MPO"}, 
                {"LANGUAGE","N"},


                {"CLIENT", 100},
                {"ARCHIVE_HANDLE", 0}
            });

            
                
                

                        /*
                        var csb = new MaxDB.Data.MaxDBConnectionStringBuilder(Properties.Settings.Default.connection_string_sap);
                        Output.Info("SAP Connectionstring: " + csb.ConnectionString);
                        var sapconnection = new MaxDB.Data.MaxDBConnection(csb.ConnectionString);

                        // read the sap data
                        sapconnection.Open();
                        var importer = new Database2Domain(sapconnection);
                        var data = importer.ReadEindArtikelData();
                        //var data = importer.ReadEindArtikelData("42760X99");
                        //var data = importer.ReadEindArtikelData("81110X99");
                        //var data = importer.ReadEindArtikelData("14009Z25");
                        //var data = importer.ReadEindArtikelData("01050D15");            
                        sapconnection.Close();

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
                        */
            Output.Info("Press any key to continue...");
            Console.In.Read();

            // dispose everything
            Output.Dispose();
            sdk.Dispose();
        }
    }
}

