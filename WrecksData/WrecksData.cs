using System;
using System.Data;
using System.Diagnostics.Metrics;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;


class WrecksFileReader
{
    private String[] headers;           //holds header tokens
    private int numberOfTokens;         //the expected number of tokens, based on a validated set of headers 
    private int lineReadErrors;         //the number of content lines that could not be validated
    private List<String[]> allData = new List<string[]>();     //holds all the non-header data as initially read in, before parsing

    static void Main(string[] args)
    {
        WrecksFileReader reader = new WrecksFileReader();

        reader.readFile("D:\\VS Projects\\AzureSQLTest\\WrecksData\\Wrecks.txt");
    }

    public void readFile(String path)
    {
        //first the file needs to be read. A quick check shows that it is in a human readable, tab delimated format,
        //but it is too large to do a thorough assessment by eye. 
        Console.WriteLine("Opening reader...\n");
        using (StreamReader reader = new StreamReader(path))
        {
            //the first line is a header, which can be assessed for the data to be expected
            Console.WriteLine("Reading header...");
            String line;
            line = reader.ReadLine();
            headers = line.Split(new char[] { '\t' });

            Console.WriteLine("Header tokens found: " + headers.Length);
            Console.WriteLine("Empty tokens found: " + EmptyTokensList(headers).Count);

            Console.WriteLine("\nHeader list: ");
            foreach (String val in headers) { Console.WriteLine(val); }

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

}




