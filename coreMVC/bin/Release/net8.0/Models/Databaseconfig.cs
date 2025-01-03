using Microsoft.Data.SqlClient;

namespace ResultModificationApp.Models
{
    public class Databaseconfig
    {
        public string DbName { get; set; }
        public string ConnectionString { get; set; }
        public string Port { get; set; }

        public Databaseconfig() {
            DbName = "Server=localhost;Database=MasterGHL;User Id=sa;Password=Kamyen@72;TrustServerCertificate=true;";
            ConnectionString = "Connecting People";
            Port = "9999";
        }
    }
}
