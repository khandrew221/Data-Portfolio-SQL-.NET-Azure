using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;


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

    //the cleaning process for the data read in
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


        //The wreck_id column contains one null/empty value, and several duplicated values. On the assumption that this
        //column will represent a primary key in a future database table, these should all be non-null and unique
        //The column needs to be investigated.

        PrintColumnDuplicatesReport(0);

        //all duplicates consist of doubled entries. Each can be checked to see how their data matches up.
        // 

        /*
        RowDifferencesReport(LinesWithValueInColumn("12490", 0).ToList());
        */


        //wreck_id values to check: 12490, 12486, 48548, 83361, 6899, 34895, 34888, 53957, 1258, 1276, 1255, 99955, 99368, 101604, 37379, 37459

        //12490, rows 8361 and 8366: slightly different locational data, only row 8361 contains sonar data
        //12486, rows 8399 and 8418: slightly different locational data, only row 8418 contains limits
        //48548, rows 30958 and 87610: slightly different locational data, only row 87610 contains limits and note
        //83361, rows 43865 and 43870: appears to represent bow and stern of the same wreck seperately
        //6899, rows 70980 and 70991: appears to represent two parts of the same wreck seperately
        //34895, rows 87293 and 87541: slightly different locational data, row 87293 contains considerably more supplemental data
        //34888, rows 87294 and 87295: slightly different locational data, row 87294 contains considerably more supplemental data
        //53957, rows 87612 and 87623: appears to represent main and debris components of the same wreck
        //1258, rows 88267 and 96518: single difference: row 88267 contains a wreck_category entry absent in 96518
        //1276, rows 88270 and 96519: row 88270 is more up to date (see last ammended date) and contains more data
        //1255, rows 88287 and 97308: row 88287 is an update 1 day after row 97308, adding wreck_category data
        //99955, rows 91573 and 96504: several data changes, row 91573 more up to date
        //99368, rows 95054 and 96514: several data changes, row 95054 more up to date
        //101604, rows 97362 and 97371: slightly different locational data
        //37379, rows 99289 and 99290: locational and misc differences. Different last_detection_year, row 99290 more recent
        //37459, rows 99294 and 99296: slightly different locational data, row 99294 amended a few days after row 99296


        //the discovery of entries for distinct parts of wrecks under the same wreck_id challenges the assumption that this field can be used as
        //a unique identifier for rows. However, there are also duplicates that appear to have been made in error (as part of an update process?)

        //How to handle these duplicates would depend on final purpose and individual double checking of data. A simplified set of recommendations, 
        //designed to maximise available data and provide unique wreck_ids for each entry at the potential cost of best available locational data:

        //where duplicates differ in date, keep only the most recent (removing rows 96519, 97308, 96504, 96514, 99289, 99296)
        rowsToRemove.Add(96519);
        rowsToRemove.Add(97308);
        rowsToRemove.Add(96504);
        rowsToRemove.Add(96514);
        rowsToRemove.Add(99289);
        rowsToRemove.Add(99296);

        //where one copy contains more data, assume it is more up to date and remove the other (removing rows 8336, 8339, 87541, 87295, 96518)
        rowsToRemove.Add(8336);
        rowsToRemove.Add(8339);
        rowsToRemove.Add(87541);
        rowsToRemove.Add(87295);
        rowsToRemove.Add(96518);

        //where two rows refer to different parts of the same wreck, assign a new unique wreck_id to one of them
        AssignValue("833611", 43865, 0);
        AssignValue("689900", 70980, 0);
        AssignValue("539570", 87623, 0);

        //This leaves only rows 97362 and 97371, where best data accuracy cannot be determined between them. For example purposes, row 97371 will be
        //removed
        rowsToRemove.Add(97371);


        //now we need to deal with the entry with no wreck_id data. 
        //The following code (commented out as its print output will not be part of the cleaning report) will find and
        //show the contents of all rows with a null wreck_id value

        /*
        Console.WriteLine("\nContents of rows with null value in column 0:");
        foreach (int i in LinesWithNullInColumn(0))
        {
            Console.WriteLine("Row " + i);
            foreach (string val in getRow(i))
            {
                Console.WriteLine(val);
            }
        }*/

        //The row index is 97401, and it contains several valid looking content entries. There are several paths from here.
        //The first would be to check a previous or parallel record to see if this content can be matched to a wreck_id.
        //Since this data is not available, checks against existing rows could be performed to see if this is a duplicate,
        //partial duplicate, or hybrid of existing data. A full investigation of this would be costly in time and processing.
        //Ultimately, the row could be removed or assigned a new wreck_id, depending on the outcomes of investigation.
        //As a current shortcut, it will be assigned a new wreck_id value.
        AssignValue("0", 97401, 0);

        //This should conclude the process of making sure that all wreck_id entries are unique and non-null.





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

    HashSet<int> LinesWithValueInColumn(String s, int colInd)
    {
        HashSet<int> output = new HashSet<int>();

        string[] col = getColumn(colInd);

        for (int i = 0; i < col.Length; i++)
        {
            if (col[i].Equals(s))
            {
                output.Add(i);
            }
        }

        return output;
    }

    HashSet<int> LinesWithNullInColumn(int colInd)
    {
        HashSet<int> output = new HashSet<int>();

        string[] col = getColumn(colInd);

        for (int i = 0; i < col.Length; i++)
        {
            if(String.IsNullOrWhiteSpace(col[i]))
            {
                output.Add(i);
            }
        }

        return output;
    }

    //will return a map of all values duplicated in a column and how many times they are duplicated
    public Dictionary<string, int> GetDuplicatesFromColumn(int i)
    {
        Dictionary<string, int> output = new Dictionary<string, int>();
        List<string> column = getColumn(i).ToList<string>();

        if (column.Count != 0)
        {
            output = column.GroupBy(x => x).Where(g => g.Count() > 1).ToDictionary(x => x.Key, x => x.Count());
            return output;
        }
        return output;
    }

    public void PrintColumnDuplicatesReport(int i)
    {
        Dictionary<string, int> duplicatesMap = GetDuplicatesFromColumn(0);
        if (duplicatesMap.Count != 0)
        {
            Console.WriteLine("\nColumn " + i + " duplicates report:");
            foreach (string s in duplicatesMap.Keys)
            {
                Console.WriteLine(s + ": " + duplicatesMap[s]);
            }
        }
    }

    /*
     * Returns a list of column indices where the two provided rows' contents matches 
     */
    public static List<int> RowMatches(string[] row1, string[] row2)
    {
        List<int> output = new List<int>();
        if (row1.Length == row2.Length)
        {
            for (int i = 0; i < row1.Length; i++)
            {
                if (row1[i].Equals(row2[i]))
                {
                    output.Add(i);
                }
            }
        }
        else
        {
            Console.WriteLine("Error: rows not the same length");
        }
        return output;
    }

    public void RowDifferencesReport(List<int>rowInds)
    {

        if (rowInds.Count > 1) 
        {
            List<string[]> rows = new List<string[]>();

            foreach (int i in rowInds)
            {
                string[] row = getRow(i);
                rows.Add(row);
            }

            Console.WriteLine(String.Format("\nDifferences between rows " + String.Join(", ", rowInds)));

            int diffs = 0;
            for (int i = 0; i < numberOfTokens; i++)    //assumes correct number of tokens, should be okay with getRow()
            {
                //all tokens from column
                List<string> entries = new List<string>();
                foreach (string[] tokens in rows)
                {
                    entries.Add(tokens[i]);

                    if (entries.Distinct().Count() > 1)     //more than one distinct entry: show differences
                    {
                        Console.WriteLine("Column " + i + ": " + String.Join(", ", entries));
                        diffs++;
                    }
                }
            }
            //no differences message
            if (diffs == 0)
            {
                Console.WriteLine("No differences found.");
            }
        } 
        else
        {
            Console.WriteLine("No differences: only " + rowInds.Count + " row(s) to compare.");
        }
    }

    //tries to assign a value to a position in allData. Returns true if successful.
    private bool AssignValue(string val, int row, int column)
    {
        try
        {
            allData[row][column] = val;
            return true;
        }
        catch (Exception e) 
        {
            Console.WriteLine("Assign value failed: " + e.Message);
            return false;
        }
    }

}




