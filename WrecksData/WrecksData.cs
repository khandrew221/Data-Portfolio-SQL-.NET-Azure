using System;
using System.Data;

//first the file needs to be read. A quick check shows that it is in a human readable, tab delimated format,
//but it is too large to do a thorough assessment by eye. 
Console.WriteLine("Opening reader...\n");
using (StreamReader reader = new StreamReader("D:\\VS Projects\\AzureSQLTest\\WrecksData\\Wrecks.txt"))
{
    //the first line is a header, which can be assessed for the data to be expected
    Console.WriteLine("Reading header...");
    String line;
    line = reader.ReadLine();
    String[] headers = line.Split(new char[] { '\t' });

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
    int numberOfTokens = headers.Length;

    //the rest of the lines need to be read in and validated.
    //As the file is large, in this very early testing phase only the
    //top 10 will be read in. 

    //while ((line = reader.ReadLine()) != null)
    for (int i = 0; i < 2; i++)
    {
        line = reader.ReadLine();

        String[] tokens = line.Split(new char[] { '\t' });

        Console.WriteLine("Tokens found: " + tokens.Length);
        Console.WriteLine("Empty tokens found: " + EmptyTokensList(tokens).Count);

        foreach (String val in tokens) { Console.WriteLine(val); }

    }

}

// finds empty tokens in a list and returns a list of their indices
static List<int> EmptyTokensList(string[] tokens)
{
    List<int> output = new List<int>();

    for (int i = 0; i < tokens.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(tokens[i]))
            output.Add(i);
        else if (tokens[i].Equals("\"\"") ||        //this would be better achieved with a regex or list of strings to treat as empty 
                tokens[i].Equals("\" \"") || 
                tokens[i].Equals("\'\'") || 
                tokens[i].Equals("\' \'")) 
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

    return true;
}