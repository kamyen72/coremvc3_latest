using CoreMVC.Models;
using Microsoft.Data.SqlClient;
using ResultModificationApp.Models;
using System.Collections;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace CoreMVC.Repository
{
    public class clsOLottery31Repository
    {
        public List<clsOLottery31> GetOLotteryResults(string StartDate, string EndDate, long LotteryTypeID)
        {
            List<clsOLottery31> mylist = new List<clsOLottery31>();

            string sql = "";
            sql = sql + $"select ID ";
            sql = sql + $", CurrentPeriod ";
            sql = sql + $", RealCloseTime ";
            sql = sql + $", CloseTime ";
            sql = sql + $", cast(CloseTime as Date) as CloseDate ";
            sql = sql + $", IsOpen ";
            sql = sql + $", a.LotteryTypeID ";
            sql = sql + $", isnull(b.LotteryTypeName, '') as LotteryTypeName ";
            sql = sql + $", Result ";
            sql = sql + $", TimesChanged = (select count(*) from ResultModificationLog where CurrentPeriod = a.CurrentPeriod) ";
            sql = sql + $"from[dbo].[OLottery31] a ";
            sql = sql + $"left join[dbo].[LotteryType] b on a.LotteryTypeID = b.LotteryTypeID ";
            sql = sql + $"where a.IsOpen = 1 ";
            sql = sql + $"and cast(CloseTime as datetime) >= cast('{StartDate} 00:00:00' as datetime)";
            sql = sql + $"and cast(CloseTime as datetime) <= cast('{EndDate} 23:59:59' as datetime) ";
            sql = sql + $"and b.LotteryTypeID = {LotteryTypeID} ";
            sql = sql + $"order by cast(CloseTime as datetime) asc ";

            Databaseconfig myconfig = new Databaseconfig();
            string connstr = myconfig.DbName;
            SqlConnection conn = new SqlConnection(connstr);
            SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                clsOLottery31 olottery = new clsOLottery31();
                bool dummy = false;
                long val = 0;
                olottery.OLottery31ID = (int)reader[0];
                olottery.CurrentPeriod = reader[1].ToString();
                olottery.RealCloseTime = reader[2].ToString();
                olottery.CloseTime = reader[3].ToString();
                DateTime closedatetime = (DateTime)reader[4];
                olottery.CloseDate = closedatetime.ToString("yyyy-MM-dd");
                olottery.IsOpen = (bool)reader[5];
                dummy = long.TryParse(reader[6].ToString(), out val);
                olottery.LotteryTypeID = val;
                olottery.LotteryTypeName = reader[7].ToString();
                olottery.Result = reader[8].ToString();
                int times = 0;
                dummy = int.TryParse(reader[9].ToString(), out times);
                olottery.TimesChanged = times;
                mylist.Add(olottery);
            }

            return mylist;
        }

        public bool UpdateResult(long OLottery31ID, string NewValue, string OldValue)
        {
            bool success = true;

            Databaseconfig myconfig = new Databaseconfig();
            string connstr = myconfig.DbName;
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            string sql = "update oLottery set Result = '" + NewValue + "' where ID = " + OLottery31ID.ToString() + ";";
            clsOLottery31 modifiedResult = new clsOLottery31();
            modifiedResult = GetOLotteryRecord(OLottery31ID);
            DateTime modifiedDatetime = DateTime.Now;

            string sql2 = "insert into ResultModificationLog (CurrentPeriod, OLotteryID, NewValue, PreviousValue, ModifiedDatetime) values (";
            sql2 = sql2 + "'" + modifiedResult.CurrentPeriod + "', ";
            sql2 = sql2 + "" + OLottery31ID + ", ";
            sql2 = sql2 + "'" + NewValue + "', ";
            sql2 = sql2 + "'" + OldValue + "', ";
            sql2 = sql2 + "'" + modifiedDatetime.ToString("yyyy-MM-dd HH:mm:ss") + "') ";

            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                int recs = cmd.ExecuteNonQuery();

                SqlCommand cmd2 = new SqlCommand(sql2, conn);
                recs = cmd2.ExecuteNonQuery();
                success = true;
                conn.Close();

                ReversalRepository rev = new ReversalRepository();
                success = rev.ResetPreviousWinners(modifiedResult.CurrentPeriod);
            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }

        public clsOLottery31 GetOLotteryRecord(long OLottery31ID)
        {
            clsOLottery31 myOLottery = new clsOLottery31();

            string sql = "";
            sql = sql + "select ID ";
            sql = sql + ", CurrentPeriod ";
            sql = sql + ", RealCloseTime ";
            sql = sql + ", CloseTime ";
            sql = sql + ", cast(CloseTime as Date) as CloseDate ";
            sql = sql + ", IsOpen ";
            sql = sql + ", LotteryTypeID ";
            sql = sql + ", '' as LotteryTypeName ";
            sql = sql + ", Result ";
            sql = sql + "from[dbo].[OLottery] ";
            sql = sql + "where ID = " + OLottery31ID.ToString() + ";";

            Databaseconfig myconfig = new Databaseconfig();
            string connstr = myconfig.DbName;
            SqlConnection conn = new SqlConnection(connstr);
            SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                
                bool dummy = false;
                long val = 0;
                myOLottery.OLottery31ID = (int)reader[0];
                myOLottery.CurrentPeriod = reader[1].ToString();
                myOLottery.RealCloseTime = reader[2].ToString();
                myOLottery.CloseTime = reader[3].ToString();
                DateTime closedatetime = (DateTime)reader[4];
                myOLottery.CloseDate = closedatetime.ToString("yyyy-MM-dd");
                myOLottery.IsOpen = (bool)reader[5];
                dummy = long.TryParse(reader[6].ToString(), out val);
                myOLottery.LotteryTypeID = val;
                myOLottery.LotteryTypeName = reader[7].ToString();
                myOLottery.Result = reader[8].ToString();
                
            }

            return myOLottery;
        }

        public bool IsModified(long OLottery31ID)
        {
            bool Modified = true;

            //int ModifiedRecs = 0;

            //string sql = "select count(*) as ModifiedRecs from ResultModificationLog where OLotteryID = " + OLottery31ID.ToString();

            //Databaseconfig myconfig = new Databaseconfig();
            //string connstr = myconfig.DbName;
            //SqlConnection conn = new SqlConnection(connstr);
            //SqlCommand cmd = new SqlCommand(sql, conn);
            //conn.Open();
            //SqlDataReader reader = cmd.ExecuteReader();
            //while (reader.Read())
            //{
            //    ModifiedRecs = (int)reader[0];
            //}

            //if (ModifiedRecs == 0)
            //{
            //    Modified = false;
            //}

            return Modified;
        }
    }
}
