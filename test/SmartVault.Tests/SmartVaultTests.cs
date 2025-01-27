using Dapper;
using System.Data.SQLite;
using SmartVaultProgram = SmartVault.Program.Program;

namespace SmartVault.Test
{
    public class SmartVaultTests : IDisposable
    {
        private readonly SQLiteConnection _connection;
        private readonly string _tempDirectory;

        public SmartVaultTests()
        {
            _connection = new SQLiteConnection("Data Source=:memory:;Version=3;");
            _connection.Open();

            _connection.Execute("CREATE TABLE Document (Id INTEGER PRIMARY KEY,Name TEXT,FilePath TEXT,Length INTEGER,AccountId INTEGER,CreatedOn TEXT);");

            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
        }

        [Fact]
        public void GetAllFileSizesReturnsZeroForEmptyDatabase()
        {
            long totalFileSize = SmartVaultProgram.GetAllFileSizes(_connection);

            Assert.Equal(0, totalFileSize);
        }

        [Fact]
        public void GetAllFileSizesReturnsZeroForNonExistentFile()
        {
            _connection.Execute("INSERT INTO Document (AccountId, FilePath) VALUES ('1', 'NonExistentFile.txt')");

            long totalFileSize = SmartVaultProgram.GetAllFileSizes(_connection);

            Assert.Equal(0, totalFileSize);
        }

        [Fact]
        public void GetAllFileSizesForExistentFiles()
        {
            string filePath1 = CreateTempFile("File1.txt", "AAAAA"); // 5 bytes
            string filePath2 = CreateTempFile("File2.txt", "BBBBBBBBBB"); // 10 bytes

            _connection.Execute($"INSERT INTO Document (AccountId, FilePath) VALUES ('1', '{filePath1}'), ('1', '{filePath2}')");

            long totalFileSize = SmartVaultProgram.GetAllFileSizes(_connection);

            Assert.Equal(15, totalFileSize);
        }

        [Fact]
        public void OutputEmptyFileWhenFewerThanThreeDocumentsExist()
        {
            string filePath1 = CreateTempFile("File1.txt", "Content 1");
            string filePath2 = CreateTempFile("File2.txt", "Content 2");

            _connection.Execute($"INSERT INTO Document (AccountId, FilePath) VALUES ('1', '{filePath1}'), ('1', '{filePath2}')");
            
            SmartVaultProgram.WriteEveryThirdFileToFile("1", _connection, _tempDirectory);

            string outputFilePath = Path.Combine(_tempDirectory, "Account_1_ThirdSmithProperties.txt");
            Assert.True(File.Exists(outputFilePath));
            Assert.Empty(File.ReadAllText(outputFilePath));
        }

        [Fact]
        public void WriteEveryThirdFileToFileWhenMatchConditions()
        {
            string filePath1 = CreateTempFile("File1.txt", "Not relevant");
            string filePath2 = CreateTempFile("File2.txt", "Not relevant");
            string filePath3 = CreateTempFile("File3.txt", "Contains Smith Property");
            string filePath4 = CreateTempFile("File4.txt", "Not relevant");

            _connection.Execute($@"
            INSERT INTO Document (AccountId, FilePath) 
            VALUES 
                ('1', '{filePath1}'), 
                ('1', '{filePath2}'), 
                ('1', '{filePath3}'),
                ('1', '{filePath4}')");

            SmartVaultProgram.WriteEveryThirdFileToFile("1", _connection, _tempDirectory);

            string outputFilePath = Path.Combine(_tempDirectory, "Account_1_ThirdSmithProperties.txt");
            Assert.True(File.Exists(outputFilePath));

            string outputContent = File.ReadAllText(outputFilePath);
            Assert.Contains("Contains Smith Property", outputContent);
            Assert.DoesNotContain("Not relevant", outputContent);
        }

        private string CreateTempFile(string fileName, string content)
        {
            string filePath = Path.Combine(_tempDirectory, fileName);
            File.WriteAllText(filePath, content);
            return filePath;
        }

        public void Dispose()
        {
            _connection.Dispose();

            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
    }
}