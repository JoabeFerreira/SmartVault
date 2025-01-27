using Dapper;
using SmartVault.DataGeneration;
using SmartVault.Program.BusinessObjects;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace SmartVault.Program
{
    public partial class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            using (SQLiteConnection connection = new ConfigurationHelper().InitializeConfig().CreateConnection())
            {
                connection.Open();

                WriteEveryThirdFileToFile(args[0], connection);

                _ = GetAllFileSizes(connection);
            }
        }

        public static long GetAllFileSizes(SQLiteConnection connection)
        {
            IEnumerable<string> documents = connection.Query<string>(
                $"select FilePath from {nameof(Document)}"
            );

            long totalFileSizes = 0;

            foreach (string doc in documents)
            {
                if (File.Exists(doc))
                {
                    totalFileSizes += new FileInfo(doc).Length;
                }
            }

            return totalFileSizes;
        }

        public static void WriteEveryThirdFileToFile(string accountId, SQLiteConnection connection)
        {
            List<string> accountDocuments = connection.Query<string>(
                $"select FilePath from {nameof(Document)} where AccountId = {accountId}"
            ).ToList();

            const string OUTPUT_DIRECTORY = @"..\..\..\Output";

            Directory.CreateDirectory(OUTPUT_DIRECTORY);

            string thirdFilesFile = Path.Combine(OUTPUT_DIRECTORY, $"Account_{accountId}_ThirdSmithProperties.txt");

            File.WriteAllText(thirdFilesFile, string.Empty);

            for (int i = 2; i < accountDocuments.Count; i += 3)
            {
                var doc = accountDocuments[i];
                if (File.Exists(doc))
                {
                    string fileContent = File.ReadAllText(doc);
                    if (fileContent.Contains("Smith Property"))
                    {
                        File.AppendAllText(thirdFilesFile, fileContent);
                    }
                }
            }
        }
    }
}