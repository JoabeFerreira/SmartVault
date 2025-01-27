using Microsoft.Extensions.Configuration;
using System;
using System.Data.SQLite;
using System.IO;

namespace SmartVault.DataGeneration
{
    public class SQLiteConfigurationManager
    {
        private IConfigurationRoot? _config;
        private string _databaseName = string.Empty;

        public SQLiteConfigurationManager InitializeConfig()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();

            _databaseName = _config?["DatabaseFileName"] ?? string.Empty;
            return this;
        }

        public SQLiteConfigurationManager CreateDatabaseFile()
        {
            SQLiteConnection.CreateFile(_databaseName);
            return this;
        }

        public SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection(string.Format(_config?["ConnectionStrings:DefaultConnection"] ?? string.Empty, _databaseName));
        }
    }
}
