# Data Analysis Portfolio: SQL, C# .NET, Azure

This portfolio demonstrates simple projects that use a .NET framework and C# code to connect to and query Azure SQL databases. Screenshots of outputs are included in .png files.

## AzureSQLTest.cs

A starting test of the code to connect to and query an Azure SQL database, later expanded to interrogate the data via various SQL SELECT queries and display the answers to set questions. The dataset used is the AdventureWorks sample database.

## AzureSQL-DDL-DML.cs

A small demonstration of taking a collection of CSV files and turning them into a database. 

## WrecksData.cs (IN PROGRESS)

Investigating the Wrecks_and_Obstructions_Text_File (retrieved 14/08/2023), available from https://datahub.admiralty.co.uk under the Open Government License (https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/). 

Significant findings during cleaning:

Doubled wreck_id entries:
- 12490, rows 8361 and 8366: slightly different locational data, only row 8361 contains sonar data
- 12486, rows 8399 and 8418: slightly different locational data, only row 8418 contains limits
- 48548, rows 30958 and 87610: slightly different locational data, only row 87610 contains limits and note
- 83361, rows 43865 and 43870: appears to represent bow and stern of the same wreck seperately
- 6899, rows 70980 and 70991: appears to represent two parts of the same wreck seperately
- 34895, rows 87293 and 87541: slightly different locational data, row 87293 contains considerably more supplemental data
- 34888, rows 87294 and 87295: slightly different locational data, row 87294 contains considerably more supplemental data
- 53957, rows 87612 and 87623: appears to represent main and debris components of the same wreck
- 1258, rows 88267 and 96518: single difference: row 88267 contains a wreck_category entry absent in 96518
- 1276, rows 88270 and 96519: row 88270 is more up to date (see last ammended date) and contains more data
- 1255, rows 88287 and 97308: row 88287 is an update 1 day after row 97308, adding wreck_category data
- 99955, rows 91573 and 96504: several data changes, row 91573 more up to date
- 99368, rows 95054 and 96514: several data changes, row 95054 more up to date
- 101604, rows 97362 and 97371: slightly different locational data
- 37379, rows 99289 and 99290: locational and misc differences. Different last_detection_year, row 99290 more recent
- 37459, rows 99294 and 99296: slightly different locational data, row 99294 amended a few days after row 99296

Possible bad dates in original_detection_year & last_detection_year eg (in last_detection_year):
- line 558: 20161611 (day/month reversal?)
- line 57068: 19681984 (two years 1968/1984?)
- line 59507: 201501115 (too many digits?)
- line 62809: 19836 (too many digits / badly formatted yyyyMM for 198606 ?)
- line 89736: 11/06/2022 (non-standard but parseable format)
- line 92883: 13.01.2022 (non-standard but parseaable format)

Possiibility of yyyyMMdd vs yyyyddMM confusion is concerning, as it impossible to verify in all cases where dd < 13 without external reference
