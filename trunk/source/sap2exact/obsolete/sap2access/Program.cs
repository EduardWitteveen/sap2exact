using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2access
{
    class Program
    {
        static void Main(string[] args)
        {
            //From: MaxDB.Data.MaxDBConnectionStringBuilder
            var csb = new MaxDB.Data.MaxDBConnectionStringBuilder(Properties.Settings.Default.connection_string_sap);
            Console.WriteLine("SAP Connectionstring: " + csb.ConnectionString);
            var sapconnection = new MaxDB.Data.MaxDBConnection(csb.ConnectionString);
            sapconnection.Open();

            // var sql = "SELECT * FROM MAST";
            // Export2Excel("MAST", sql, sapconnection);
            //var sql = queries.EmbeddedResource.GetString("queries.artikel.sql");
            //Export2Excel("artikel", sql, sapconnection);
            //sql = queries.EmbeddedResource.GetString("queries.stuklijst.sql");
            //Export2Excel("stuklijst", sql, sapconnection);

            // open the access database
            var accessconnection = new OleDbConnection(Properties.Settings.Default.connection_string_access);
            accessconnection.Open();

            // remove the old stuff from the access database
            Console.WriteLine("Removing all sapdata from the access database");
            new OleDbCommand("DELETE FROM artikel", accessconnection).ExecuteNonQuery();
            new OleDbCommand("DELETE FROM stuklijst", accessconnection).ExecuteNonQuery();

            // query artikelen
            Console.WriteLine("Fetching all sap artikelen");
            var sapartikelcmd = new MaxDB.Data.MaxDBCommand(queries.EmbeddedResource.GetString("queries.artikel.sql"), sapconnection);
            var sapartikelreader = sapartikelcmd.ExecuteReader();
            var sapartikeltable = new DataTable();
            sapartikeltable.Load(sapartikelreader);

            // create the insert stuff on the access artikel
            Console.WriteLine("Transforming the artikelen for access");
            var accessartikeladapter = new OleDbDataAdapter("SELECT * FROM artikel", accessconnection);
            var accessartikelbuilder = new OleDbCommandBuilder(accessartikeladapter); 
            accessartikelbuilder.QuotePrefix = "[";
            accessartikelbuilder.QuoteSuffix = "]";
            var accessartikeltable = new DataTable();
            accessartikeladapter.Fill(accessartikeltable);
            //accessartikeladapter.InsertCommand = accessartikelbuilder.GetInsertCommand();

            
            foreach (DataRow sapartikelrow in sapartikeltable.Rows)
            {
                var accessartikelrow = accessartikeltable.NewRow();
                foreach (DataColumn sapartikelcolumn in sapartikeltable.Columns)
                {
                    // copy all the row values
                    // we assume that the columns are in the same order
                    //accessartikelrow[sapartikeltable.Columns.IndexOf(sapartikelcolumn)] = sapartikelrow[sapartikelcolumn];
                    accessartikelrow[sapartikelcolumn.ColumnName] = sapartikelrow[sapartikelcolumn];
                }
                accessartikeltable.Rows.Add(accessartikelrow);
            }

            // now store our data inside the access database
            Console.WriteLine("Writing the artikelen to access");
            accessartikeladapter.Update(accessartikeltable);

            // create the insert stuff on the access artikel
            var accessbomadapter = new OleDbDataAdapter("SELECT * FROM stuklijst", accessconnection);
            var accessbombuilder = new OleDbCommandBuilder(accessbomadapter);
            accessbombuilder.QuotePrefix = "[";
            accessbombuilder.QuoteSuffix = "]";
            var accessbomtable = new DataTable();
            accessbomadapter.Fill(accessbomtable);
            //accessartikeladapter.InsertCommand = accessartikelbuilder.GetInsertCommand();


            Console.WriteLine("Looping over the artikelen, to get the BOMs");
            string artikelcode = null;
            foreach (DataRow sapartikelrow in sapartikeltable.Rows)
            {

                if (artikelcode != Convert.ToString(sapartikelrow["Artikelnummer"]))
                {
                    artikelcode = Convert.ToString(sapartikelrow["Artikelnummer"]);
                    Console.WriteLine("Retrieving the BOM for:" + artikelcode);
                    var sapbomcommand = new MaxDB.Data.MaxDBCommand(queries.EmbeddedResource.GetString("queries.stuklijst.sql"), sapconnection);
                    sapbomcommand.CommandType = System.Data.CommandType.Text;
                    sapbomcommand.Parameters.Add(new MaxDB.Data.MaxDBParameter(":artikelnummer", artikelcode));
                    var sapbomadapter = new MaxDB.Data.MaxDBDataAdapter(sapbomcommand);
                    var sapbomtable = new DataTable();
                    sapbomadapter.Fill(sapbomtable);

                    foreach (DataRow sapbomrow in sapbomtable.Rows)
                    {
                        var accessbomrow = accessbomtable.NewRow();
                        foreach (DataColumn sapbomcolumn in sapbomtable.Columns)
                        {
                            // copy all the row values
                            // we assume that the columns are in the same order
                            //accessartikelrow[sapartikeltable.Columns.IndexOf(sapartikelcolumn)] = sapartikelrow[sapartikelcolumn];
                            accessbomrow[sapbomcolumn.ColumnName] = sapbomrow[sapbomcolumn];
                        }
                        accessbomtable.Rows.Add(accessbomrow);
                    }
                    if (sapbomtable.Rows.Count > 0)
                    {
                        accessbomadapter.Update(accessbomtable);
                        Console.WriteLine("\tAdded #" + sapbomtable.Rows.Count + " rows");
                    }
                }
            }
            //itemcommand.CommandType = System.Data.CommandType.Text;
            //itemcommand.Parameters.AddWithValue("@code", "33024X99");
            //var itemreader = itemcommand.ExecuteReader();
            //var itemtable = new DataTable();
            //itemtable.Load(itemreader);

            accessconnection.Close();
            sapconnection.Close();

            Console.In.ReadLine();
        }

        private static void Export2Excel(string name, string sql, MaxDB.Data.MaxDBConnection connection)
        {
            Console.WriteLine("Exporting: " + name + "\n\t" + sql);

            // query
            var command = new MaxDB.Data.MaxDBCommand(sql, connection);
            var reader = command.ExecuteReader();
            var table = new DataTable();
            table.Load(reader);

            // export to excel
            var exportfile = new System.IO.FileInfo(Properties.Settings.Default.exportpath_xlsx + @"\" + name  +  ".xlsx");
            if (exportfile.Exists) exportfile.Delete();
            XLWorkbook workbook = new XLWorkbook();
            workbook.Worksheets.Add(table, "data");
            var worksheet = workbook.Worksheets.Add("sql");
            worksheet.Cell("A1").Value = sql;
            workbook.SaveAs(exportfile.FullName);
        }
    }
}
