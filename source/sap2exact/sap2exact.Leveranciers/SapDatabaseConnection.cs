using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact.Leveranciers
{
    public class SapDatabaseConnection
    {
        private static MaxDB.Data.MaxDBConnection sapconnection;

        public SapDatabaseConnection(string connectionstring)
        {
            var csb = new MaxDB.Data.MaxDBConnectionStringBuilder(connectionstring);
            System.Diagnostics.Debug.WriteLine("SAP Connectionstring: " + csb.ConnectionString);
            sapconnection = new MaxDB.Data.MaxDBConnection(csb.ConnectionString);
        }
        public void Open()
        {
            sapconnection.Open();
        }
        public void Close()
        {
            sapconnection.Close();
        }
        public DateTime ConvertSapDate(object value, string where)
        {
            if (Convert.ToString(value) == "00000000")
            {
                System.Diagnostics.Debug.WriteLine("Could not convert the date for: " + where);
                return DateTime.Now;
            }
            var str = Convert.ToString(value);
            int year = Convert.ToInt32(str.Substring(0, 4));
            int month = Convert.ToInt32(str.Substring(4, 2));
            int day = Convert.ToInt32(str.Substring(6, 2));
            return new DateTime(year, month, day);
        }

        public DataRow QueryRow(string sql, Dictionary<string, object> parameters = null)
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

        public DataTable QueryTable(string sql, Dictionary<string, object> parameters = null)
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
            System.Diagnostics.Debug.WriteLine("Exporting: " + name + "\n\t" + sql);

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

            System.Diagnostics.Debug.WriteLine("Exporting finished to file//:" + exportfile.FullName);
        }

        public void Export2Csv(string name, string sql, Dictionary<string, object> parameters = null)
        {
            System.Diagnostics.Debug.WriteLine("Exporting: " + name + "\n\t" + sql);

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

            // export to csv
            var exportfile = new System.IO.FileInfo(name + ".csv");
            if (exportfile.Exists) exportfile.Delete();            
            var sw = new System.IO.StreamWriter(exportfile.FullName);

            bool firstcolumn = true;
            foreach (DataColumn column in table.Columns)
            {
                String columnname = column.ColumnName.Replace("\"", "\"\"") ;
                if (firstcolumn)
                {
                    sw.Write("\"" + columnname + "\"");
                    firstcolumn = false;
                }
                else
                {
                    sw.Write(";\"" + columnname +  "\"");
                }
            }
            sw.Write("\n");
            foreach (DataRow row in table.Rows)
            {
                firstcolumn = true;
                foreach (DataColumn column in table.Columns)
                {
                    String columnvalue = Convert.ToString(row[column]).Replace("\"", "\"\"");
                    if (firstcolumn)
                    {
                        sw.Write("\"" + columnvalue + "\"");
                        firstcolumn = false;
                    }
                    else
                    {
                        sw.Write(";\"" + columnvalue + "\"");
                    }
                }
                sw.Write("\n");
            }
            sw.Close();
            System.Diagnostics.Debug.WriteLine("Exporting finished to file//:" + exportfile.FullName);
        }

    }
}
