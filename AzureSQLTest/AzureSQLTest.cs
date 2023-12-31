﻿using Microsoft.Data.SqlClient;
using System.Data;

namespace sqltest
{
    class AzureSQLTest
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

                    Console.WriteLine("=========================================");
                    Console.WriteLine("Question 1: How many countries are customers from, and what are those countries?");
                    Console.WriteLine("=========================================\n");

                    String sql = "SELECT DISTINCT CountryRegion FROM [SalesLT].[Address]";
                    DataTable dataTable1 = RunSQLQuery(connection, sql);

                    Console.WriteLine("Customers are from " + dataTable1.Rows.Count + " different countries.\n");
                    Console.WriteLine("Those countries are:\n");
                    PrintDataTable(dataTable1, ", ");

                    Console.WriteLine("\n=========================================");
                    Console.WriteLine("Question 2: How many customers are there from each country?");
                    Console.WriteLine("=========================================\n");

                    sql = "SELECT Address.CountryRegion, COUNT(CustomerAddress.CustomerID) AS NumberOfCustomers " +
                        "FROM[SalesLT].[CustomerAddress] " +
                        "LEFT JOIN[SalesLT].[Address] ON CustomerAddress.AddressID = Address.AddressID " +
                        "GROUP BY Address.CountryRegion";
                    DataTable dataTable2 = RunSQLQuery(connection, sql);

                    PrintDataTable(dataTable2, ": ");

                    Console.WriteLine("\n=========================================");
                    Console.WriteLine("Question 3: What is the total due on all orders from each country?");
                    Console.WriteLine("=========================================\n");

                    sql = "SELECT Address.CountryRegion, SUM(SalesOrderHeader.TotalDue) AS 'AllOrdersTotalDue' " +
                        "FROM[SalesLT].[SalesOrderHeader] " +
                        "LEFT JOIN[SalesLT].[CustomerAddress] ON SalesOrderHeader.CustomerID = CustomerAddress.CustomerID " +
                        "RIGHT JOIN[SalesLT].[Address] ON CustomerAddress.AddressID = Address.AddressID " +
                        "GROUP BY Address.CountryRegion";
                    DataTable dataTable3 = RunSQLQuery(connection, sql);

                    //The query will return null for countries with no orders; replace these with 0
                    //The column needs to be made writable first
                    dataTable3.Columns[1].ReadOnly = false;
                    foreach (DataRow row in dataTable3.Rows)
                    {
                        if (row[1] is System.DBNull)
                        {
                            row[1] = 0;
                        }
                    }

                    PrintDataTable(dataTable3, ": $");

                    Console.WriteLine("\n=========================================");
                    Console.WriteLine("Question 4: Which product has had the most units shipped to addresses in the United States?");
                    Console.WriteLine("=========================================\n");

                    sql = "SELECT TOP 1 Product.Name, SUM(SalesOrderDetail.OrderQty) AS 'Total US Sales' " +
                        "FROM[SalesLT].[SalesOrderDetail] " +
                        "LEFT JOIN[SalesLT].[Product] ON SalesOrderDetail.ProductID = Product.ProductID " +
                        "LEFT JOIN[SalesLT].[SalesOrderHeader] ON SalesOrderDetail.SalesOrderID = SalesOrderHeader.SalesOrderID " +
                        "LEFT JOIN[SalesLT].[Address] ON SalesOrderHeader.ShipToAddressID = Address.AddressID " +
                        "WHERE Address.CountryRegion = 'United States' " +
                        "GROUP BY Product.Name " +
                        "ORDER BY 'Total US Sales' DESC";
                    DataTable dataTable4 = RunSQLQuery(connection, sql);

                    PrintDataTable(dataTable4, ": ");


                    Console.WriteLine("\n=========================================");
                    Console.WriteLine("Question 5: What is the total number of units sold, and for what total sales value, for each parent product category?");
                    Console.WriteLine("=========================================\n");

                    sql = "SELECT ParentProductCategoryName, SUM(OrderQty) AS TotalUnits, SUM(LineTotal) AS TotalSales " +
                        "FROM [SalesLT].[SalesOrderDetail] " +
                        "LEFT JOIN [SalesLT].[Product] ON SalesOrderDetail.ProductID = Product.ProductID " +
                        "LEFT JOIN [SalesLT].[vGetAllCategories] ON Product.ProductCategoryID = vGetAllCategories.ProductCategoryID " +
                        "GROUP BY ParentProductCategoryName";
                    DataTable dataTable5 = RunSQLQuery(connection, sql);

                    using (DataTableReader reader = dataTable5.CreateDataReader())
                    {
                        while (reader.Read())
                        {
                            IDataRecord record = (IDataRecord)reader;
                            String category = record[0].ToString();
                            String units = record[1].ToString();
                            String sales = record[2].ToString();
                            String output = string.Format("Product category {0} sold {1} units for a total of ${2} in sales.", category, units, sales);
                            Console.WriteLine(output);
                        }
                    }

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