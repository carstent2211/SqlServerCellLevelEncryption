using EF_SqlServerEncryption;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using Xunit;

namespace Tests {
    public class IntegrationTests : IDisposable {
        private AdventureWorks2017Entities _adventureWorksContext;

        public IntegrationTests() {
            _adventureWorksContext = new AdventureWorks2017Entities(ConfigurationManager.ConnectionStrings["AdventureWorks2017Entities"].ToString());
        }

        [Fact]
        [Trait("TestType", "Integration")]
        [Trait("Author", "Carsten Thomsen")]
        public void ColumnValueIsCorrectlyEncrypted() {
            // Arrange
            var newCardNo = RandomNumbers(14);
            var credit = new CreditCard {
                CardNumber = newCardNo,
                CardType = "TestCard",
                ExpMonth = byte.Parse(RandomNumbers(1)),
                ExpYear = short.Parse("20" + RandomNumbers(2)),
                ModifiedDate = DateTime.UtcNow
            };

            // Act
            while (_adventureWorksContext.CreditCards.Any(c => c.CardNumber == newCardNo)) {
                newCardNo = RandomNumbers(14);
                credit.CardNumber = newCardNo;
            }

            _adventureWorksContext.CreditCards.Add(credit);
            _adventureWorksContext.SaveChanges();
            var encryptedCardNo = DbContextOperations<AdventureWorks2017Entities>.EncryptColumn("Sales.CreditCard", "CardNumber", "CardNumberEncrypted", "CreditCardID", "CardNumber", newCardNo);
            RefreshContext(_adventureWorksContext.CreditCards);

            // Assert
            Assert.Equal(_adventureWorksContext.CreditCards.FirstOrDefault(c => c.CardNumber == newCardNo).CardNumberEncrypted,
                encryptedCardNo);
            // Reverse
            _adventureWorksContext.CreditCards.Remove(_adventureWorksContext.CreditCards.FirstOrDefault(c => c.CardNumber == newCardNo));
            _adventureWorksContext.SaveChanges();
        }

        [Fact]
        [Trait("TestType", "Integration")]
        [Trait("Author", "Carsten Thomsen")]
        public void ValueIsCorrectlyEncrypted() {
            // Arrange
            var authHash = int.Parse(RandomNumbers(5));
            var newCardNo = RandomNumbers(14);

            var encryptedValue = DbContextOperations<AdventureWorks2017Entities>.EncryptValue(newCardNo, authHash);
            var decryptedValue = DbContextOperations<AdventureWorks2017Entities>.DecryptValue(encryptedValue, authHash);

            // Assert
            Assert.Equal(newCardNo, decryptedValue);
        }

        [Fact]
        [Trait("TestType", "Integration")]
        [Trait("Author", "Carsten Thomsen")]
        public void ColumnValueIsCorrectlyDecrypted() {
            // Arrange
            var newCardNo = RandomNumbers(14);
            var credit = new CreditCard {
                CardNumber = newCardNo,
                CardType = "TestCard",
                ExpMonth = byte.Parse(RandomNumbers(1)),
                ExpYear = short.Parse("20" + RandomNumbers(2)),
                ModifiedDate = DateTime.UtcNow
            };

            // Act
            while (_adventureWorksContext.CreditCards.Any(c => c.CardNumber == newCardNo)) {
                newCardNo = RandomNumbers(14);
                credit.CardNumber = newCardNo;
            }

            _adventureWorksContext.CreditCards.Add(credit);
            _adventureWorksContext.SaveChanges();
            DbContextOperations<AdventureWorks2017Entities>.EncryptColumn("Sales.CreditCard", "CardNumber", "CardNumberEncrypted", "CreditCardID", "CardNumber", newCardNo);
            RefreshContext(_adventureWorksContext.CreditCards);

            // Assert
            Assert.Equal(_adventureWorksContext.CreditCards.FirstOrDefault(c => c.CardNumber == newCardNo).CardNumber,
               DbContextOperations<AdventureWorks2017Entities>.DecryptColumn("Sales.CreditCard", "CardNumberEncrypted", "CreditCardID", "CardNumber", newCardNo));
            // Reverse
            _adventureWorksContext.CreditCards.Remove(_adventureWorksContext.CreditCards.FirstOrDefault(c => c.CardNumber == newCardNo));
            _adventureWorksContext.SaveChanges();
        }

        [Fact]
        [Trait("TestType", "Integration")]
        [Trait("Author", "Carsten Thomsen")]
        public void ValueIsCorrectlyDecrypted() {
            // Arrange
            var newCardNo = RandomNumbers(14);
            var credit = new CreditCard {
                CardNumber = newCardNo,
                CardType = "TestCard",
                ExpMonth = byte.Parse(RandomNumbers(1)),
                ExpYear = short.Parse("20" + RandomNumbers(2)),
                ModifiedDate = DateTime.UtcNow
            };

            // Act
            while (_adventureWorksContext.CreditCards.Any(c => c.CardNumber == newCardNo)) {
                newCardNo = RandomNumbers(14);
                credit.CardNumber = newCardNo;
            }

            _adventureWorksContext.CreditCards.Add(credit);
            _adventureWorksContext.SaveChanges();
            RefreshContext(_adventureWorksContext.CreditCards);
            var encryptedValue = DbContextOperations<AdventureWorks2017Entities>.EncryptValue(newCardNo, 19273);
            var decryptedValue = DbContextOperations<AdventureWorks2017Entities>.DecryptValue(encryptedValue, 19273);

            // Assert
            Assert.Equal(newCardNo, decryptedValue);
            // Reverse
            _adventureWorksContext.CreditCards.Remove(_adventureWorksContext.CreditCards.FirstOrDefault(c => c.CardNumber == newCardNo));
            _adventureWorksContext.SaveChanges();
        }

        private void RefreshContext(DbSet list) {
            var ctx = ((IObjectContextAdapter)_adventureWorksContext).ObjectContext;
            ctx.Refresh(RefreshMode.StoreWins, list);
        }

        private string RandomNumbers(int no) {
            var chars = "0123456789";
            var stringChars = new char[no];
            var random = new Random();

            for (var i = 0; i < stringChars.Length; i++) 
                stringChars[i] = chars[random.Next(chars.Length)];

            return new string(stringChars);
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _adventureWorksContext = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        void IDisposable.Dispose() {
            Dispose(true);
        }

        #endregion
    }
}