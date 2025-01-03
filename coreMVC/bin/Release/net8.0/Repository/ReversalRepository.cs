using CoreMVC3.Models;
using Microsoft.Data.SqlClient;
//using ResultModificationApp.Models;

namespace CoreMVC.Repository
{
    public class ReversalRepository
    {
        public bool ResetPreviousWinners(string CurrentPeriod)
        {
            bool success = true;

            string connstr = LocalClass.dbconn;
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            string sql = "update MPlayer set ";
            sql = sql + "IsWin = null, ";
            sql = sql + "WinMoney = '0', ";
            sql = sql + "WinMoneyWithCapital = '0' ";
            sql = sql + "where currentPeriod = '" + CurrentPeriod + "' ";
            SqlCommand cmd = new SqlCommand(sql, conn);
            int recs = cmd.ExecuteNonQuery();

            sql = "update GameDealerMPlayer set ";
            sql = sql + "IsWin = null, ";
            sql = sql + "WinMoney = '0', ";
            sql = sql + "WinMoneyWithCapital = '0' ";
            sql = sql + "where currentPeriod = '" + CurrentPeriod + "' ";
            SqlCommand cmd2 = new SqlCommand(sql, conn);
            recs = cmd2.ExecuteNonQuery();

            return success;
        }
    }
}
