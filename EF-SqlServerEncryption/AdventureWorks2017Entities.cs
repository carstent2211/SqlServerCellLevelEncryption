using System;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Security.Authentication;

namespace EF_SqlServerEncryption {
    public partial class AdventureWorks2017Entities {
        private byte[] _appRoleCookie;

        public AdventureWorks2017Entities(string connectionString) : base("name=AdventureWorks2017Entities") {
            Database.Connection.Open();
            //SetAppRole();
        }

        private void SetAppRole() {
            try {
                var sql = string.Concat(@"DECLARE @cookie VARBINARY(8000); ",
                                        @"DECLARE @r INT; ",
                                        @"EXEC sp_setapprole '", ConfigurationManager.AppSettings["ApplicationRole"].ToString(),
                                        "', '", ConfigurationManager.AppSettings["ApplicationRolePwd"].ToString(),
                                        "', @fCreateCookie = true, @cookie = @cookie OUTPUT; ",
                                        "SELECT @cookie;");
                _appRoleCookie = Database.SqlQuery<byte[]>(sql).First();
            }
            catch (Exception e) {
                throw new AuthenticationException(e.Message, e);
            }
        }

        private void UnSetAppRole() {
            var failed = Database.SqlQuery<bool>(@"DECLARE @result BIT; 
                                                   EXEC @result = sp_unsetapprole @cookie = " + _appRoleCookie.ToHexadecimalString() +
                                                   "; SELECT @result;").First();

            if (failed) throw new SecurityException();
        }

        private bool disposed = false;

        protected override void Dispose(bool disposing) {
            if (disposed) return;

            //UnSetAppRole();
            Database.Connection.Close();
            disposed = true;

            base.Dispose(disposing);
        }
    }
}