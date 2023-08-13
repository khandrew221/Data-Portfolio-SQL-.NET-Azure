using Microsoft.Data.SqlClient;
using System.Data;

namespace sqltest
{
    class AzureSQLDDLDML
    {
        static void Main(string[] args)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "myportfolioserver.database.windows.net";
                builder.UserID = "azureuser";
                builder.Password = "LgFF8>CGc5w]-4w";
                builder.InitialCatalog = "ReadWriteDatabase";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();


                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /*
         * Runs a SQL query and returns the results as a DataTable
         */
        private static DataTable RunSQLQuery(SqlConnection connection, String sql)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        dataTable.Load(reader);
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("RunSQLQuery failed.");
                Console.WriteLine(e.ToString());
            }

            return dataTable;
        }

        /*
        *  Prints a datatable to the console. Rows are delimited with the provided delimiter.
        */
        private static void PrintDataTable(DataTable table, String del)
        {
            using (DataTableReader reader = table.CreateDataReader())
            {
                while (reader.Read())
                {
                    PrintDelimitedRow((IDataRecord)reader, del);
                }
            }
        }

        /*
         *  Prints a delimited row from a IDataRecord object to the console
         */
        private static void PrintDelimitedRow(IDataRecord dataRecord, String del)
        {
            // get the number of field in this record
            int fields = dataRecord.FieldCount;

            // if there are fields...
            if (fields > 0)
            {
                String output = "";

                //construct an output string...
                for (int i = 0; i < fields; i++)
                {
                    output += dataRecord[i].ToString();
                    if (i < fields - 1)
                    {
                        output += del;
                    }
                }

                //...and write it to console
                Console.WriteLine(output);

            }
            else
            {
                Console.WriteLine("No fields found!");
            }
        }
    }

}