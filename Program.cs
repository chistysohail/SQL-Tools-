using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using SqlCommand = Microsoft.Data.SqlClient.SqlCommand;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;
using SqlDataAdapter = Microsoft.Data.SqlClient.SqlDataAdapter;

namespace ExportSqlData
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString =
                "Server=(localdb)\\mssqllocaldb;Database=Prices;Trusted_Connection=True;MultipleActiveResultSets=true;";
            string outputDirectory = "E:\\tem3";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                DataTable schema = connection.GetSchema("Tables");
                var tableRows = schema.AsEnumerable().Where(row => row["TABLE_TYPE"].ToString() == "BASE TABLE");

                foreach (DataRow row in tableRows)
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    string outputFile = Path.Combine(outputDirectory, tableName + ".sql");

                    SqlCommand cmd = new SqlCommand($"SELECT TOP 100 * FROM {tableName} ORDER BY 1 DESC", connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable tableData = new DataTable();
                    adapter.Fill(tableData);

                    StringBuilder insertStatements = new StringBuilder();

                    foreach (DataRow dataRow in tableData.Rows)
                    {
                        StringBuilder columns = new StringBuilder();
                        StringBuilder values = new StringBuilder();

                        for (int i = 0; i < tableData.Columns.Count; i++)
                        {
                            columns.Append($"[{tableData.Columns[i].ColumnName}]");
                            values.Append(dataRow.IsNull(i) ? "NULL" : $"'{dataRow[i].ToString().Replace("'", "''")}'");

                            if (i < tableData.Columns.Count - 1)
                            {
                                columns.Append(", ");
                                values.Append(", ");
                            }
                        }

                        insertStatements.AppendLine($"INSERT INTO {tableName} ({columns}) VALUES ({values});");
                    }

                    File.WriteAllText(outputFile, insertStatements.ToString());
                }
            }
        }
    }
}

//using System;
//using System.Data;
//using System.Data.SqlClient;
//using System.IO;
//using System.Text;

//namespace GenerateInsertScriptMsSqlTool
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            string server = "your_server_name";
//            string database = "your_database_name";
//            string username = "your_username";
//            string password = "your_password";
//            string outputFolder = "E:/tem2/";

//            string connectionString = $"Server={server};Database={database};User Id={username};Password={password};";

//            using (SqlConnection connection = new SqlConnection(connectionString))
//            {
//                connection.Open();
//                DataTable tables = connection.GetSchema("Tables");

//                foreach (DataRow row in tables.Rows)
//                {
//                    string tableName = (string)row["TABLE_NAME"];
//                    string outputFile = Path.Combine(outputFolder, $"{tableName}.sql");

//                    using (SqlCommand cmdColumns =
//                           new SqlCommand(
//                               $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'",
//                               connection))
//                    {
//                        SqlDataReader reader = cmdColumns.ExecuteReader();
//                        string firstColumnName = string.Empty;

//                        if (reader.Read())
//                        {
//                            firstColumnName = reader.GetString(0);
//                        }

//                        reader.Close();

//                        if (!string.IsNullOrEmpty(firstColumnName))
//                        {
//                            using (SqlCommand cmd = new SqlCommand(
//                                       $"SELECT TOP 100 * FROM {tableName} ORDER BY {firstColumnName} DESC",
//                                       connection))
//                            {
//                                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
//                                DataTable tableData = new DataTable();
//                                adapter.Fill(tableData);

//                                using (StreamWriter writer = new StreamWriter(outputFile))
//                                {
//                                    foreach (DataRow dataRow in tableData.Rows)
//                                    {
//                                        StringBuilder columns = new StringBuilder();
//                                        StringBuilder values = new StringBuilder();

//                                        for (int i = 0; i < tableData.Columns.Count; i++)
//                                        {
//                                            DataColumn column = tableData.Columns[i];
//                                            columns.Append($"[{column.ColumnName}]");
//                                            values.Append($"'{dataRow[column].ToString().Replace("'", "''")}'");

//                                            if (i < tableData.Columns.Count - 1)
//                                            {
//                                                columns.Append(", ");
//                                                values.Append(", ");
//                                            }
//                                        }

//                                        writer.WriteLine($"INSERT INTO [{tableName}] ({columns}) VALUES ({values});");
//                                    }
//                                }
//                            }
//                        }

//                    }
//                }
//            }
//        }
//    }
//}

////Code to Merge in one file:

////using System;
////using System.IO;

////namespace MergeSqlFiles
////{
////    class Program
////    {
////        static void Main(string[] args)
////        {
////            string sourceFolder = @"e:\tem2"; // Replace with the path to the folder containing the .sql files
////            string masterFile = @"e:\tem3\master.sql"; // Replace with the path to the output master file

////            MergeSqlFiles(sourceFolder, masterFile);
////        }

////        private static void MergeSqlFiles(string sourceFolder, string masterFile)
////        {
////            DirectoryInfo directoryInfo = new DirectoryInfo(sourceFolder);

////            if (!directoryInfo.Exists)
////            {
////                Console.WriteLine("The source folder does not exist.");
////                return;
////            }

////            using (StreamWriter writer = new StreamWriter(masterFile))
////            {
////                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.sql"))
////                {
////                    using (StreamReader reader = new StreamReader(fileInfo.FullName))
////                    {
////                        string content = reader.ReadToEnd();
////                        writer.WriteLine($"-- Contents of file: {fileInfo.Name}");
////                        writer.WriteLine(content);
////                        writer.WriteLine();
////                    }
////                }
////            }

////            Console.WriteLine($"Merged .sql files into {masterFile}");
////        }
////    }
////}


//--get size of db:
//WITH SchemaSizes AS (
//    SELECT SCHEMA_NAME(schema_id) AS SchemaName,
//    SUM(used_pages * 8.0 / 1024) AS UsedSpaceMB,
//    SUM(reserved_pages * 8.0 / 1024) AS ReservedSpaceMB
//FROM sys.tables t
//JOIN sys.indexes i ON t.object_id = i.object_id
//CROSS APPLY (
//    SELECT SUM(used_page_count) AS used_pages,
//SUM(reserved_page_count) AS reserved_pages
//FROM sys.dm_db_partition_stats
//WHERE object_id = t.object_id
//AND index_id = i.index_id
//    ) AS partition_stats
//GROUP BY SCHEMA_NAME(schema_id)
//    )
//SELECT SchemaName,
//    UsedSpaceMB,
//    ReservedSpaceMB,
//(UsedSpaceMB + ReservedSpaceMB) AS TotalSpaceMB
//FROM SchemaSizes
//ORDER BY TotalSpaceMB DESC;


