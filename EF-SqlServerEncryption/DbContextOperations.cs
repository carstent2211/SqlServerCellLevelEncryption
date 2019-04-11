using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace EF_SqlServerEncryption {
    public static class DbContextOperations<T> where T : DbContext, new() {
        private static DbContext _dBContext = null;

        static DbContextOperations() {
            _dBContext = new T();
        }

        public static byte[] EncryptValue(string value2Encrypt, int authHash) {
            var result = new byte[0];

            try {
                var dmlOpenCert = "OPEN SYMMETRIC KEY " + ConfigurationManager.AppSettings["SymmetricKey"] + " DECRYPTION BY CERTIFICATE " + ConfigurationManager.AppSettings["Cert"] + ";";
                var dmlEncrypt = "SELECT EncryptByKey(Key_GUID('" + ConfigurationManager.AppSettings["SymmetricKey"] + "'), N'" +
                    value2Encrypt + "', 1, HashBytes('SHA1', CONVERT(varbinary, " + authHash + ")));";

                var dmlCloseCert = "CLOSE SYMMETRIC KEY " + ConfigurationManager.AppSettings["SymmetricKey"] + ";";
                result = _dBContext.Database.SqlQuery<byte[]>(dmlOpenCert + dmlEncrypt + dmlCloseCert).FirstOrDefault();
            }
            catch (Exception e) {
                // TODO: Log exception
                Console.WriteLine(e.Message);
            }

            return result;
        }

        public static byte[] EncryptColumn(string tableName, string valueColName, string enryptedColName, string tableIdColName, string queryColName, string queryColValue) {
            var result = new byte[0];

            try {
                var dmlOpenCert = "OPEN SYMMETRIC KEY " + ConfigurationManager.AppSettings["SymmetricKey"] + " DECRYPTION BY CERTIFICATE " + ConfigurationManager.AppSettings["Cert"] + ";";
                var dmlUpdate = "UPDATE " + tableName + " SET " + enryptedColName + " = EncryptByKey(Key_GUID('" + ConfigurationManager.AppSettings["SymmetricKey"] + "'), " +
                    valueColName + ", 1, HashBytes('SHA1', CONVERT(varbinary, " + tableIdColName + "))) WHERE " + valueColName + " = '" + queryColValue + "';";
                var sqlSelect = "SELECT " + enryptedColName + " FROM " + tableName + " WHERE " + valueColName + " = '" + queryColValue + "';";

                var dmlCloseCert = "CLOSE SYMMETRIC KEY " + ConfigurationManager.AppSettings["SymmetricKey"] + ";";

                result = _dBContext.Database.SqlQuery<byte[]>(dmlOpenCert + dmlUpdate + sqlSelect + dmlCloseCert).FirstOrDefault();
            }
            catch (Exception e) {
                // TODO: Log exception
                Console.WriteLine(e.Message);
            }

            return result;
        }

        public static string DecryptValue(byte[] value2Decrypt, int authHash) {
            var result = string.Empty;

            try {
                var dmlOpenCert = "OPEN SYMMETRIC KEY " + ConfigurationManager.AppSettings["SymmetricKey"] + " DECRYPTION BY CERTIFICATE " + ConfigurationManager.AppSettings["Cert"] + ";";
                var dmlDecrypt = "SELECT CONVERT(nvarchar(4000), DecryptByKey(" + value2Decrypt.ToHexadecimalString() + ", 1, HashBytes('SHA1', " +
                    "CONVERT(varbinary, " + authHash + "))));";

                var dmlCloseCert = "CLOSE SYMMETRIC KEY " + ConfigurationManager.AppSettings["SymmetricKey"] + ";";

                result = _dBContext.Database.SqlQuery<string>(dmlOpenCert + dmlDecrypt + dmlCloseCert).FirstOrDefault();
            }
            catch (Exception e) {
                // TODO: Log exception
                Console.WriteLine(e.Message);
            }

            return result;
        }

        public static string DecryptColumn(string tableName, string enryptedColName, string tableIdColName, string queryColName, string queryColValue) {
            var result = string.Empty;

            try {
                var dmlOpenCert = "OPEN SYMMETRIC KEY " + ConfigurationManager.AppSettings["SymmetricKey"] + " DECRYPTION BY CERTIFICATE " + ConfigurationManager.AppSettings["Cert"] + ";";
                var dmlDecrypt = "SELECT CONVERT(nvarchar, DecryptByKey(" + enryptedColName + ", 1, HashBytes('SHA1', " +
                    "CONVERT(varbinary, " + tableIdColName + ")))) AS '" + enryptedColName + "Dec' FROM " + tableName + " WHERE " + queryColName + " = '" + queryColValue + "';";
                var dmlCloseCert = "CLOSE SYMMETRIC KEY " + ConfigurationManager.AppSettings["SymmetricKey"] + ";";

                result = _dBContext.Database.SqlQuery<string>(dmlOpenCert + dmlDecrypt + dmlCloseCert).FirstOrDefault();
            }
            catch (Exception e) {
                // TODO: Log exception
                Console.WriteLine(e.Message);
            }

            return result;
        }
    }
}