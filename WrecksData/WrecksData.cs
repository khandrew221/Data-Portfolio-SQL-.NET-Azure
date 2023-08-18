using System;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;


class MainEntry
{
    static void Main(string[] args)
    {
        WrecksFileReader reader = new WrecksFileReader();
        reader.readFile("D:\\VS Projects\\AzureSQLTest\\WrecksData\\Wrecks.txt");

        //WrecksFileParser.ParseStringsToInt(reader.getColumn(0));

        Console.WriteLine("\nHeader list: ");
        foreach (String val in reader.GetDistictFromColumn(4))
        {
            Console.WriteLine(val);
        }

        reader.PrintReport();

        reader.Clean();
    }
}

class WrecksFileParser
{
    public static void ParseStringsToInt(string[] tokens)
    {
        foreach (string token in tokens)
        {
            try { Console.WriteLine(Int32.Parse(token)); } catch (Exception e) { Console.WriteLine(e); }  //will potentially output a huge list of error messages; write breakout clause?
        }
    }
}

class WrecksFileReader
{
    private String[] headers;           //holds header tokens
    private int numberOfTokens;         //the expected number of tokens, based on a validated set of headers 
    private int lineReadErrors;         //the number of content lines that could not be validated
    private List<String[]> allData = new List<string[]>();     //holds all the non-header data as initially read in, before parsing

    public void readFile(String path)
    {
        //first the file needs to be read. A quick check shows that it is in a human readable, tab delimated format,
        //but it is too large to do a thorough assessment by eye. 
        Console.WriteLine("\nOpening reader...");
        using (StreamReader reader = new StreamReader(path))
        {
            //the first line is a header, which can be assessed for the data to be expected
            Console.WriteLine("Reading header...");
            String line;
            line = reader.ReadLine();
            headers = line.Split(new char[] { '\t' });

            Console.WriteLine("Header tokens found: " + headers.Length);
            Console.WriteLine("Empty header tokens found: " + EmptyTokensList(headers).Count);

            //if the header can't be validated, exit the program.
            if (!ValidateHeader(headers))
            {
                Console.WriteLine("Header invalid. File may be corrupt or require cleaning. Exiting program.");
                Environment.Exit(1);
            }

            //we can set the expected number of tokens now
            numberOfTokens = headers.Length;

            //the rest of the lines need to be read in and validated.
            //As the file is large, in this very early testing phase only the
            //top 10 will be read in. 

            lineReadErrors = 0;

            while ((line = reader.ReadLine()) != null)
            {
                String[] tokens = line.Split(new char[] { '\t' });

                for (int j = 0; j < tokens.Length; j++)
                {
                    tokens[j] = TrimToken(tokens[j]);
                }

                if (!ValidateRow(tokens))
                {
                    Console.WriteLine("Invalid row at line ");
                    Console.WriteLine("Tokens found: " + tokens.Length);
                    Console.WriteLine("Empty tokens found: " + EmptyTokensList(tokens).Count);

                    foreach (String val in tokens)
                    {
                        Console.WriteLine(val);
                    }
                    lineReadErrors++;
                }
                else
                {
                    allData.Add(tokens);
                }
            }

            Console.WriteLine(allData.Count + " lines read in with " + lineReadErrors + " invalid lines found.");
        }

        Console.WriteLine("Closing reader.");
    }

    // finds empty tokens in an array and returns a list of their indices
    static List<int> EmptyTokensList(string[] tokens)
    {
        List<int> output = new List<int>();

        for (int i = 0; i < tokens.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(tokens[i]))
                output.Add(i);
        }

