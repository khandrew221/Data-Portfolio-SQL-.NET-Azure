using System;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Data.Common;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace sqltest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "myportfolioserver.database.windows.net";
                builder.UserID = "azureuser";
                builder.Password = "LgFF8>CGc5w]-4w";
                builder.InitialCatalog = "SampleDatabase";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    Console.WriteLine("\nQuestion: How many countries are customers from, and what are they?");
                    Console.WriteLine("=========================================\n");

                    String sql = "SELECT DISTINCT CountryRegion FROM [SalesLT].[Address]";
                    DataTable dataTable1 = RunSQLQuery(connection, sql);

                    Console.WriteLine("Customers are from " + dataTable1.Rows.Count + " different countries.\n");
                    Console.WriteLine("Those countries are:\n");
                    PrintDataTable(dataTable1, ", ");

                    Console.WriteLine("\nQuestion: How many customers are there from each country?");
                    Console.WriteLine("=========================================\n");

                    sql = "SELECT Address.CountryRegion, COUNT(CustomerAddress.CustomerID) AS NumberOfCustomers " +
                        "FROM[SalesLT].[CustomerAddress] " +
                        "LEFT JOIN[SalesLT].[Address] ON CustomerAddress.AddressID = Address.AddressID " +
                        "GROUP BY SalesLT.Address.CountryRegion";
                    DataTable dataTable2 = RunSQLQuery(connection, sql);

                    PrintDataTable(dataTable2, ": ");

                    Console.WriteLine("\nQuestion: What is the total due on orders from each country?");
                    Console.WriteLine("=========================================\n");

                    sql = "SELECT Address.CountryRegion, SUM(SalesOrderHeader.TotalDue) FROM[SalesLT].[SalesOrderHeader] " +
                        "LEFT JOIN[SalesLT].[CustomerAddress] ON SalesOrderHeader.CustomerID = CustomerAddress.CustomerID " +
                        "LEFT JOIN[SalesLT].[Address] ON CustomerAddress.AddressID = Address.AddressID " +
                        "GROUP BY Address.CountryRegion;";
                    DataTable dataTable3 = RunSQLQuery(connection, sql);

                    //The query will not include countries with nothing due on orders; they should be added
                    //to the dataTable as 0
                    if (dataTable3.Rows.Count < dataTable1.Rows.Count)
                    {
                        //iterate all countries found
                        for (int i = 0; i < dataTable1.Rows.Count; i++)
                        {
                            //get the country name
                            String country = dataTable1.Rows[i].Field<String>("CountryRegion");

                            //check the order totals data table for the country
                            bool containsCountry = dataTable3.AsEnumerable().Any(row => country == row.Field<String>("CountryRegion"));

                            //if the country is not found, add a new row to the datatable that gives it $0 due on orders 
                            if (!containsCountry)
                            {
                                dataTable3.Rows.Add(new Object[] { country, 0 });
                            }
                        }
                    }    

                    PrintDataTable(dataTable3, ": $");


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
                    if (i < fields-1)
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