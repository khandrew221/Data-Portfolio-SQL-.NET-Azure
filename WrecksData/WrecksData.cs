using System;
using System.Data;

using (StreamReader reader = new StreamReader("D:\\VS Projects\\AzureSQLTest\\WrecksData\\Wrecks.txt"))
{
    String line;

    /*while ((line = reader.ReadLine()) != null)
    {
        Console.WriteLine(line);
    }*/

    // 
    line = reader.ReadLine();

    String[] headers = line.Split(new char[] { '\t' });

    foreach (String val in headers) { Console.WriteLine(val); }

    /*
    for (int i = 0; i < 2; i++)
    {
        line = reader.ReadLine();

        String[] strVals = line.Split(new char[] { '\t' });

        foreach (String val in strVals) { Console.WriteLine(val); }
        
    }*/
}