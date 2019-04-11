USE AdventureWorks2017
GO
-- Drop login
IF EXISTS (SELECT name FROM master.sys.server_principals
  WHERE name = N'enc')
  BEGIN
	DROP LOGIN enc;
  END;
GO
-- Drop user
IF EXISTS (SELECT name 
                FROM [sys].[database_principals]
                WHERE [type] = 'S' AND name = N'enc')
BEGIN
    DROP USER enc;
END;
GO
CREATE LOGIN enc
WITH PASSWORD = '340$Uuxwp7Mcxo7Khy';
GO
CREATE USER enc FOR LOGIN enc;
GO
-- Drop master key
DROP SYMMETRIC KEY CreditCardNoKey
GO
DROP CERTIFICATE CreditCardNoCert
GO 
DROP MASTER KEY 
GO
-- Create master key
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'SomeSillyPassword@2018'
GO
-- Create certificate
CREATE CERTIFICATE CreditCardNoCert WITH SUBJECT = 'Credit card Numbers'
GO
-- Create symmetric key
CREATE SYMMETRIC KEY CreditCardNoKey WITH ALGORITHM = AES_256 ENCRYPTION BY CERTIFICATE CreditCardNoCert
GO
-- Grant permissions
GRANT VIEW DEFINITION ON CERTIFICATE::CreditCardNoCert TO "enc"
GO
GRANT VIEW DEFINITION ON SYMMETRIC KEY::CreditCardNoKey TO "enc"
GO
GRANT CONTROL ON CERTIFICATE::CreditCardNoCert TO "enc"
GO
-- Add new column to hold encrypted value
IF NOT EXISTS(SELECT 1 FROM sys.columns
  WHERE Name = N'CardNumberEncrypted'
  AND Object_ID = Object_ID(N'Sales.CreditCard'))
BEGIN
	ALTER TABLE Sales.CreditCard
	ADD CardNumberEncrypted varbinary(8000)
END
GO
-- You can encrypt the CardNumber column for all existing rows, using the below T-SQL and the symmetric key.
-- Notice how the encrypted value is copied from the original column, CardNumber, to the newly created column, CardNumberEncrypted,
-- and how the symmetric key is being opened and closed.
OPEN SYMMETRIC KEY CreditCardNoKey
DECRYPTION BY CERTIFICATE CreditCardNoCert
UPDATE Sales.CreditCard
SET CardNumberEncrypted = EncryptByKey(Key_GUID('CreditCardNoKey'), CardNumber, 1, HashBytes('SHA1', CONVERT(varbinary, CreditCardID)));
GO
CLOSE SYMMETRIC KEY CreditCardNoKey
GO
-- As a means of protection, you could set the CardNumber column to an empty string, or you can delete it, 
-- and rename the CardNumberEncrypted column to CardNumber.

-- Now that we have the encryption in place, we need to look at how you can decrypt CardNumberEncrypted.
OPEN SYMMETRIC KEY CreditCardNoKey
DECRYPTION BY CERTIFICATE CreditCardNoCert
SELECT CardNumberEncrypted AS 'CardNumberEncrypted', CONVERT(nvarchar, DecryptByKey(CardNumberEncrypted, 1, HASHBYTES('SHA1', CONVERT(varbinary, CreditCardID)))) AS 'CardNumberDecrypted' FROM Sales.CreditCard;
CLOSE SYMMETRIC KEY CreditCardNoKey
GO