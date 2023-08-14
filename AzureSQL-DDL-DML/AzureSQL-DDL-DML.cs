using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Data;
using System.Diagnostics.Metrics;

namespace sqltest
{
    class AzureSQLDDLDML
    {
        static void Main(string[] args)
        {

            //First, create datatables corresponding to the CSV files. This allows for cleaning and normalisation
            //before commiting anything to the database with a SQL query 

            Console.WriteLine("Reading from CSVs...");

            DataTable doctorDirty = CSVtoDataTable("D:\\VS Projects\\AzureSQLTest\\AzureSQL-DDL-DML\\CSV files\\doctor.csv", ',');
            DataTable drugDirty = CSVtoDataTable("D:\\VS Projects\\AzureSQLTest\\AzureSQL-DDL-DML\\CSV files\\drug.csv", ',');
            DataTable patientDirty = CSVtoDataTable("D:\\VS Projects\\AzureSQLTest\\AzureSQL-DDL-DML\\CSV files\\patient.csv", ',');
            DataTable prescriptionDirty = CSVtoDataTable("D:\\VS Projects\\AzureSQLTest\\AzureSQL-DDL-DML\\CSV files\\prescription.csv", ',');

            // The files used here are simple and clean in terms of values parsable to the correct data type,
            // and already normalised, so much of the work of converting a spreadsheet style dataset into a database is already done.

            // However, all data has been read in as strings. That needs to be fixed, so the initial tables are labelled "dirty" and
            // a minor cleaning process must take place.

            Console.WriteLine("Cleaning data...");

            //the doctor and drug dataframes only contain string data anyway
            DataTable doctorClean = doctorDirty;
            doctorClean.TableName = "doctor";
            DataTable drugClean = drugDirty;
            drugClean.TableName = "drug";

            //the patient dataframe contains multiple data types
            DataTable patientClean = new DataTable("patient");
            patientClean.Columns.Add(patientDirty.Columns[0].ColumnName, typeof(String));
            patientClean.Columns.Add(patientDirty.Columns[1].ColumnName, typeof(String));
            patientClean.Columns.Add(patientDirty.Columns[2].ColumnName, typeof(DateTime));
            patientClean.Columns.Add(patientDirty.Columns[3].ColumnName, typeof(String));
            patientClean.Columns.Add(patientDirty.Columns[4].ColumnName, typeof(Decimal));
            patientClean.Columns.Add(patientDirty.Columns[5].ColumnName, typeof(Decimal));
            patientClean.Columns.Add(patientDirty.Columns[6].ColumnName, typeof(String));

            //luckily, the data types are auto parsed from strings in this process
            for (int i = 0; i < patientDirty.Rows.Count; i++)
            {
                DataRow cleanRow = patientClean.NewRow();
                DataRow dirtyRow = patientDirty.Rows[i];
                cleanRow[0] = dirtyRow[0];
                cleanRow[1] = dirtyRow[1];
                cleanRow[2] = dirtyRow[2];
                cleanRow[3] = dirtyRow[3];
                cleanRow[4] = dirtyRow[4];
                cleanRow[5] = dirtyRow[5];
                cleanRow[6] = dirtyRow[6];
                patientClean.Rows.Add(cleanRow);
            }

            //A similar process can be applied to the prescription dataframe
            DataTable prescriptionClean = new DataTable("prescription");
            prescriptionClean.Columns.Add(prescriptionDirty.Columns[0].ColumnName, typeof(String));
            prescriptionClean.Columns.Add(prescriptionDirty.Columns[1].ColumnName, typeof(DateTime));
            prescriptionClean.Columns.Add(prescriptionDirty.Columns[2].ColumnName, typeof(String));
            prescriptionClean.Columns.Add(prescriptionDirty.Columns[3].ColumnName, typeof(String));
            prescriptionClean.Columns.Add(prescriptionDirty.Columns[4].ColumnName, typeof(String));
            prescriptionClean.Columns.Add(prescriptionDirty.Columns[5].ColumnName, typeof(String));

            for (int i = 0; i < prescriptionDirty.Rows.Count; i++)
            {
                DataRow cleanRow = prescriptionClean.NewRow();
                DataRow dirtyRow = prescriptionDirty.Rows[i];
                cleanRow[0] = dirtyRow[0];
                cleanRow[1] = dirtyRow[1];
                cleanRow[2] = dirtyRow[2];
                cleanRow[3] = dirtyRow[3];
                cleanRow[4] = dirtyRow[4];
                cleanRow[5] = dirtyRow[5];
                prescriptionClean.Rows.Add(cleanRow);
            }

            Console.WriteLine("Connecting to database...");
 
             //Now the connection to the database can start
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

                     //The connection is open and adding tables to the database can begin...
                     //...after we make sure the database is empty. Drop order is important because there are foreign keys!
                     Console.WriteLine("Clearing database...");
                     RunSQLQuery(connection, "DROP TABLE IF EXISTS prescription, patient, doctor, drug");
                     RunSQLQuery(connection, "DROP VIEW IF EXISTS patientdrug");

                     Console.WriteLine("Creating tables...");
                     // the tables need to be created
                     RunSQLQuery(connection, SQLCreateTableString(doctorClean));
                     RunSQLQuery(connection, SQLCreateTableString(drugClean));
                     RunSQLQuery(connection, SQLCreateTableString(patientClean));
                     RunSQLQuery(connection, SQLCreateTableString(prescriptionClean));

                     // they're nice tables, but with no keys defined to link them up into a real database
                     Console.WriteLine("Assigning keys...");
                     RunSQLQuery(connection, "ALTER TABLE doctor ADD PRIMARY KEY (doctor_id)");
                     RunSQLQuery(connection, "ALTER TABLE patient ADD PRIMARY KEY (patient_id)");
                     RunSQLQuery(connection, "ALTER TABLE drug ADD PRIMARY KEY (drug_code)");

                     RunSQLQuery(connection, "ALTER TABLE patient ADD FOREIGN KEY (doctor_id) REFERENCES doctor(doctor_id)");
                     RunSQLQuery(connection, "ALTER TABLE prescription ADD FOREIGN KEY (doctor_id) REFERENCES doctor(doctor_id)");
                     RunSQLQuery(connection, "ALTER TABLE prescription ADD FOREIGN KEY (patient_id) REFERENCES patient(patient_id)");
                     RunSQLQuery(connection, "ALTER TABLE prescription ADD FOREIGN KEY (drug_code) REFERENCES drug(drug_code)");


                     // and populated with data. Again the order tables are populated is sensitive due to the presence
                     // of foreign keys
                     Console.WriteLine("Populating tables...");
                     foreach (DataRow row in doctorClean.Rows)
                     {
                         RunSQLQuery(connection, SQLInsertRowString(doctorClean.TableName, row));
                     }

                     foreach (DataRow row in drugClean.Rows)
                     {
                         RunSQLQuery(connection, SQLInsertRowString(drugClean.TableName, row));
                     }

                     foreach (DataRow row in patientClean.Rows)
                     {
                         RunSQLQuery(connection, SQLInsertRowString(patientClean.TableName, row));
                     }

                     foreach (DataRow row in prescriptionClean.Rows)
                     {
                         RunSQLQuery(connection, SQLInsertRowString(prescriptionClean.TableName, row));
                     }

                     Console.WriteLine("Database created!\n");

                     //now the database is done, let's create a useful view showing which patients have
                     //a prescription for which drug
                     Console.WriteLine("Creating view...");
                     RunSQLQuery(connection, "CREATE VIEW patientdrug AS SELECT patient.patient_name, drug_name FROM prescription LEFT JOIN patient ON patient.patient_id = prescription.patient_id LEFT JOIN drug ON drug.drug_code = prescription.drug_code");

                     Console.WriteLine("Retriving view...\n");
                     //and pull that view into a dataframe and print the results
                     PrintDataTable(RunSQLQuery(connection, "SELECT * FROM patientdrug"), ": ");
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


        /**
         * An extremely simple CSV parser. This will do for the current purpose, but in  
         * most projects a more comprehensive function should be used from the many available
         */
        public static DataTable CSVtoDataTable(string FilePath, char CSVdel)
        {
            DataTable table = new DataTable();

            using (StreamReader reader = new StreamReader(FilePath))
            {
                string[] headers = reader.ReadLine().Split(CSVdel);

                foreach (string header in headers)
                {
                    try
                    {
                        table.Columns.Add(header);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }

                while (!reader.EndOfStream)
                {
                    string[] rows = reader.ReadLine().Split(CSVdel);
                    DataRow newRow = table.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        newRow[i] = rows[i];
                    }
                    table.Rows.Add(newRow);
                }

            }

            return table;
        }

        /**
        * Converts the DataTable data type string to the best SQL match 
        *     
         * This should have considerably more range and safeguards if it 
         * was being used without prior knowledge of exactly what is going in
         */
        public static String SQLDataType(string type)
        {
            if (type.Equals("System.String"))
                return new String("varchar(255)");
            if (type.Equals("System.DateTime"))
                return new String("date");
            if (type.Equals("System.Decimal"))
                return new String("float");
            return type;
        }

        /*
         * This should have considerably more range and safeguards if it 
         * was being used without prior knowledge of exactly what is going in
         */
        public static String SQLCreateTableString(DataTable table)
        {
            String output = new String("");

            output += "CREATE TABLE " + table.TableName;
            output += " ( ";

            foreach (DataColumn column in table.Columns) 
            {
                output += column.ColumnName + " " + SQLDataType(column.DataType.ToString()) + " NOT NULL, ";
            }

            //clean up last comma
            output = output.Remove(output.Length - 2);

            output += " )";

            return output;
        }

        /*
         * This should have considerably more range and safeguards if it 
         * was being used without prior knowledge of exactly what is going in
         */
        public static String SQLInsertRowString(String tableName, DataRow row)
        {
            String output = new String("");

            output += "INSERT INTO " + tableName;
            output += " VALUES (";

            foreach (DataColumn column in row.Table.Columns)
            {
                string data = row[column].ToString();
                if (column.DataType.ToString().Equals("System.String"))
                    output += "'" + data + "', ";
                else if (column.DataType.ToString().Equals("System.DateTime"))
                {
                    //date format conversion needs to take place to YYYY-MM-DD
                    data = String.Format("{0:u}", row[column]);
                    output += "'" + data.Remove(data.Length - 10) + "', ";
                }                    
                else
                    output += data + ", ";
            }

            //clean up last comma
            output = output.Remove(output.Length - 2);

            output += " )";

            return output;
        }

    }
}

