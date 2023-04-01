
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace GenerateInsertScriptMsSqlTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string server = "your_server_name";
            string database = "your_database_name";
            string username = "your_username";
            string password = "your_password";
            string outputFolder = "E:/tem2/";

            string connectionString = $"Server={server};Database={database};User Id={username};Password={password};";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                DataTable tables = connection.GetSchema("Tables");

                foreach (DataRow row in tables.Rows)
                {
                    string tableName = (string)row["TABLE_NAME"];
                    string outputFile = Path.Combine(outputFolder, $"{tableName}.sql");

                    using (SqlCommand cmdColumns =
                           new SqlCommand(
                               $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'",
                               connection))
                    {
                        SqlDataReader reader = cmdColumns.ExecuteReader();
                        string firstColumnName = string.Empty;

                        if (reader.Read())
                        {
                            firstColumnName = reader.GetString(0);
                        }

                        reader.Close();

                        if (!string.IsNullOrEmpty(firstColumnName))
                        {
                            using (SqlCommand cmd = new SqlCommand(
                                       $"SELECT TOP 100 * FROM {tableName} ORDER BY {firstColumnName} DESC",
                                       connection))
                            {
                                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                                DataTable tableData = new DataTable();
                                adapter.Fill(tableData);

                                using (StreamWriter writer = new StreamWriter(outputFile))
                                {
                                    foreach (DataRow dataRow in tableData.Rows)
                                    {
                                        StringBuilder columns = new StringBuilder();
                                        StringBuilder values = new StringBuilder();

                                        for (int i = 0; i < tableData.Columns.Count; i++)
                                        {
                                            DataColumn column = tableData.Columns[i];
                                            columns.Append($"[{column.ColumnName}]");
                                            values.Append($"'{dataRow[column].ToString().Replace("'", "''")}'");

                                            if (i < tableData.Columns.Count - 1)
                                            {
                                                columns.Append(", ");
                                                values.Append(", ");
                                            }
                                        }

                                        writer.WriteLine($"INSERT INTO [{tableName}] ({columns}) VALUES ({values});");
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }
    }
}

//Code to Merge in one file:

//using System;
//using System.IO;

//namespace MergeSqlFiles
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            string sourceFolder = @"e:\tem2"; // Replace with the path to the folder containing the .sql files
//            string masterFile = @"e:\tem3\master.sql"; // Replace with the path to the output master file

//            MergeSqlFiles(sourceFolder, masterFile);
//        }

//        private static void MergeSqlFiles(string sourceFolder, string masterFile)
//        {
//            DirectoryInfo directoryInfo = new DirectoryInfo(sourceFolder);

//            if (!directoryInfo.Exists)
//            {
//                Console.WriteLine("The source folder does not exist.");
//                return;
//            }

//            using (StreamWriter writer = new StreamWriter(masterFile))
//            {
//                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.sql"))
//                {
//                    using (StreamReader reader = new StreamReader(fileInfo.FullName))
//                    {
//                        string content = reader.ReadToEnd();
//                        writer.WriteLine($"-- Contents of file: {fileInfo.Name}");
//                        writer.WriteLine(content);
//                        writer.WriteLine();
//                    }
//                }
//            }

//            Console.WriteLine($"Merged .sql files into {masterFile}");
//        }
//    }
//}
