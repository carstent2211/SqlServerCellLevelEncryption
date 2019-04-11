using System;
using System.Linq;
using System.Text;

namespace EF_SqlServerEncryption {
    public class Operations {
        private AdventureWorks2017Entities _adventureWorksContext = null;
        public void EnsureEncryptedCreditCardNoColExists() {
            using (_adventureWorksContext = new AdventureWorks2017Entities()) {
                try {
                    _adventureWorksContext.Database.ExecuteSqlCommand("ALTER TABLE Sales.CreditCard ADD CardNumberEncrypted varbinary(128)");
                    Console.WriteLine("Column CardNumberEncrypted created");
                }
                catch (Exception) {
                    Console.WriteLine("Column CardNumberEncrypted already exists");
                }
            }
        }

        public string GetFirstCreditCardNo() {
            var result = string.Empty;

            using (_adventureWorksContext = new AdventureWorks2017Entities()) {
                CreditCard ccs = _adventureWorksContext.CreditCards.FirstOrDefault();
                result = ccs.CardNumber + " - ";

                if (ccs.CardNumberEncrypted != null)
                    result += ByteArrayStringRepresentation(ccs.CardNumberEncrypted);
            }

            return result;
        }

        public byte[] EncryptCreditCardNo(string cardNo) {
            var result = new byte[0];

            try {
                _adventureWorksContext = new AdventureWorks2017Entities();
                var dmlOpenCert = "OPEN SYMMETRIC KEY CreditCardNoKey DECRYPTION BY CERTIFICATE CreditCardNoCert;";
                var dmlCloseCert = "CLOSE SYMMETRIC KEY CreditCardNoKey;";
                var dmlEncrypt = "SELECT CardNumberEncrypted = EncryptByKey(Key_GUID('CreditCardNoKey'), CardNumber, 1, HashBytes('SHA1', CONVERT(varbinary, CreditCardID))) FROM Sales.CreditCard WHERE CardNumber = '" + cardNo + "';";
                result = _adventureWorksContext.Database.SqlQuery<byte[]>(dmlOpenCert + dmlEncrypt + dmlCloseCert).FirstOrDefault();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            finally {
                _adventureWorksContext.Dispose();
            }

            return result;
        }

        public byte[] UpdateEncryptedCreditCardNo(string cardNo) {
            var result = new byte[0];

            try {
                EnsureEncryptedCreditCardNoColExists();

                _adventureWorksContext = new AdventureWorks2017Entities();

                var dmlOpenCert = "OPEN SYMMETRIC KEY CreditCardNoKey DECRYPTION BY CERTIFICATE CreditCardNoCert;";
                var dmlCloseCert = "CLOSE SYMMETRIC KEY CreditCardNoKey;";
                var dmlUpdate = "UPDATE Sales.CreditCard SET CardNumberEncrypted = EncryptByKey(Key_GUID('CreditCardNoKey'), CardNumber, 1, HashBytes('SHA1', CONVERT(varbinary, CreditCardID))) WHERE CardNumber = '" + cardNo + "';";
                var sqlSelect = "SELECT CardNumberEncrypted FROM Sales.CreditCard WHERE CardNumber = '" + cardNo + "';";

                result = _adventureWorksContext.Database.SqlQuery<byte[]>(dmlOpenCert + dmlUpdate + sqlSelect + dmlCloseCert).FirstOrDefault();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            finally {
                _adventureWorksContext.Dispose();
            }

            return result;
        }

        public string DecryptFirstCreditCardNo() {
            var result = string.Empty;

            try {
                EnsureEncryptedCreditCardNoColExists();

                _adventureWorksContext = new AdventureWorks2017Entities();
                System.Collections.Generic.IEnumerable<CreditCard> res = _adventureWorksContext.Database.SqlQuery<CreditCard>("SELECT TOP 1 * FROM Sales.CreditCard WHERE CardNumberEncrypted IS NOT NULL");

                var cardNo = res.FirstOrDefault().CardNumber;
                var dmlOpenCert = "OPEN SYMMETRIC KEY CreditCardNoKey DECRYPTION BY CERTIFICATE CreditCardNoCert;";
                var dmlDecrypt = "SELECT CONVERT(nvarchar, DecryptByKey(CardNumberEncrypted, 1, HashBytes('SHA1', " +
                    "CONVERT(varbinary, CreditCardID)))) AS 'CardNumberDec' FROM Sales.CreditCard WHERE CardNumber = '" + cardNo + "';";
                var dmlCloseCert = "CLOSE SYMMETRIC KEY CreditCardNoKey;";

                // Carsten Thomsen 07/06/2018: Surprisingly this works, with the combination of DML and a query. I got lucky as I was running out of luck
                //                             with EF closing the connection after each query or DML, meaning the Cert would be closed too, effectively
                //                             preventing encryption and decryption.
                result = _adventureWorksContext.Database.SqlQuery<string>(dmlOpenCert + dmlDecrypt + dmlCloseCert).FirstOrDefault();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            finally {
                _adventureWorksContext.Dispose();
            }

            return result;
        }

        public string DecryptCreditCardNo(string cardNo) {
            var result = string.Empty;

            try {
                EnsureEncryptedCreditCardNoColExists();

                _adventureWorksContext = new AdventureWorks2017Entities();

                var dmlOpenCert = "OPEN SYMMETRIC KEY CreditCardNoKey DECRYPTION BY CERTIFICATE CreditCardNoCert;";
                var dmlDecrypt = "SELECT CONVERT(nvarchar, DecryptByKey(CardNumberEncrypted, 1, HashBytes('SHA1', " +
                    "CONVERT(varbinary, CreditCardID)))) AS 'CardNumberDec' FROM Sales.CreditCard WHERE CardNumber = '" + cardNo + "';";
                var dmlCloseCert = "CLOSE SYMMETRIC KEY CreditCardNoKey;";

                // Carsten Thomsen 07/06/2018: Surprisingly this works, with the combination of DML and a query. I got lucky as I was running out of luck
                //                             with EF closing the connection after each query or DML, meaning the Cert would be closed too, effectively
                //                             preventing encryption and decryption.
                result = _adventureWorksContext.Database.SqlQuery<string>(dmlOpenCert + dmlDecrypt + dmlCloseCert).FirstOrDefault();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            finally {
                _adventureWorksContext.Dispose();
            }

            return result;
        }
        public static string ByteArrayStringRepresentation(byte[] ba) {
            var bytesStringRepresentation = new StringBuilder(ba.Length * 2);
            bytesStringRepresentation.Append("0x");

            foreach (var b in ba) {
                bytesStringRepresentation.AppendFormat("{0:x2}", b);
            }

            return bytesStringRepresentation.ToString().ToUpper();
        }
    }
}