using System.IO;
using Xunit;
using TestEpisoft_QD.Services;



namespace TestEpisoft_QD.Tests
{
    public class CsvReaderTests
    {
        [Fact]
        public void ReadTransactions_LigneInvalide_NeDoitPasPlanter()
        {
            // Arrange
            var reader = new CsvReader();

            var path = "test_invalid.csv";

            File.WriteAllText(path,
        @"Date,Description,Amount
2023-10-01,Test,-10.50
BADDATE,Coffee,-2.00");

            // Act
            var result = reader.ReadTransactions(path, "Bank");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void ReadTransactions_FichierValide_DoiventLireLesTransactions()
        {
            // Arrange
            var reader = new CsvReader();

            var path = "test_valid.csv";

            File.WriteAllText(path,
        @"Date,Description,Amount
2023-10-01,Test,-10.50
2023-10-02,Coffee,-2.00");

            // Act
            var result = reader.ReadTransactions(path, "Bank");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Test", result[0].Description);
            Assert.Equal(-10.50m, result[0].Amount);
            Assert.Equal("Bank", result[0].Source);
        }

        [Fact]
        public void ReadTransactions_FichierVide_DoiventRetournerListeVide()
        {
            var reader = new CsvReader();

            var path = "empty.csv";

            File.WriteAllText(path, "Date,Description,Amount");

            var result = reader.ReadTransactions(path, "Bank");

            Assert.Empty(result);
        }

    }
}