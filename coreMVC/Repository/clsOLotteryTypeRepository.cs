using ResultModificationApp.Models;
using System.Runtime.CompilerServices;
using Microsoft.Data;
using Microsoft.Data.SqlClient;
using CoreMVC3.Classes;

namespace CoreMVC.Repository
{
    public class clsOLotteryTypeRepository
    {
        public List<clsOLotteryType> GetLotteryTypes(string StartDate, string EndDate, string LType, bool ToReverse)
        {
            List<clsOLotteryType> list = new List<clsOLotteryType>();

            string sql = "";
            if (!ToReverse)
            {
                sql = sql + "select ";
                sql = sql + "LotteryTypeName, LotteryTypeID, recs = count(*) ";
                sql = sql + "from( ";
                sql = sql + "    select ";
                sql = sql + "    ID ";
                sql = sql + "    , CurrentPeriod ";
                sql = sql + "    , RealCloseTime ";
                sql = sql + "    , CloseTime ";
                sql = sql + "    , cast(CloseTime as Date) as CloseDate ";
                sql = sql + "    , IsOpen ";
                sql = sql + "    , a.LotteryTypeID ";
                sql = sql + "    , isnull(b.LotteryTypeName, '') as LotteryTypeName ";
                sql = sql + "    , Result ";
                sql = sql + "    from openrowset('SQLOLEDB', '@dbip'; '@dbuser'; '@dbpwd', [@dbname].[dbo].[oLottery]) a ";
                sql = sql + "    left join openrowset('SQLOLEDB', '@dbip'; '@dbuser'; '@dbpwd', [@dbname].[dbo].[LotteryType]) b on a.LotteryTypeID = b.LotteryTypeID ";
                sql = sql + "    where a.IsOpen = 1 ";
                sql = sql + $" and cast(CloseTime as datetime) >= cast('{StartDate} 00:00:00' as datetime) ";
                sql = sql + $" and cast(CloseTime as datetime) <= cast('{EndDate} 23:59:59' as datetime) ";
                sql = sql + "    and b.LotteryTypeName != '' ";
                sql = sql + $"    and b.LotteryTypeName like '%{LType}%'";
                sql = sql + ") x ";
                sql = sql + "group by LotteryTypeName, LotteryTypeID ";
                sql = sql + "order by LotteryTypeName ";
            }
            else
            {
                sql = sql + "select ";
                sql = sql + "LotteryTypeName, LotteryTypeID, recs = count(*) ";
                sql = sql + "from( ";
                sql = sql + "    select ";
                sql = sql + "    ID ";
                sql = sql + "    , CurrentPeriod ";
                sql = sql + "    , RealCloseTime ";
                sql = sql + "    , CloseTime ";
                sql = sql + "    , cast(CloseTime as Date) as CloseDate ";
                sql = sql + "    , IsOpen ";
                sql = sql + "    , a.LotteryTypeID ";
                sql = sql + "    , isnull(b.LotteryTypeName, '') as LotteryTypeName ";
                sql = sql + "    , Result ";
                sql = sql + "    from openrowset('SQLOLEDB', '@dbip'; '@dbuser'; '@dbpwd', [@dbname].[dbo].[oLottery]) a ";
                sql = sql + "    left join openrowset('SQLOLEDB', '@dbip'; '@dbuser'; '@dbpwd', [@dbname].[dbo].[LotteryType]) b on a.LotteryTypeID = b.LotteryTypeID ";
                sql = sql + "    where a.IsOpen = 1 ";
                sql = sql + $" and cast(CloseTime as datetime) >= cast('{StartDate} 00:00:00' as datetime) ";
                sql = sql + $" and cast(CloseTime as datetime) <= cast('{EndDate} 23:59:59' as datetime) ";
                sql = sql + "    and b.LotteryTypeName != '' ";
                sql = sql + $"    and b.LotteryTypeName not like '%{LType}%'";
                sql = sql + ") x ";
                sql = sql + "group by LotteryTypeName, LotteryTypeID ";
                sql = sql + "order by LotteryTypeName";
            }

            sql = sql.Replace("@dbip", LocalClass.ghl_ip)
                .Replace("@dbuser", LocalClass.ghl_uid) 
                .Replace("@dbpwd", LocalClass.ghl_pwd)
                .Replace("@dbname", LocalClass.ghl_name);

            //Databaseconfig myconfig = new Databaseconfig();
            string connstr = LocalClass.maindb;
            SqlConnection conn = new SqlConnection(connstr);
            SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                clsOLotteryType olottery = new clsOLotteryType();
                int i = (int) reader[1];
                string t = reader[0].ToString();
                olottery.LotteryTypeID = i;
                olottery.LotteryTypeName = t;
                olottery.Recs = (int)reader[2];

                list.Add(olottery);
            }

            return list;
        }

        public string GetCurrentPeriods(string StartDate, string EndDate, string SearchText, int TopRecords)
        {
            string result = "";

            return result;
        }
    }
}
