# SqlServerCellLevelEncryption
Want your applications to handle db field encryption and decryption on demand? This is where cell level encryption comes in, and it is supported by SQL Server which is a good thing. You can read about the implementation here https://carstent.com/2018/10/25/sql-server-cell-column-field-level-encryption-handled-from-your-applications/.

The solution contains a class library with encryption functionality, as well as a few integration tests that shows how to use the class library.

**TODO:**
1. You can download a copy of the AdventureWorks database from here, https://github.com/Microsoft/sql-server-samples/releases/tag/adventureworks.
2. Once you have downloaded and restored the AdventureWorks2017 database, execute the *Prep AdventureWorks Database for Cell Level Encryption.sql* file, to esnure you have the correct set up.
3. Please ensure you update the AdventureWorks2017Entities connection string in the App.config file in the *Tests* projects to point to your AdventureWorks2017 db.
