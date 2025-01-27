using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SmartVault.Library;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Xml.Serialization;

namespace SmartVault.DataGeneration
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var config = new SQLiteConfigurationManager();

            config.InitializeConfig();

            config.CreateDatabaseFile();

            using (var connection = config.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    CreateBusinessObjectsTables(connection);

                    PopulateDatabaseWithFakeData(connection);

                    transaction.Commit();
                }

                PrintTableCount(connection, "Account");
                PrintTableCount(connection, "Document");
                PrintTableCount(connection, "User");
                PrintTableCount(connection, "OAuthToken");
            }
        }

        private static void PopulateDatabaseWithFakeData(SQLiteConnection connection)
        {
            var userValues = new StringBuilder();
            var accountValues = new StringBuilder();
            var documentValues = new StringBuilder();
            var oauthTokenValues = new StringBuilder();

            File.WriteAllText("TestDoc.txt", $"This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}");
            string documentPath = new FileInfo("TestDoc.txt").FullName;
            long documentLength = new FileInfo(documentPath).Length;

            int documentNumber = 0;
            for (int i = 0; i < 100; i++)
            {
                DateTime now = DateTime.Now;
                DateTime expiracy = DateTime.Now.AddMinutes(30);

                var randomDayIterator = RandomDay().GetEnumerator();
                randomDayIterator.MoveNext();

                userValues.Append($"('{i}','FName{i}','LName{i}','{randomDayIterator.Current:yyyy-MM-dd}','{i}','UserName-{i}','e10adc3949ba59abbe56e057f20f883e', '{now:yyyy-MM-dd HH:mm:ss}'),");
                accountValues.Append($"('{i}','Account{i}', '{now:yyyy-MM-dd HH:mm:ss}'),");
                oauthTokenValues.Append($"('{i}', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...', 'dXNlcjEyMy1yZWZyZXNoLWtleQ==', '{expiracy:yyyy-MM-dd HH:mm:ss}', '{now:yyyy-MM-dd HH:mm:ss}'),");

                for (int d = 0; d < 10000; d++, documentNumber++)
                {
                    documentValues.Append($"('{documentNumber}','Document{i}-{d}.txt','{documentPath}','{documentLength}','{i}', '{now:yyyy-MM-dd HH:mm:ss}'),");
                }
            }

            connection.Execute("INSERT INTO User (Id, FirstName, LastName, DateOfBirth, AccountId, Username, Password, CreatedOn) VALUES" + TrimLastComma(userValues));
            connection.Execute("INSERT INTO Account (Id, Name, CreatedOn) VALUES" + TrimLastComma(accountValues));
            connection.Execute("INSERT INTO Document (Id, Name, FilePath, Length, AccountId, CreatedOn) VALUES" + TrimLastComma(documentValues));
            connection.Execute("INSERT INTO OAuthToken (AccountId, AccessToken, RefreshToken, TokenExpiry, CreatedOn) VALUES " + TrimLastComma(oauthTokenValues));
        }

        private static string TrimLastComma(StringBuilder stringBuilder) => stringBuilder.ToString().TrimEnd(',');

        private static void CreateBusinessObjectsTables(SQLiteConnection connection)
        {
            var files = Directory.GetFiles(@"..\..\..\..\BusinessObjectSchema");
            var serializer = new XmlSerializer(typeof(BusinessObject));

            foreach (var file in files)
            {
                var businessObject = serializer.Deserialize(new StreamReader(file)) as BusinessObject;
                connection.Execute(businessObject?.Script);
            }
        }

        static IEnumerable<DateTime> RandomDay()
        {
            DateTime start = new DateTime(1985, 1, 1);
            Random gen = new Random();
            int range = (DateTime.Today - start).Days;
            while (true)
                yield return start.AddDays(gen.Next(range));
        }

        private static void PrintTableCount(SQLiteConnection connection, string table)
        {
            var data = connection.Query($"SELECT COUNT(*) FROM {table};");
            Console.WriteLine($"{table}Count: {JsonConvert.SerializeObject(data)}");
        }
    }
}