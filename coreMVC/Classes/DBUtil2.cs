using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using CoreMVC3.Classes;
using System.Data;
using System.Net;
using System.Xml.Linq;
using CoreMVC3.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Identity.Client;

namespace CoreMVC3.Classes
{
    public class DBUtil2
    {
        public List<MPlayerID> GetMPlayer(string dstart, string dend)
        {
            
            var localconnstr = db_master.connStr;

            string sql2 = "Select ID from MPlayer where UpdateDate between '" + dstart + "' and '" + dend + "' ";

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            List <MPlayerID> transList = new List<MPlayerID>();

            int maxcount = myDataRows.Rows.Count;

            for (int x = 0; x < maxcount; x++)
            {
                DataRow thisrow = myDataRows.Rows[x];

                MPlayerID oneBet = new MPlayerID();
                oneBet.ID = int.Parse( thisrow["ID"].ToString() );
                //oneBet.BetTime = DateTime.Parse( thisrow["ShowResultDate"].ToString() );
                //oneBet.UpdateDate = DateTime.Parse(thisrow["UpdateDate"].ToString());
                //oneBet.Bet_3 = thisrow["SelectedNums"].ToString();
                //oneBet.IsWin = bool.Parse( thisrow["IsWin"].ToString() );
                //oneBet.TOver = decimal.Parse(thisrow["Price"].ToString())
                //oneBet.Capital = decimal.Parse(thisrow["DiscountPrice"].ToString());
                //oneBet.GameDealerMemberID = int.Parse(thisrow["GameDealerMemberID"].ToString());
                //oneBet.Username = thisrow["UserName"].ToString();

                transList.Add(oneBet);

                
            }
            return transList;

            //return "";

        }

        public BetTrans GetMPlayerInfo(string ID)
        {

            var localconnstr = db_master.connStr;

            string sql2 = "Select * from MPlayer where ID = " + ID + ";";

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            List<BetTrans> transList = new List<BetTrans>();

            int maxcount = myDataRows.Rows.Count;
            BetTrans oneBet = new BetTrans();
            for (int x = 0; x < maxcount; x++)
            {
                DataRow thisrow = myDataRows.Rows[x];
                oneBet.ID = int.Parse(thisrow["ID"].ToString());
                oneBet.BetTime = DateTime.Parse(thisrow["CreateDate"].ToString());
                oneBet.UpdateDate = DateTime.Parse(thisrow["UpdateDate"].ToString());
                oneBet.Bet_3 = thisrow["SelectedNums"].ToString();
                oneBet.IsWin = bool.Parse(thisrow["IsWin"].ToString());
                oneBet.TOver = decimal.Parse(thisrow["Price"].ToString());
                oneBet.Capital = decimal.Parse(thisrow["DiscountPrice"].ToString());
                oneBet.GameDealerMemberID = int.Parse(thisrow["GameDealerMemberID"].ToString());
                oneBet.Username =  thisrow["UserName"].ToString() ;
                oneBet.Openning_Time = DateTime.Parse(thisrow["ShowResultDate"].ToString());
                oneBet.Bill_No_Ticket = thisrow["CurrentPeriod"].ToString();
            }
            return oneBet;

            //return "";

        }

        public decimal Get_MPlayer_TOver_by_Date_LotteryTypeId(string DateStart, string DateEnd, int LotteryTypeId)
        {
            var localconnstr = db_master.connStr;

            string sql = "";
            sql = sql + "select sum( cast(isnull(price, 0) as decimal) ) as TOver ";
            sql = sql + "from openrowset('SQLOLEDB', '192.82.60.148'; 'MasterUser'; '@master85092212', [MasterGHL].[dbo].[MPlayer]) a ";
            sql = sql + "inner join openrowset('SQLOLEDB', '192.82.60.148'; 'MasterUser'; '@master85092212', [MasterGHL].[dbo].[LotteryInfo]) b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where LotteryTypeID = @dbLotteryTypeID ";
            sql = sql + "and a.UpdateDate between '@dbDateStart' and '@dbDateEnd' ";

            string sql2 = sql.Replace("@dbLotteryTypeID", LotteryTypeId.ToString())
                             .Replace("@dbDateStart", DateStart)
                             .Replace("@dbDateEnd", DateEnd);

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            int maxcount = myDataRows.Rows.Count;
            decimal returnval = 0;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow dis = myDataRows.Rows[x];
                returnval = decimal.Parse( dis["TOver"].ToString() );
            }

            return returnval;
        }

        public decimal Get_MPlayer_Pending_by_Date_LotteryTypeId(string DateStart, string DateEnd, int LotteryTypeId)
        {
            var localconnstr = db_master.connStr;

            string sql = "";
            sql = sql + "select sum( ";
            sql = sql + "case ";
            sql = sql + "when iswin is null then cast(isnull(price, 0) as decimal) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as pending ";
            sql = sql + "from [MPlayer] a ";
            sql = sql + "inner join [LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where LotteryTypeID = @dbLotteryTypeID ";
            sql = sql + "and a.UpdateDate between '@dbDateStart' and '@dbDateEnd' ";

            string sql2 = sql.Replace("@dbLotteryTypeID", LotteryTypeId.ToString())
                             .Replace("@dbDateStart", DateStart)
                             .Replace("@dbDateEnd", DateEnd);

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            int maxcount = myDataRows.Rows.Count;
            decimal returnval = 0;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow dis = myDataRows.Rows[x];
                returnval = decimal.Parse(dis["pending"].ToString());
            }

