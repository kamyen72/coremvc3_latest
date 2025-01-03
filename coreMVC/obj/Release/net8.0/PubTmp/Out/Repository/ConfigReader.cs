using Microsoft.Extensions.Configuration;
using ResultModificationApp.Models;

namespace CoreMVC.Repository
{
    public class ConfigReader
    {
        private IConfiguration _configuration;

        public List<Databaseconfig> GetConfigList()
        {
            return _configuration.GetSection("DatabaseConfig").Get<List<Databaseconfig>>();
        }
    }
}
