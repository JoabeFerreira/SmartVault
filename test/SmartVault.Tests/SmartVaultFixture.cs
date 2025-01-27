using Dapper;
using System.Data.SQLite;

namespace SmartVault.Tests
{
    public class SmartVaultFixture : IDisposable
    {
        public SQLiteConnection Connection { get; set; }

        public const string OUTPUT_DIRECTORY = @"..\..\..\Output";
        public const string TESTS_DIRECTORY = @"..\..\..\TestFiles";
        public const string ACCOUNT_ID = "1";
        public const string SMITH_PROPERTY_CONTENT = "Smith Property: Third file content.";
        public const string NOT_MATCH_CONTENT = "Content Doesn't match.";

        public SmartVaultFixture()
        {
            Connection = new SQLiteConnection("Data Source=:memory:;Version=3;");
            Connection.Open();

            string file3 = Path.Combine(TESTS_DIRECTORY, "file3.txt");
            string file6 = Path.Combine(TESTS_DIRECTORY, "file6.txt");

            Connection.Execute("CREATE TABLE Document (Id INTEGER PRIMARY KEY,Name TEXT,FilePath TEXT,Length INTEGER,AccountId INTEGER,CreatedOn TEXT);");
            Connection.Execute("INSERT INTO Document (FilePath, AccountId) VALUES " +
                $"('file1.txt', '{ACCOUNT_ID}'), ('file2.txt', '{ACCOUNT_ID}'), ('{file3}', '{ACCOUNT_ID}'), " +
                $"('file4.txt', '{ACCOUNT_ID}'), ('file5.txt', '{ACCOUNT_ID}'), ('{file6}', '{ACCOUNT_ID}');");

            Directory.CreateDirectory(TESTS_DIRECTORY);

            File.WriteAllText(file3, SMITH_PROPERTY_CONTENT);
            File.WriteAllText(file6, NOT_MATCH_CONTENT);
        }

        public void Dispose()
        {
            Connection.Close();

            if (Directory.Exists(OUTPUT_DIRECTORY))
            {
                Directory.Delete(OUTPUT_DIRECTORY, true);
            }

            if (Directory.Exists(TESTS_DIRECTORY))
            {
                Directory.Delete(TESTS_DIRECTORY, true);
            }
        }
    }
}
