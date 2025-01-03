using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace CoreMVC3.Classes
{
    public static class LocalClass
    {
        public static DateTime ckyDate = DateTime.Parse("2024-02-07 00:00:00");
        public static string OLottery = "oLottery31";
        public static string dbconn = "Server=localhost;Database=ThirdM;User Id=sa;Password=Kamyen@72;TrustServerCertificate=true;";

        public static string ghl_conn = "Server=192.82.60.31;Database=GHL;User Id=GHLUser;Password=@golden85092212;";
        public static string tm_conn = "Server=localhost;Database=ThirdM;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string tm2_conn = "Server=localhost;Database=ThirdM2;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string bv_conn = "Server=localhost;Database=BV;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string king_conn = "Server=localhost;Database=King4D;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string ace_conn = "Server=localhost;Database=ACE99;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string ghl55_conn = "Server=localhost;Database=GHL55;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string master_conn = "Server=localhost;Database=MasterGHL;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";

        //this is for testing purpose only
        public static string wl_conn = "Server=localhost;Database=ACE99;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string tok_conn = "Server=localhost;Database=ACE99;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string wama88_conn = "Server=localhost;Database=ACE99;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string redwin_conn = "Server=localhost;Database=ACE99;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string ofa_conn = "Server=localhost;Database=ACE99;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string naga_conn = "Server=localhost;Database=ACE99;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string mc888_conn = "Server=localhost;Database=ACE99;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string eko9_conn = "Server=localhost;Database=ACE99;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";

        public static string ghl_fullname = "";
        public static string tm_fullname = "";
        public static string tm2_fullname = "";
        public static string bv_fullname = "";
        public static string king_fullname = "";
        public static string ace_fullname = "";
        public static string ghl55_fullname = "";
        public static string master_fullname = "";

        //this is for testing purpose only
        public static string wl_fullname = "";
        public static string tok_fullnamen = "";
        public static string wama88_fullname = "";
        public static string redwin_fullname = "";
        public static string ofa_fullname = "";
        public static string naga_fullname = "";
        public static string mc888_fullname = "";
        public static string eko9_fullname = "";

        public static string gip = "";
        public static string guserId = "";
        public static string gpassword = "";

        public static string server31 = "Server=192.82.60.31;Database=GHL;User Id=GHLUser;Password=@golden85092212;TrustServerCertificate=true;";
        public static string maindb = "Server=localhost;Database=MasterGHL;User Id = sa; Password=Kamyen@72;TrustServerCertificate=true;";
        public static string processdate = "";
        public static string server31_dbname = "GHL31";
        public static int daysBefore = -1;

        public static string ghl_ip = "192.82.60.31";
        public static string ghl_uid = "GHLUser";
        public static string ghl_pwd = "@golden85092212";
        public static string ghl_name = "GHL";

        public static bool ValidateJson(string jstr3)
        {
            bool status = false;

            try
            {
                JsonDocument.Parse(jstr3);
                status = true;
            }
            catch
            {
                status = false;
            }

            return status;
        }
        public static async void loginfo(string message)
        {
            string connstr = master_conn;
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            message = message.Replace("'", "''");
            string sql = "insert into logRecord (msg) ";
            sql = sql + "values ( ";
            sql = sql + "'" + message + "');";
            SqlCommand cmd = new SqlCommand(sql, conn);
            int recs = await cmd.ExecuteNonQueryAsync();
            conn.Close();
        }
        public static async void logJson(string subject, string json)
        {
            string sql = "insert into jsonlog (subject, jvalue) values (";
            sql = sql + $"'{subject.Replace("'", "''")}', '{json.Replace("'", "''")}');";

            string connstr = master_conn;
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            SqlCommand cmd = new SqlCommand(sql, conn);
            int recs = await cmd.ExecuteNonQueryAsync();
            conn.Close();
        }
        public static void GetSystemSettings()
        {
            // Specify the path to the JSON file
            string filePath = "systemsettings.json";

            try
            {
                // Read the JSON file as a string
                string json = File.ReadAllText(filePath);

                // Deserialize the JSON string into a C# object
                var obj = JsonConvert.DeserializeObject<MyClass>(json);

                // Access the deserialized object's properties
                server31 = obj.server31;
                maindb = obj.maindb;
                dbconn = obj.maindb;
                processdate = obj.processdate;
                daysBefore = int.Parse(obj.daysBefore);

                ghl_conn = obj.ghl_conn;
                ghl55_conn = obj.ghl55_conn;
                tm_conn = obj.tm_conn;
                tm2_conn = obj.tm2_conn;
                master_conn = obj.master_conn;
                bv_conn = obj.bv_conn;
                king_conn = obj.king_conn;
                ace_conn = obj.ace_conn;

                if (processdate == "")
                {
                    ckyDate = DateTime.Now;
                }
                else
                {
                    ckyDate = DateTime.Parse(processdate);
                }

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"File not found: {filePath}");
            }
            catch (Newtonsoft.Json.JsonException)
            {
                Console.WriteLine($"Invalid JSON format in file: {filePath}");
            }
        }


        public static string ConditionalText(bool condition, string trueText, string falseText)
        {
            string result = "";

            if (condition)
            {
                result = trueText;
            }
            else { result = falseText; }

            return result;
        }

        public static int BinaryBool(string value)
        {
            int returnval = 0;
            bool testwater = false;

            bool bvalue = false;
            testwater = bool.TryParse(value, out bvalue);

            switch (bvalue)
            {
                case true:
                    returnval = 1;
                    break;
                case false:
                    returnval = 0;
                    break;
                default:
                    returnval = 0;
                    break;
            }

            return returnval;
        }
    }
}
