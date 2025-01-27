using Dapper;
using SmartVault.DataGeneration;
using SmartVault.Program.BusinessObjects;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace SmartVault.Program
{
    partial class Program
    {

        static void Main(string[] args)
        {
            //if (args.Length == 0)
            //{
            //    return;
            //}

            using (SQLiteConnection connection = new ConfigurationHelper().InitializeConfig().CreateConnection())
            {
                connection.Open();

                WriteEveryThirdFileToFile("0", connection);

                GetAllFileSizes(connection);
            }
        }

        private static void GetAllFileSizes(SQLiteConnection connection)
        {
            // TODO: Implement functionality
        }

        private static void WriteEveryThirdFileToFile(string accountId, SQLiteConnection connection)
        {
            List<Document> accountDocuments = connection.Query<Document>(
                $"select * from {nameof(Document)} where AccountId = {accountId}"
            ).ToList();

            const string OUTPUT_DIRECTORY = @"..\..\..\Output";

            Directory.CreateDirectory(OUTPUT_DIRECTORY);

            string thirdFilesFile = Path.Combine(OUTPUT_DIRECTORY, $"Account_{accountId}_ThirdSmithProperties.txt");

            File.WriteAllText(thirdFilesFile, string.Empty);

            for (int i = 2; i < accountDocuments.Count; i += 3)
            {
                var doc = accountDocuments[i];
                if (File.Exists(doc.FilePath))
                {
                    string fileContent = File.ReadAllText(doc.FilePath);
                    if (fileContent.Contains("Smith Property"))
                    {
                        File.AppendAllText(thirdFilesFile, fileContent);
                    }
                }
            }
        }
    }
}