        return output;
    }

    //tests the header tokens for validity. 
    //? Additional tests for duplicate headers?  
    static bool ValidateHeader(String[] headers)
    {
        //if there are no headers, the header is invalid
        if (headers.Length == 0)
        {
            return false;
        }

        //check for empty tokens
        List<int> emptyHeaders = EmptyTokensList(headers);
        if (emptyHeaders.Count > 0)
        {
            return false;
        }

        //all tests passed; line is validated
        return true;
    }

    //tests a content line for validity 
    bool ValidateRow(String[] tokens)
    {
        if (tokens.Length != numberOfTokens)     //if there is the wrong number of tokens the line is invalid
        {
            return false;
        }

        //check if all tokens are empty. Lines with at least one nonempty entry pass.
        List<int> emptyTokens = EmptyTokensList(tokens);
        if (emptyTokens.Count == tokens.Length)
        {
            return false;
        }

        //all tests passed; line is validated
        return true;
    }

    // Trims tokens of whitespace and enclosing quotation marks
    string TrimToken(string token)
    {
        String trimToken = token;

        trimToken = trimToken.TrimStart();          //remove whitepace in front of first character
        trimToken = trimToken.TrimStart('\"');      //remove the initial " if it is present
        if (trimToken.Length > 0)
            trimToken = trimToken.TrimStart();      //remove any whitespace that might have sneaked in between " and start of data

        if (trimToken.Length > 0)
            trimToken = trimToken.TrimEnd();        //remove any trailing whitespace
        trimToken = trimToken.TrimEnd('\"');        //remove the final " if it is present
        if (trimToken.Length > 0)
            trimToken = trimToken.TrimEnd();        //remove any whitespace that might have sneaked in between end of data string and "

        return trimToken;
    }

    //returns a safe copy of a row, or an empty array for an invalid index
    public string[] getRow(int i)
    {
        if (i > -1 && i < allData.Count)
            return (string[])allData[i].Clone();

        Console.WriteLine("Invalid row index: " + i);
        return new string[0];
    }

    //returns a safe copy of a column, without header, or an empty array for an invalid index
    public string[] getColumn(int i)
    {
        if (i > -1 && i < numberOfTokens)
        {
            string[] output = new string[allData.Count];
            for (int j = 0; j < allData.Count; j++)
            {
                output[j] = allData[j][i];      //indices should be safe to use as row length enforced. Data is safe as primitive type
            } 
            return output;
        }            
        Console.WriteLine("Invalid column index: " + i);
        return new string[0];
    }

    //returns a safe copy of the header array
    public string[] GetHeaders()
    {
        return (string[])headers.Clone();
    }

    //returns a safe array of all distinct values in a column, or an empty array for an invalid index
    public string[] GetDistictFromColumn(int i)
    {
        string[] column = getColumn(i);
        if (column.Length != 0)
        {
            return column.Distinct().ToArray();
        }
        return new string[0];
    }


    //prints an analysis of the data to the console
    public void PrintReport()
    {
        Console.WriteLine("Data Reader Report:\n");
        if (allData.Count > 0)
        {
            Console.WriteLine(headers.Length + " column headers.");
            Console.WriteLine(headers.Distinct().Count() + " unique column headers.");
            Console.WriteLine(allData.Count + " lines.");

            //header stuck at the end because aligning tabs is hard
            Console.WriteLine("\nColumn\tUnique Entries\tNull Entries\tNull Column\tHeader");

            for (int i = 0; i < headers.Length; i++)
            {
                String[] columnData = getColumn(i);
                String header = headers[i];
                int uniqueEntries = columnData.Distinct().Count();
                int nullEntries = EmptyTokensList(columnData).Count;
                string nullColumn = (nullEntries == allData.Count) ? "Y" : "N";      //Y if all entries for column empty
                Console.WriteLine(String.Format("{0}\t{1}\t\t{2}\t\t{3}\t\t{4}", i, uniqueEntries, nullEntries, nullColumn, header));
            }

        } 
        else
        {
            Console.WriteLine("No data found.");
        }
    }

    //the cleaning process for the data read in, 
    public void Clean()
    {
        Console.WriteLine("\nCleaning data...");

        HashSet<int> columnsToRemove = new HashSet<int>();
        HashSet<int> rowsToRemove = new HashSet<int>();

        //first, checking the data report from PrintReport(), the required number of rows and columns is present
        //but there is no current knowledge of the cleanliness or quality of the data it contains

        //The report also shows us that there are two columns with nothing but null entries, headers "classification"
        //and "reported_year". Some checking showed that this is not a trimming error, and there really seems to be
        //no data contained in them. Therefore, those columns are redundant and can be removed.

        //Some columns contain only one distict entry, i.e. they are filled with a single data value. This is 
        //techincally redundant, though there may be legitimate reasons for this. For these purposes, those 
        //columns will be removed.

        //Filled but redundant:
        //horizontal_datum : WDG 2

        //A function, redundantColumns(), has been written to find column indices of columns that fit these criteria
        HashSet<int> redundantCols = RedundantColumns();
        columnsToRemove.UnionWith(redundantCols);
        Console.WriteLine("\nRedundant columns found: " + redundantCols.Count);

        //The wreck_id column contains one null/empty value. On the assumption that this column will represent a
        //primary key in a future database table, these should all be non-null and unique

        /*
        string[] col = getColumn(0);

        for (int i = 0; i < col.Length; i++) 
        {
            if (String.IsNullOrWhiteSpace(col[i]))
            {
                Console.WriteLine("Row " + i + " is null in column " + 0);
                foreach (string val in getRow(i))
                {
                    Console.WriteLine(val);
                }
            }
        }*/

        /*
        foreach (String val in getColumn(0))
        {
            if (String.IsNullOrWhiteSpace(val))
                Console.WriteLine("null");
        }

        foreach (String val in GetDistictFromColumn(8))
        {
            Console.WriteLine(val);
        }*/

        /*
        foreach (String val in GetDistictFromColumn(8))
        {
            Console.WriteLine(val);
        }

        foreach (String val in getColumn(8))
        {
            Console.WriteLine(val);
        }

        Console.WriteLine(getColumn(8).Length);*/
    }

    //finds redundant rows and returns a list of their indices
    //columns are redundant if they only contain a single unique entry
    //this includes null columns, which will contain null as single unique entry
    HashSet<int> RedundantColumns()
    {
        HashSet<int> output = new HashSet<int>();
        for (int i = 0; i < allData.Count; i++)
        {
            if (allData[i].Distinct().Count() == 1)
            {
                output.Add(i);
            }
        }
        return output;
    }

}




