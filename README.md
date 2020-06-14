A fork of existing repository https://github.com/improvedk/OrcaMDF with the following enhancements:

 - modern data types support;
 - improved formatting of the data shown in grid;
 - caching system tables only;
 - big databases support;
 - schema added to names of the tables and data types;
 - fixed some bugs (like usage of pg_first - it works mainly for the sample DBs, but doesn't for real DBs);
 - ability to generate an SQL script to fix corrupted page of database based on the page data from the file;
 - ability to export the data of the table from the backup to the SQL Server for further analysis;
 - columns with zero physical length (with default constraints);
 - skipping dropped column data;
 - skipping ghost ang ghost forwarded records;
 - plugins support;
 - collation support;
 - SQL syntax highlighting;
 - speed improvement.