            return returnval;
        }

        public decimal Get_MPlayer_AllLost_by_Date_LotteryTypeId(string DateStart, string DateEnd, int LotteryTypeId)
        {
            var localconnstr = db_master.connStr;

            string sql = "";
            sql = sql + "select sum( ";
            sql = sql + "case ";
            sql = sql + "when (iswin = 0) then cast(isnull(winMoney, 0) as decimal) - cast(isnull(DiscountPrice, 0) as decimal) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as AllLost ";
            sql = sql + "from [MPlayer] a ";
            sql = sql + "inner join [LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where LotteryTypeID = @dbLotteryTypeID ";
            sql = sql + "and a.UpdateDate between '@dbDateStart' and '@dbDateEnd' ";

            string sql2 = sql.Replace("@dbLotteryTypeID", LotteryTypeId.ToString())
                             .Replace("@dbDateStart", DateStart)
                             .Replace("@dbDateEnd", DateEnd);

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            int maxcount = myDataRows.Rows.Count;
            decimal returnval = 0;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow dis = myDataRows.Rows[x];
                returnval = decimal.Parse(dis["AllLost"].ToString());
            }

            return returnval;
        }

        public decimal Get_MPlayer_All4DWin_by_Date_LotteryTypeId(string DateStart, string DateEnd, int LotteryTypeId)
        {
            var localconnstr = db_master.connStr;

            string sql = "";
            sql = sql + "select sum( ";
            sql = sql + "case ";
            sql = sql + "when (iswin = 1) and (b.DrawTypeID between 142 and 152) then cast(isnull(winMoney, 0) as decimal) - cast(isnull(DiscountPrice, 0) as decimal) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as all4Dwin ";
            sql = sql + "from [MPlayer] a ";
            sql = sql + "inner join [LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where LotteryTypeID = @dbLotteryTypeID ";
            sql = sql + "and a.UpdateDate between '@dbDateStart' and '@dbDateEnd' ";

            string sql2 = sql.Replace("@dbLotteryTypeID", LotteryTypeId.ToString())
                             .Replace("@dbDateStart", DateStart)
                             .Replace("@dbDateEnd", DateEnd);

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            int maxcount = myDataRows.Rows.Count;
            decimal returnval = 0;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow dis = myDataRows.Rows[x];
                returnval = decimal.Parse(dis["all4Dwin"].ToString());
            }

            return returnval;
        }

        public MPlayerCalcFields Get_MPlayer_CalcFields(string DateStart, string DateEnd, int LotteryTypeId)
        {
            var localconnstr = db_master.connStr;

            string sql = "";
            sql = sql + "select ";
            sql = sql + "sum( cast(isnull(price, 0) as decimal) ) as TOver ";
            sql = sql + ", sum( ";
            sql = sql + "case ";
            sql = sql + "when iswin is null then cast(isnull(price, 0) as decimal) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as pending ";
            sql = sql + ", sum( ";
            sql = sql + "case ";
            sql = sql + "when (iswin = 0) then cast(isnull(winMoney, 0) as decimal) - cast(isnull(DiscountPrice, 0) as decimal) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as AllLost ";
            sql = sql + ", sum( ";
            sql = sql + "case ";
            sql = sql + "when (iswin = 1) and (b.DrawTypeID between 142 and 152) then cast(isnull(winMoney, 0) as decimal) - cast(isnull(DiscountPrice, 0) as decimal) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as all4Dwin ";

            sql = sql + ", sum( ";
            sql = sql + "case ";
            sql = sql + "when (iswin = 1) and (b.DrawTypeID NOT between 142 and 152) then cast(isnull(winMoney, 0) as decimal)";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as allnon4Dwin ";

            sql = sql + "from [MPlayer] a ";
            sql = sql + "inner join [LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where LotteryTypeID = @dbLotteryTypeID ";
            sql = sql + "and a.UpdateDate between '@dbDateStart' and '@dbDateEnd' ";

            string sql2 = sql.Replace("@dbLotteryTypeID", LotteryTypeId.ToString())
                             .Replace("@dbDateStart", DateStart)
                             .Replace("@dbDateEnd", DateEnd);

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            int maxcount = myDataRows.Rows.Count;
            MPlayerCalcFields mpc = new MPlayerCalcFields();

            for (int x = 0; x < maxcount; x++)
            {
                DataRow dis = myDataRows.Rows[x];
                mpc.TOVer = decimal.Parse( dis["TOver"].ToString() );
                mpc.Pending = decimal.Parse(dis["pending"].ToString());
                mpc.AllLost = decimal.Parse(dis["AllLost"].ToString());
                mpc.All4dWin = decimal.Parse(dis["all4Dwin"].ToString());
                mpc.Allnon4dWin = decimal.Parse(dis["allnon4Dwin"].ToString());
                mpc.WL = mpc.AllLost + mpc.All4dWin + mpc.Allnon4dWin;
                decimal p90 = decimal.Parse( "0.9" );
                decimal p10 = decimal.Parse("0.1");
                decimal neg = decimal.Parse("-1");
                mpc.Agent_WL = decimal.Multiply( decimal.Multiply( mpc.WL , p90) , neg);
                mpc.Com_WL = decimal.Multiply(decimal.Multiply(mpc.WL, p10), neg);
            }

            return mpc;
        }

        public List<LotteryTypeSummary> Get_LotteryTypeSummary(string DateStart, string DateEnd)
        {
            var localconnstr = db_master.connStr;

            string sql = "";
            sql = sql + "select distinct LotteryTypeName, c.LotteryTypeID ";
            sql = sql + "from [MPlayer] a ";
            sql = sql + "left join [LotteryInfo] b on a.LotteryInfoId = b.LotteryInfoID ";
            sql = sql + "left join [LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "where a.UpdateDate between '@dbDateStart' and '@dbDateEnd' ";
            sql = sql + "group by LotteryTypeName, c.LotteryTypeID ";
            sql = sql + "order by LotteryTypeName ";

            string sql2 = sql.Replace("@dbDateStart", DateStart)
                             .Replace("@dbDateEnd", DateEnd);

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            List<LotteryTypeSummary> lotsum = new List<LotteryTypeSummary>();

            int maxcount = myDataRows.Rows.Count;
            decimal returnval = 0;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow dis = myDataRows.Rows[x];
                
                LotteryTypeSummary sumone = new LotteryTypeSummary();
                sumone.LotteryTypeName = dis["LotteryTypeName"].ToString();
                sumone.LotteryTypeID = int.Parse(dis["LotteryTypeID"].ToString());
                //sumone.LotteryInfoID = int.Parse(dis["LotteryInfoID"].ToString());

                lotsum.Add(sumone);
            }

            return lotsum;
        }

        public List<LotteryTypeSummary> Get_LotteryTypeSummaryV2(string DateStart, string DateEnd)
        {
            var localconnstr = db_master.connStr;

            string sql = "";

            sql = sql + "declare @Date11 DATETIME = '@dbDateStart'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbDateEnd'; ";
            sql = sql + "drop table if exists #tempMPsummary ";
            sql = sql + "create table #tempMPsummary ( ";
            sql = sql + "LotteryTypeName nvarchar(max) null ";
            sql = sql + ", LotteryTypeID int null ";
            sql = sql + ", UserName nvarchar(max) null ";
            sql = sql + ", CurrentPeriod nvarchar(max) null ";
            sql = sql + ", ShowResultDate Datetime null ";
            sql = sql + ", UpdateDate Datetime null ";
            sql = sql + ", TOver decimal(38,4) null ";
            sql = sql + ", BetAmount decimal(38,4) null ";
            sql = sql + ", All4DWin decimal(38,4) null ";
            sql = sql + ", Allnon4DWin decimal(38,4) null ";
            sql = sql + ", AllLose decimal(38,4) null ";
            sql = sql + ", WL decimal(38,4) null ";
            sql = sql + ", IsWin bit null ";
            sql = sql + ", WinMoney decimal(38,4) null ";
            sql = sql + ", DrawTypeID int null ";
            sql = sql + ", Margin decimal null ";
            sql = sql + ", TotalWin decimal(38,4) null ";
            sql = sql + ", TotalLose decimal(38,4) null ";
            sql = sql + ", TotalPending decimal(38,4) null ";
            sql = sql + ") ";
            sql = sql + "insert into #tempMPsummary (UserName, CurrentPeriod, ShowResultDate, UpdateDate, TOver, BetAmount, IsWin, WinMoney, DrawTypeID, LotteryTypeName, LotteryTypeID) ";
            sql = sql + "select ";
            sql = sql + "a.UserName ";
            sql = sql + ", a.CurrentPeriod ";
            sql = sql + ", a.ShowResultDate ";
            sql = sql + ", a.UpdateDate ";
            sql = sql + ", cast( isnull( a.Price, 0) as decimal(38,4)) as TOver ";
            sql = sql + ", cast( isnull(a.DiscountPrice, 0) as decimal(38,4)) as BetAmount ";
            sql = sql + ", a.IsWin ";
            sql = sql + ", cast( isnull(a.WinMoney, 0) as decimal(38,4)) as WinMoney ";
            sql = sql + ", b.DrawTypeID ";
            sql = sql + ", c.LotteryTypeName ";
            sql = sql + ", c.LotteryTypeID ";
            sql = sql + "from [MPlayer] a ";
            sql = sql + "left join [LotteryInfo] b on a.LotteryInfoId = b.LotteryInfoID ";
            sql = sql + "left join [LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "where a.UpdateDate between @Date11 and @Date22 ";
            sql = sql + "Update #tempMPsummary ";
            sql = sql + "set AllLose = ( case  ";
            sql = sql + "when ( a.IsWin = 0 ) then WinMoney - BetAmount ";
            sql = sql + "else 0  ";
            sql = sql + "end  ";
            sql = sql + ") ";
            sql = sql + ", All4DWin = ( case ";
            sql = sql + "when (a.DrawTypeID between 142 and 152 and a.Iswin = 1) then WinMoney - BetAmount ";
            sql = sql + "else 0  ";
            sql = sql + "end  ";
            sql = sql + ") ";
            sql = sql + ", Allnon4DWin = ( case	 ";
            sql = sql + "when (a.DrawTypeID NOT between 142 and 152 and Iswin = 1) then WinMoney ";
            sql = sql + "else 0  ";
            sql = sql + "end  ";
            sql = sql + ") ";
            sql = sql + "from #tempMPsummary a ";
            sql = sql + "Update #tempMPsummary ";
            sql = sql + "set WL = (AllLose + All4DWin + Allnon4DWin)  ";
            sql = sql + ", Margin = BetAmount - AllLose ";
            sql = sql + ", TotalWin = case  ";
            sql = sql + "when (a.isWin = 1) then All4DWin + Allnon4DWin + BetAmount ";
            sql = sql + "else 0  ";
            sql = sql + "end  ";
            sql = sql + ", TotalLose = AllLose ";
            sql = sql + ", TotalPending = case ";
            sql = sql + "when (a.IsWin is null) then TOver ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + "from #tempMPsummary a ";
            sql = sql + "select LotteryTypeName, LotteryTypeID ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(TotalPending) as Pending ";
            sql = sql + ", sum(WL) as MemberWL ";
            sql = sql + ", (sum(WL) * 0.9) as AgentWL ";
            sql = sql + ", (sum(WL) * 0.1) as ComWL ";
            sql = sql + ", 0 as MAWL ";
            sql = sql + ", 0 as SMWL ";
            sql = sql + "from #tempMPsummary ";
            sql = sql + "group by LotteryTypeName, LotteryTypeID ";
            sql = sql + "order by LotteryTypeName ";

            string sql2 = sql.Replace("@dbDateStart", DateStart)
                             .Replace("@dbDateEnd", DateEnd);

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            List<LotteryTypeSummary> lotsum = new List<LotteryTypeSummary>();

            int maxcount = myDataRows.Rows.Count;
            decimal returnval = 0;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow dis = myDataRows.Rows[x];

                LotteryTypeSummary sumone = new LotteryTypeSummary();
                sumone.LotteryTypeName = dis["LotteryTypeName"].ToString();
                sumone.TOver = decimal.Parse(dis["TOver"].ToString());
                sumone.Pending = decimal.Parse(dis["Pending"].ToString());
                sumone.Member_WL = decimal.Parse(dis["MemberWL"].ToString());
                sumone.Agent_WL = decimal.Parse(dis["AgentWL"].ToString());
                sumone.Com_WL = decimal.Parse(dis["ComWL"].ToString());
                sumone.MA_WL = decimal.Parse(dis["MAWL"].ToString());
                sumone.SM_WL = decimal.Parse(dis["SMWL"].ToString());
                sumone.LotteryTypeID = int.Parse(dis["LotteryTypeID"].ToString());
                //sumone.LotteryInfoID = int.Parse(dis["LotteryInfoID"].ToString());

                lotsum.Add(sumone);
            }

            return lotsum;
        }

        public List<LotteryTypeSummary> Get_LotteryTypeSummaryV3(string DateStart, string DateEnd)
        {
            var localconnstr = db_master.connStr;

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate'; ";
            sql = sql + "drop table if exists #tempLotteryTypeInfo ";
            sql = sql + "Create table #tempLotteryTypeInfo ( ";
            sql = sql + "ID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), ";
            sql = sql + "LotteryTypeName nvarchar(max) null ";
            sql = sql + ", LotteryTypeId int null ";
            sql = sql + ", LotteryInfoID int null ";
            sql = sql + ", MRec int null ";
            sql = sql + ", TOver decimal(38, 4) null ";
            sql = sql + ", BetAmount decimal(38, 4) null ";
            sql = sql + ", AllLost decimal(38, 4) null ";
            sql = sql + ", All4Dwin decimal(38,4) null ";
            sql = sql + ", Allnon4DWin decimal(38, 4) null ";
            sql = sql + ", MemberWL decimal(38, 4) null ";
            sql = sql + ", Margin decimal(38, 4) null ";
            sql = sql + ", TotalWin decimal(38, 4) null ";
            sql = sql + ", TotalLose decimal(38, 4) null ";
            sql = sql + ", Pending decimal(38, 4) null ";
            sql = sql + ", AgentWL decimal(38, 4) null ";
            sql = sql + ", ComWL decimal(38, 4) null ";
            sql = sql + ", SMWL decimal(38, 4) null ";
            sql = sql + ", MAWL decimal(38, 4) null ";
            sql = sql + ") ";
            sql = sql + "insert into #tempLotteryTypeInfo (LotteryTypeName, LotteryTypeId, LotteryInfoID) ";
            sql = sql + "select distinct ";
            sql = sql + "LotteryTypeName ";
            sql = sql + ", a.LotteryTypeID ";
            sql = sql + ", b.LotteryInfoID ";
            sql = sql + "from LotteryType a ";
            sql = sql + "inner join LotteryInfo b on a.LotteryTypeID = b.LotteryTypeID ";
            sql = sql + "order by LotteryTypeName ";
            sql = sql + "update #tempLotteryTypeInfo ";
            sql = sql + "set mrec = ( ";
            sql = sql + "select count(*)  ";
            sql = sql + "from mplayer a ";
            sql = sql + "inner join LotteryInfo b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where a.updatedate between @Date11 and @Date22  ";
            sql = sql + "and a.LotteryInfoID = x.lotteryinfoid ";
            sql = sql + "and b.LotteryTypeID = x.LotteryTypeID ";
            sql = sql + ") ";
            sql = sql + ", TOver = (select sum(Price) from ";
            sql = sql + "( ";
            sql = sql + "select cast( a.Price as decimal(38, 4) ) as Price ";
            sql = sql + "from mplayer a ";
            sql = sql + "inner join LotteryInfo b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where a.updatedate between @Date11 and @Date22  ";
            sql = sql + "and a.LotteryInfoID = x.lotteryinfoid ";
            sql = sql + "and b.LotteryTypeID = x.LotteryTypeID ";
            sql = sql + ")x) ";
            sql = sql + ", BetAmount = (select sum(DiscountPrice) from ";
            sql = sql + "( ";
            sql = sql + "select cast( a.DiscountPrice as decimal(38, 4) ) as DiscountPrice ";
            sql = sql + "from mplayer a ";
            sql = sql + "inner join LotteryInfo b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where a.updatedate between @Date11 and @Date22  ";
            sql = sql + "and a.LotteryInfoID = x.lotteryinfoid ";
            sql = sql + "and b.LotteryTypeID = x.LotteryTypeID ";
            sql = sql + ")y) ";
            sql = sql + ", AllLost = (select sum(AllLost) from ";
            sql = sql + "( ";
            sql = sql + "select ( cast( a.WinMoney as decimal(38, 4) ) -  cast( a.DiscountPrice as decimal(38,4) ) )  as AllLost ";
            sql = sql + "from mplayer a ";
            sql = sql + "inner join LotteryInfo b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where a.updatedate between @Date11 and @Date22 and a.IsWin = 0 ";
            sql = sql + "and a.LotteryInfoID = x.lotteryinfoid ";
            sql = sql + "and b.LotteryTypeID = x.LotteryTypeID ";
            sql = sql + ")z) ";
            sql = sql + ", All4Dwin = isnull((select sum(All4Dwin) from ";
            sql = sql + "( ";
            sql = sql + "select ( cast( a.WinMoney as decimal(38, 4) ) -  cast( a.DiscountPrice as decimal(38,4) ) )  as All4Dwin ";
            sql = sql + "from mplayer a ";
            sql = sql + "inner join LotteryInfo b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where a.updatedate between @Date11 and @Date22 and b.DrawTypeID between 142 and 152 and a.Iswin = 1 ";
            sql = sql + "and a.LotteryInfoID = x.lotteryinfoid ";
            sql = sql + "and b.LotteryTypeID = x.LotteryTypeID ";
            sql = sql + ")j), 0) ";
            sql = sql + ", Allnon4DWin = isnull( ";
            sql = sql + "(select sum(All4Dwin) from ";
            sql = sql + "( ";
            sql = sql + "select ( cast( a.WinMoney as decimal(38, 4) ) -  0 )  as All4Dwin ";
            sql = sql + "from mplayer a ";
            sql = sql + "inner join LotteryInfo b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where a.updatedate between @Date11 and @Date22 and b.DrawTypeID NOT between 142 and 152 and a.Iswin = 1 ";
            sql = sql + "and a.LotteryInfoID = x.lotteryinfoid ";
            sql = sql + "and b.LotteryTypeID = x.LotteryTypeID ";
            sql = sql + ")k), 0) ";
            sql = sql + ", Pending = isnull( ";
            sql = sql + "(select sum(Pending) from ";
            sql = sql + "( ";
            sql = sql + "select ( cast( a.Price as decimal(38, 4) ) -  0 )  as Pending ";
            sql = sql + "from mplayer a ";
            sql = sql + "inner join LotteryInfo b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "where a.updatedate between @Date11 and @Date22 and a.iswin is null ";
            sql = sql + "and a.LotteryInfoID = x.lotteryinfoid ";
            sql = sql + "and b.LotteryTypeID = x.LotteryTypeID ";
            sql = sql + ")m), 0) ";
            sql = sql + "from #tempLotteryTypeInfo x ";
            sql = sql + "update #tempLotteryTypeInfo ";
            sql = sql + "set MemberWL = (isnull(AllLost, 0) + isnull(All4DWin, 0) + isnull(Allnon4DWin, 0)) ";
            sql = sql + ", Margin = BetAmount - AllLost ";
            sql = sql + ", TotalWin = All4DWin + Allnon4DWin + BetAmount + AllLost ";
            sql = sql + ", TotalLose = AllLost ";
            sql = sql + "from #tempLotteryTypeInfo x ";

            sql = sql + "update #tempLotteryTypeInfo ";
            sql = sql + "set AgentWL = MemberWL * 0.9 ";
            sql = sql + ", ComWL = MemberWL * 0.1 ";
            sql = sql + ", MAWL = 0 ";
            sql = sql + ", SMWL = 0 ";
            sql = sql + "from #tempLotteryTypeInfo x ";

            sql = sql + "delete from #tempLotteryTypeInfo where mrec = 0 ";
            sql = sql + "select LotteryTypeName ";
            sql = sql + ", LotteryTypeId ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(Betamount) as BetAmount ";
            sql = sql + ", sum(AllLost) as AllLost ";
            sql = sql + ", isnull(sum(All4Dwin), 0) as All4Dwin ";
            sql = sql + ", ISNULL(SUM(Allnon4DWin), 0) AS Allnon4DWin ";
            sql = sql + ", sum(MemberWL) as MemberWL ";
            sql = sql + ", sum(Margin) as Margin ";
            sql = sql + ", sum(TotalWin) as TotalWin ";
            sql = sql + ", sum(TotalLose) as TotalLose ";
            sql = sql + ", sum(Pending) as TotalPending ";
            sql = sql + ", sum(AgentWL) as AgentWL, sum(ComWL) as ComWL, sum(MAWL) as MAWL, sum(SMWL) as SMWL ";
            sql = sql + "from #tempLotteryTypeInfo ";
            sql = sql + "group by LotteryTypeName, LotteryTypeId ";
            sql = sql + "order by LotteryTypeName, LotteryTypeId ";
            

            string sql2 = sql.Replace("@dbStartDate", DateStart)
                             .Replace("@dbEndDate", DateEnd);

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            List<LotteryTypeSummary> lotsum = new List<LotteryTypeSummary>();

            int maxcount = myDataRows.Rows.Count;
            decimal returnval = 0;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow dis = myDataRows.Rows[x];

                LotteryTypeSummary sumone = new LotteryTypeSummary();
                sumone.LotteryTypeName = dis["LotteryTypeName"].ToString();
                sumone.TOver = decimal.Parse(dis["TOver"].ToString());
                sumone.Pending = decimal.Parse(dis["TotalPending"].ToString());
                sumone.Member_WL = decimal.Parse(dis["MemberWL"].ToString());
                sumone.Agent_WL = decimal.Parse(dis["AgentWL"].ToString());
                sumone.Com_WL = decimal.Parse(dis["ComWL"].ToString());
                sumone.MA_WL = decimal.Parse(dis["MAWL"].ToString());
                sumone.SM_WL = decimal.Parse(dis["SMWL"].ToString());
                sumone.LotteryTypeID = int.Parse(dis["LotteryTypeId"].ToString());
                //sumone.LotteryInfoID = int.Parse(dis["LotteryInfoID"].ToString());

                lotsum.Add(sumone);
            }

            return lotsum;
        }

        public string Get_Month_ID(string DateStart, string DateEnd)
        {
            var localconnstr = db_report.conn_str;

            string sql0 = "select Top 1 ID from rt_Month where StartDate = '" + DateStart + "' and EndDate = '" + DateEnd + "' OPTION (FAST 1); ";

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows0 = new DataTable();
            SqlCommand command0 = new SqlCommand(sql0, connection);
            command0.CommandTimeout = 60; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter0 = new SqlDataAdapter(command0);
            adapter0.Fill(myDataRows0);
            connection.Close();

            string MonthID = "";
            if (myDataRows0.Rows.Count > 0)
            {
                DataRow dr = myDataRows0.Rows[0];
                MonthID = dr["ID"].ToString();
            }
            else
            {
                MonthID = "";
            }

            return MonthID;
        }


        public List<LotteryTypeSummary> Get_LotteryTypeSummaryV3b(string DateStart, string DateEnd)
        {
            var localconnstr = db_report.conn_str;

            string sql0 = "select Top 1 ID from rt_Month where StartDate = '" + DateStart + "' and EndDate = '" + DateEnd + "' OPTION (FAST 1); ";

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows0 = new DataTable();
            SqlCommand command0 = new SqlCommand(sql0, connection);
            command0.CommandTimeout = 60; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter0 = new SqlDataAdapter(command0);
            adapter0.Fill(myDataRows0);
            connection.Close();

            string MonthID = "";
            if (myDataRows0.Rows.Count > 0)
            {
                DataRow dr = myDataRows0.Rows[0];
                MonthID = dr["ID"].ToString();
            }
            else
            {
                MonthID = "";
            }

            string sql = "";

            if (MonthID != "")
            {
                sql += "declare @date1 datetime2(3), @date2 datetime2(3) ";
                sql += "set @date1 = '@dbStartDate'  ";
                sql += "set @date2 = '@dbEndDate'  ";
                sql += "declare @id uniqueidentifier ";
                sql += "set @id = (select top 1 ID from rt_Month where StartDate = @date1 and EndDate = @date2) ";
                sql += "SELECT  ";
                sql += "[LotteryTypeName] ";
                sql += ",[LotteryTypeId] ";
                sql += ", ID as Level1_ID ";
                sql += ",[TOver] ";
                sql += ",[Pending] as TotalPending ";
                sql += ",[MemberWL] ";
                sql += ",[AgentWL] ";
                sql += ",[ComWL] ";
                sql += ",[MAWL] ";
                sql += ",[SMWL] ";
                sql += ",[WinMoney] ";
                sql += ",[DiscountPrice] ";
                sql += "from rt_Level1 where Month_ID = @id ";
                sql += "order by [LotteryTypeName] ";
                sql += ",[LotteryTypeId] ";
            }
            else
            {
                sql += "declare @date1 datetime, @date2 datetime ";
                sql += "set @date1 = '@dbStartDate' ";
                sql += "set @date2 = '@dbEndDate' ";
                sql += "select ";
                sql += "LotteryTypeName ";
                sql += ", LotteryTypeID ";
                
                sql += ", TOver = sum(TOver) ";
                sql += ", TotalPending = sum(Pending) ";
                sql += ", MemberWL = sum(MemberWL) ";
                sql += ", AgentWL = sum(AgentWL) ";
                sql += ", ComWL = sum(ComWL) ";
                sql += ", MAWL = sum(MAWL) ";
                sql += ", SMWL = sum(SMWL) ";
                sql += ", WinMoney = sum(WinMoney) ";
                sql += ", DiscountPrice = sum(DiscountPrice) ";
                sql += "from rt_mplayer ";
                sql += "where UpdateDate between @date1 and @date2 ";
                sql += "group by LotteryTypeName, LotteryTypeID ";
                sql += "order by LotteryTypeName ";
            }

            string sql2 = sql.Replace("@dbStartDate", DateStart)
                             .Replace("@dbEndDate", DateEnd);

            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            List<LotteryTypeSummary> lotsum = new List<LotteryTypeSummary>();

            int maxcount = myDataRows.Rows.Count;
            decimal returnval = 0;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow dis = myDataRows.Rows[x];

                LotteryTypeSummary sumone = new LotteryTypeSummary();
                sumone.LotteryTypeName = dis["LotteryTypeName"].ToString();
                sumone.TOver = decimal.Parse(dis["TOver"].ToString());
                sumone.Pending = decimal.Parse(dis["TotalPending"].ToString());
                sumone.Member_WL = decimal.Parse(dis["MemberWL"].ToString());
                sumone.Agent_WL = decimal.Parse(dis["AgentWL"].ToString());
                sumone.Com_WL = decimal.Parse(dis["ComWL"].ToString());
                sumone.MA_WL = decimal.Parse(dis["MAWL"].ToString());
                sumone.SM_WL = decimal.Parse(dis["SMWL"].ToString());
                sumone.LotteryTypeID = int.Parse(dis["LotteryTypeId"].ToString());
                if (MonthID != "")
                {
                    sumone.Level1_ID = dis["Level1_ID"].ToString();
                }

                lotsum.Add(sumone);
            }

            return lotsum;
        }

        public string GetLotteryTypeName(string ID)
        {

            var localconnstr = db_master.connStr;

            string sql2 = "select distinct LotteryTypeName from LotteryType where LotteryTypeID = " + ID ;

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            string TypeName = "";

            int maxcount = myDataRows.Rows.Count;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow thisrow = myDataRows.Rows[x];
                TypeName = thisrow["LotteryTypeName"].ToString();
            }
            return TypeName;

            //return "";

        }

        public List<MPlayerUser> GetMPlayerUsers(string DateStart, string DateEnd, int LotteryTypeId)
        {

            var localconnstr = db_master.connStr;

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbDateStart'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbDateEnd'; ";
            sql = sql + "drop table if exists #tempMPsummary ";
            sql = sql + "create table #tempMPsummary ( ";
            sql = sql + "UserName nvarchar(max) null ";
            sql = sql + ", CurrentPeriod nvarchar(max) null ";
            sql = sql + ", ShowResultDate Datetime null ";
            sql = sql + ", UpdateDate Datetime null ";
            sql = sql + ", TOver decimal(38, 4) null ";
            sql = sql + ", BetAmount decimal(38, 4) null ";
            sql = sql + ", All4DWin decimal(38, 4) null ";
            sql = sql + ", Allnon4DWin decimal(38, 4) null ";
            sql = sql + ", AllLose decimal(38, 4) null ";
            sql = sql + ", WL decimal(38, 4) null ";
            sql = sql + ", IsWin bit null ";
            sql = sql + ", WinMoney decimal(38, 4) null ";
            sql = sql + ", DrawTypeID int null ";
            sql = sql + ", Margin decimal(38, 4) null ";
            sql = sql + ", TotalWin decimal(38, 4) null ";
            sql = sql + ", TotalLose decimal(38, 4) null ";
            sql = sql + ", TotalPending decimal(38, 4) null ";
            sql = sql + ") ";
            sql = sql + "insert into #tempMPsummary (UserName, CurrentPeriod, ShowResultDate, UpdateDate, TOver, BetAmount, IsWin, WinMoney, DrawTypeID) ";
            sql = sql + "select ";
            sql = sql + "a.UserName ";
            sql = sql + ", a.CurrentPeriod ";
            sql = sql + ", a.ShowResultDate ";
            sql = sql + ", a.UpdateDate ";
            sql = sql + ", cast( isnull( a.Price, 0) as decimal(38, 4)) ";
            sql = sql + ", cast( isnull(a.DiscountPrice, 0) as decimal(38, 4)) ";
            sql = sql + ", a.IsWin ";
            sql = sql + ", cast( isnull(a.WinMoney, 0) as decimal(38, 4)) ";
            sql = sql + ", b.DrawTypeID ";
            sql = sql + "from [MPlayer] a ";
            sql = sql + "left join [LotteryInfo] b on a.LotteryInfoId = b.LotteryInfoID ";
            sql = sql + "left join [LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "where a.UpdateDate between @Date11 and @Date22 ";
            sql = sql + "and c.LotteryTypeID = @dbLotteryTypeID ";
            sql = sql + "Update #tempMPsummary ";
            sql = sql + "set AllLose = ( case ";
            sql = sql + "when ( a.IsWin = 0 ) then WinMoney - BetAmount ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") ";
            sql = sql + ", All4DWin = ( case ";
            sql = sql + "when (a.DrawTypeID between 142 and 152 and a.Iswin = 1) then WinMoney - BetAmount ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") ";
            sql = sql + ", Allnon4DWin = ( case ";
            sql = sql + "when (a.DrawTypeID NOT between 142 and 152 and Iswin = 1) then WinMoney ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") ";
            sql = sql + "from #tempMPsummary a ";
            sql = sql + "Update #tempMPsummary ";
            sql = sql + "set WL = (AllLose + All4DWin + Allnon4DWin) ";
            sql = sql + ", Margin = BetAmount - AllLose ";
            sql = sql + ", TotalWin = case ";
            sql = sql + "when (a.isWin = 1) then All4DWin + Allnon4DWin + BetAmount ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ", TotalLose = AllLose ";
            sql = sql + ", TotalPending = case ";
            sql = sql + "when (a.IsWin is null) then TOver ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + "from #tempMPsummary a ";
            sql = sql + "select UserName, CurrentPeriod, ShowResultDate, UpdateDate ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(BetAmount) as BetAmount ";
            sql = sql + ", sum(TotalWin) as TotalWin ";
            sql = sql + ", sum(TotalLose) as TotalLost ";
            sql = sql + ", sum(WL) as WL ";
            sql = sql + ", sum(TotalPending) as TotalPending ";
            sql = sql + "from #tempMPsummary ";
            sql = sql + "group by UserName, CurrentPeriod, ShowResultDate, UpdateDate ";
            sql = sql + "order by UserName, CurrentPeriod, ShowResultDate, UpdateDate ";

            string sql2 = sql.Replace("@dbDateStart", DateStart)
                             .Replace("@dbDateEnd", DateEnd)
                             .Replace("@dbLotteryTypeID", LotteryTypeId.ToString());

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            List<MPlayerUser> users = new List<MPlayerUser>();

            int maxcount = myDataRows.Rows.Count;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow thisrow = myDataRows.Rows[x];
                MPlayerUser user = new MPlayerUser();
                user.UserName = thisrow["UserName"].ToString();
                user.CurrentPeriod = thisrow["CurrentPeriod"].ToString();
                user.ShowResultDate = thisrow["ShowResultDate"].ToString();
                user.UpdateDate = thisrow["UpdateDate"].ToString();
                user.TOver = decimal.Parse(thisrow["TOver"].ToString());
                user.BetAmount = decimal.Parse(thisrow["BetAmount"].ToString());
                user.TotalWin = decimal.Parse(thisrow["TotalWin"].ToString());
                user.TotalLost = decimal.Parse(thisrow["TotalLost"].ToString());
                user.WL = decimal.Parse(thisrow["WL"].ToString());
                user.TotalPending = decimal.Parse(thisrow["TotalPending"].ToString());
                users.Add(user);
            }
            return users;

            //return "";

        }

        public List<MPlayerUser> GetMPlayerUsersV3b(string DateStart, string DateEnd, int LotteryTypeId, string Level1_ID)
        {

            var localconnstr = db_report.conn_str;

            string sql = "";

            if (Level1_ID == "")
            {
                sql += "declare @date1 datetime, @date2 datetime ";
                sql += "set @date1 = '@dbDateStart' ";
                sql += "set @date2 = '@dbDateEnd' ";
                sql += "SELECT ";
                sql += "UserName ";
                sql += ", CurrentPeriod ";
                sql += ", ShowResultDate ";
                sql += ", UpdateDate ";
                sql += ", MRec = COUNT(*) ";
                sql += ", TOver = SUM(TOver) ";
                sql += ", BetAmount = SUM(BetAmount) ";
                sql += ", TotalWin = SUM(TotalWin) ";
                sql += ", TotalLost = SUM(TotalLose) ";
                sql += ", WL = SUM(MemberWL) ";
                sql += ", TotalPending = SUM(Pending) ";
                sql += "FROM rt_MPlayer ";
                sql += "where UpdateDate between @date1 and @date2 ";
                sql += "and LotteryTypeId = @dbLotteryTypeID ";
                sql += "GROUP BY ";
                sql += "UserName ";
                sql += ", CurrentPeriod ";
                sql += ", ShowResultDate ";
                sql += ", UpdateDate ";
                sql += "ORDER BY ";
                sql += "UserName ";
                sql += ", CurrentPeriod ";
                sql += ", ShowResultDate ";
                sql += ", UpdateDate ";
            }
            else
            {
                sql += "declare @Level1_ID uniqueidentifier = '" + Level1_ID + "' ";
                sql += "select ";
                sql += "[ID] as Level2_ID ";
                sql += ", [UserName] ";
                sql += ",[CurrentPeriod] ";
                sql += ",[ShowResultDate] ";
                sql += ",[UpdateDate] ";
                sql += ",[TOver] ";
                sql += ",[BetAmount] ";
                sql += ",[TotalWin] ";
                sql += ",[TotalLose] as TotalLost ";
                sql += ",[MemberWL] as WL ";
                sql += ",[TotalPending] ";
                sql += "from rt_Level2 where Level1_ID = @Level1_ID ";
                sql += "order by ";
                sql += "[UserName] ";
                sql += ",[CurrentPeriod] ";
                sql += ",[ShowResultDate] ";
                sql += ",[UpdateDate] ";
            }

            string sql2 = sql.Replace("@dbDateStart", DateStart)
                             .Replace("@dbDateEnd", DateEnd)
                             .Replace("@dbLotteryTypeID", LotteryTypeId.ToString());

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            List<MPlayerUser> users = new List<MPlayerUser>();

            int maxcount = myDataRows.Rows.Count;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow thisrow = myDataRows.Rows[x];
                MPlayerUser user = new MPlayerUser();
                user.UserName = thisrow["UserName"].ToString();
                user.CurrentPeriod = thisrow["CurrentPeriod"].ToString() ;
                user.ShowResultDate = DateTime.Parse(thisrow["ShowResultDate"].ToString()).ToString("yyyy-MM-dd HH:mm:ss.fff");
                user.UpdateDate = DateTime.Parse(thisrow["UpdateDate"].ToString()).ToString("yyyy-MM-dd HH:mm:ss.fff");
                user.TOver = decimal.Parse(thisrow["TOver"].ToString());
                user.BetAmount = decimal.Parse(thisrow["BetAmount"].ToString());
                user.TotalWin = decimal.Parse(thisrow["TotalWin"].ToString());
                user.TotalLost = decimal.Parse(thisrow["TotalLost"].ToString());
                user.WL = decimal.Parse(thisrow["WL"].ToString());
                user.TotalPending = decimal.Parse(thisrow["TotalPending"].ToString());
                if (Level1_ID != "")
                {
                    user.Level2_ID = thisrow["Level2_ID"].ToString();
                }
                users.Add(user);
            }
            return users;

            //return "";

        }

        public int GetMPlayerUsersCountV2(string DateStart, string DateEnd, int LotteryTypeId)
        {
            var localconnstr = db_report.conn_str;

            string sql = "";
            sql += "declare @date1 datetime, @date2 datetime ";
            sql += "set @date1 = '@dbDateStart' ";
            sql += "set @date2 = '@dbDateEnd' ";
            sql += "SELECT count(*) as Recs from (";
            sql += "SELECT ";
            sql += "UserName ";
            sql += ", CurrentPeriod ";
            sql += ", ShowResultDate ";
            sql += ", UpdateDate ";
            sql += ", MRec = COUNT(*) ";
            sql += ", TOver = SUM(TOver) ";
            sql += ", BetAmount = SUM(BetAmount) ";
            sql += ", TotalWin = SUM(TotalWin) ";
            sql += ", TotalLost = SUM(TotalLose) ";
            sql += ", WL = SUM(MemberWL) ";
            sql += ", TotalPending = SUM(Pending) ";
            sql += "FROM rt_MPlayer ";
            sql += "where UpdateDate between @date1 and @date2 ";
            sql += "and LotteryTypeId = @dbLotteryTypeID ";
            sql += "GROUP BY ";
            sql += "UserName ";
            sql += ", CurrentPeriod ";
            sql += ", ShowResultDate ";
            sql += ", UpdateDate ";
            sql += ") x ";

            string sql2 = sql.Replace("@dbDateStart", DateStart)
                             .Replace("@dbDateEnd", DateEnd)
                             .Replace("@dbLotteryTypeID", LotteryTypeId.ToString());

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            List<MPlayerUser> users = new List<MPlayerUser>();

            int maxcount = myDataRows.Rows.Count;
            int retCount = 0;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow thisrow = myDataRows.Rows[x];
                retCount = int.Parse(thisrow["Recs"].ToString());
            }


            return retCount;

            //return "";

        }

        public int GetMPlayerUsersCount(string DateStart, string DateEnd, int LotteryTypeId)
        {

            var localconnstr = db_master.connStr;

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbDateStart'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbDateEnd'; ";
            sql = sql + "drop table if exists #tempMPsummary ";
            sql = sql + "create table #tempMPsummary ( ";
            sql = sql + "UserName nvarchar(max) null ";
            sql = sql + ", CurrentPeriod nvarchar(max) null ";
            sql = sql + ", ShowResultDate Datetime null ";
            sql = sql + ", UpdateDate Datetime null ";
            sql = sql + ", TOver decimal null ";
            sql = sql + ", BetAmount decimal null ";
            sql = sql + ", All4DWin decimal null ";
            sql = sql + ", Allnon4DWin decimal null ";
            sql = sql + ", AllLose decimal null ";
            sql = sql + ", WL decimal null ";
            sql = sql + ", IsWin bit null ";
            sql = sql + ", WinMoney decimal null ";
            sql = sql + ", DrawTypeID int null ";
            sql = sql + ", Margin decimal null ";
            sql = sql + ", TotalWin decimal null ";
            sql = sql + ", TotalLose decimal null ";
            sql = sql + ", TotalPending decimal null ";
            sql = sql + ") ";
            sql = sql + "insert into #tempMPsummary (UserName, CurrentPeriod, ShowResultDate, UpdateDate, TOver, BetAmount, IsWin, WinMoney, DrawTypeID) ";
            sql = sql + "select ";
            sql = sql + "a.UserName ";
            sql = sql + ", a.CurrentPeriod ";
            sql = sql + ", a.ShowResultDate ";
            sql = sql + ", a.UpdateDate ";
            sql = sql + ", cast( isnull( a.Price, 0) as decimal) ";
            sql = sql + ", cast( isnull(a.DiscountPrice, 0) as decimal) ";
            sql = sql + ", a.IsWin ";
            sql = sql + ", cast( isnull(a.WinMoney, 0) as decimal) ";
            sql = sql + ", b.DrawTypeID ";
            sql = sql + "from [MPlayer] a ";
            sql = sql + "left join [LotteryInfo] b on a.LotteryInfoId = b.LotteryInfoID ";
            sql = sql + "left join [LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "where a.UpdateDate between @Date11 and @Date22 ";
            sql = sql + "and c.LotteryTypeID = @dbLotteryTypeID ";
            sql = sql + "Update #tempMPsummary ";
            sql = sql + "set AllLose = ( case ";
            sql = sql + "when ( a.IsWin = 0 ) then WinMoney - BetAmount ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") ";
            sql = sql + ", All4DWin = ( case ";
            sql = sql + "when (a.DrawTypeID between 142 and 152 and a.Iswin = 1) then WinMoney - BetAmount ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") ";
            sql = sql + ", Allnon4DWin = ( case ";
            sql = sql + "when (a.DrawTypeID NOT between 142 and 152 and Iswin = 1) then WinMoney ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") ";
            sql = sql + "from #tempMPsummary a ";
            sql = sql + "Update #tempMPsummary ";
            sql = sql + "set WL = (AllLose + All4DWin + Allnon4DWin) ";
            sql = sql + ", Margin = BetAmount - AllLose ";
            sql = sql + ", TotalWin = case ";
            sql = sql + "when (a.isWin = 1) then All4DWin + Allnon4DWin + BetAmount ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ", TotalLose = AllLose ";
            sql = sql + ", TotalPending = case ";
            sql = sql + "when (a.IsWin is null) then TOver ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + "from #tempMPsummary a ";

            sql = sql + "select count(*) as Recs from ( ";
            sql = sql + "select UserName, CurrentPeriod, ShowResultDate, UpdateDate ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(BetAmount) as BetAmount ";
            sql = sql + ", sum(TotalWin) as TotalWin ";
            sql = sql + ", sum(TotalLose) as TotalLost ";
            sql = sql + ", sum(WL) as WL ";
            sql = sql + ", sum(TotalPending) as TotalPending ";
            sql = sql + "from #tempMPsummary ";
            sql = sql + "group by UserName, CurrentPeriod, ShowResultDate, UpdateDate ";
            sql = sql + ") x ";
            //sql = sql + "order by UserName, CurrentPeriod, ShowResultDate, UpdateDate ";

            string sql2 = sql.Replace("@dbDateStart", DateStart)
                             .Replace("@dbDateEnd", DateEnd)
                             .Replace("@dbLotteryTypeID", LotteryTypeId.ToString());

            SqlConnection connection = new SqlConnection(localconnstr);
            connection.Open();
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            List<MPlayerUser> users = new List<MPlayerUser>();

            int maxcount = myDataRows.Rows.Count;
            int retCount = 0;
            for (int x = 0; x < maxcount; x++)
            {
                DataRow thisrow = myDataRows.Rows[x];
                retCount = int.Parse(thisrow["Recs"].ToString());
            }

            
            return retCount;

            //return "";

        }
    }
}
