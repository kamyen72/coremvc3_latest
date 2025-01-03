using CoreMVC3.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Reflection.Metadata;
using static CoreMVC2.Controllers.APIController;
using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Hosting;
using System.Web;
using CoreMVC3.Classes;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace CoreMVC2.Controllers
{

    [Route("/[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public APIController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [EnableCors("AllowAll")]
        [Route("AjaxMethod")]
        [HttpPost]
        public IActionResult AjaxMethod([FromBody] InputModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.InputText))
            {
                return BadRequest("Input text is required.");
            }

            string inputText = model.InputText; // Extract inputText property from model

            string person = $"You said {inputText} to me just now";

            ReturnModel returnModel = new ReturnModel();
            returnModel.ReturnText = person;

            var rJason = JsonConvert.SerializeObject(returnModel);

            //return Content("application/json", rJason);

            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetCurrentPeriodPrefixs")]
        [HttpPost]
        public IActionResult GetCurrentPeriodPrefixs([FromBody] PrefixInput model)
        {
            var StartDate = model.StartDate + " 00:00:00";
            var EndDate = model.EndDate + " 23:59:59";

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate'; ";

            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( ";
            sql = sql + "[ID] int null, ";
            sql = sql + "[BET_TYPE] Nvarchar(200) null, ";
            sql = sql + "[DrawTypeID] int null, ";
            sql = sql + "[UpdateDate] datetime null, ";
            sql = sql + "[Username] Nvarchar(200) null, ";
            sql = sql + "[Bill_No_Ticket] Nvarchar(200) null ";
            sql = sql + ") ";

            sql = sql + "insert into #TempReport ";
            sql = sql + "select ";
            sql = sql + "a.ID, ";
            sql = sql + "c.LotteryTypeName as BET_TYPE, ";
            sql = sql + "DrawTypeID, ";
            sql = sql + "a.UpdateDate, ";
            sql = sql + "a.Username, ";
            sql = sql + "a.CurrentPeriod as Bill_No_Ticket ";
            sql = sql + "from [dbo].[MPlayer] a ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID ";
            sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";

            sql = sql + "select left(Bill_No_Ticket, CHARINDEX('_', Bill_No_Ticket) - 1) as Prefix, count(*) as Recs ";
            sql = sql + "from #TempReport ";
            sql = sql + "group by left(Bill_No_Ticket, CHARINDEX('_', Bill_No_Ticket) - 1) ";
            sql = sql + "order by left(Bill_No_Ticket, CHARINDEX('_', Bill_No_Ticket) - 1) ";

            string sql2 = sql.Replace("@dbip", db_master.ip)
                .Replace("@dbuser", db_master.userId)
                .Replace("@dbpwd", db_master.password)
                .Replace("@dbfullname", db_master.dbfullname)
                .Replace("@dbStartDate", StartDate)
                .Replace("@dbEndDate", EndDate);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            int maxrows = myDataRows.Rows.Count;
            PrefixList prefixlist = new PrefixList();
            prefixlist.Rows = new List<CurPeriodPrefix>();
            CurPeriodPrefix prefix = null;
            for (int i = 0; i < maxrows; i++)
            {
                prefix = new CurPeriodPrefix();
                DataRow row = myDataRows.Rows[i];

                prefix.Prefix = row["Prefix"].ToString();
                prefix.Recs = row["Recs"].ToString();
                prefixlist.Rows.Add(prefix);
            }

            string rJason = JsonConvert.SerializeObject(prefixlist.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetCurrentPeriods")]
        [HttpPost]
        public IActionResult GetCurrentPeriods([FromBody] CurPeriodInput model)
        {
            var Prefix = model.Prefix;
            var StartDate = model.StartDate + " 00:00:00";
            var EndDate = model.EndDate + " 23:59:59";

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate'; ";

            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( ";
            sql = sql + "[ID] int null,  ";
            sql = sql + "[BET_TYPE] Nvarchar(200) null,  ";
            sql = sql + "[DrawTypeID] int null,  ";
            sql = sql + "[UpdateDate] datetime null,  ";
            sql = sql + "[Username] Nvarchar(200) null,  ";
            sql = sql + "[Bill_No_Ticket] Nvarchar(200) null  ";
            sql = sql + ") ";

            sql = sql + "insert into #TempReport ";
            sql = sql + "select  ";
            sql = sql + "a.ID,  ";
            sql = sql + "c.LotteryTypeName as BET_TYPE, ";
            sql = sql + "DrawTypeID,   ";
            sql = sql + "a.UpdateDate,  ";
            sql = sql + "a.Username,  ";
            sql = sql + "a.CurrentPeriod as Bill_No_Ticket ";
            sql = sql + "from [dbo].[MPlayer] a  ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID  ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID  ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID   ";
            sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";

            sql = sql + "select ";
            sql = sql + "Bill_No_Ticket as CurrentPeriod, ";
            sql = sql + "UserName, ";
            sql = sql + "count(*) as Recs ";
            sql = sql + "from #TempReport ";
            sql = sql + "where left(Bill_No_Ticket, CHARINDEX('_', Bill_No_Ticket) - 1)  = '@dbprefix' ";
            sql = sql + "group by ";
            sql = sql + "Bill_No_Ticket, ";
            sql = sql + "UserName ";
            sql = sql + "order by  ";
            sql = sql + "Bill_No_Ticket, ";
            sql = sql + "UserName ";

            string sql2 = sql.Replace("@dbip", db_master.ip)
                .Replace("@dbuser", db_master.userId)
                .Replace("@dbpwd", db_master.password)
                .Replace("@dbfullname", db_master.dbfullname)
                .Replace("@dbStartDate", StartDate)
                .Replace("@dbEndDate", EndDate)
                .Replace("@dbprefix", Prefix);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            int maxrows = myDataRows.Rows.Count;
            CurPeriodList prefixlist = new CurPeriodList();
            prefixlist.Rows = new List<CurPeriod>();
            CurPeriod prefix = null;
            for (int i = 0; i < maxrows; i++)
            {
                prefix = new CurPeriod();
                DataRow row = myDataRows.Rows[i];

                prefix.CurrentPeriod = row["CurrentPeriod"].ToString();
                prefix.UserName = row["UserName"].ToString();
                prefix.Recs = int.Parse(row["Recs"].ToString());
                prefixlist.Rows.Add(prefix);
            }

            string rJason = JsonConvert.SerializeObject(prefixlist.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetAccountPrefixs")]
        [HttpPost]
        public IActionResult GetAccountPrefixs([FromBody] PrefixInput model)
        {
            var StartDate = model.StartDate + " 00:00:00";
            var EndDate = model.EndDate + " 23:59:59";

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate'; ";
            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( [ID] int null, [BET_TYPE] Nvarchar(200) null, [DrawTypeID] int null, [UpdateDate] datetime null, [Username] Nvarchar(200) null, [Bill_No_Ticket] Nvarchar(200) null ) ";
            sql = sql + "insert into #TempReport select a.ID, c.LotteryTypeName as BET_TYPE, DrawTypeID, a.UpdateDate, a.Username, a.CurrentPeriod as Bill_No_Ticket ";
            sql = sql + "from [dbo].[MPlayer] a ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID ";
            sql = sql + "where a.UpdateDate between @Date11 AND @Date22 and a.IsWin is not NULL ";
            sql = sql + "select ";
            sql = sql + "left(UserName, CHARINDEX('_', UserName) - 1) as Prefix, ";
            sql = sql + "count(*) as Recs ";
            sql = sql + "from #TempReport ";
            sql = sql + "group by left(UserName, CHARINDEX('_', UserName) - 1) ";
            sql = sql + "order by left(UserName, CHARINDEX('_', UserName) - 1) ";

            string sql2 = sql.Replace("@dbip", db_master.ip)
                .Replace("@dbuser", db_master.userId)
                .Replace("@dbpwd", db_master.password)
                .Replace("@dbfullname", db_master.dbfullname)
                .Replace("@dbStartDate", StartDate)
                .Replace("@dbEndDate", EndDate);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            int maxrows = myDataRows.Rows.Count;
            AccPrefixList prefixlist = new AccPrefixList();
            prefixlist.Rows = new List<AccPrefix>();
            AccPrefix prefix = null;
            for (int i = 0; i < maxrows; i++)
            {
                prefix = new AccPrefix();
                DataRow row = myDataRows.Rows[i];

                prefix.Prefix = row["Prefix"].ToString();
                prefix.Recs = int.Parse(row["Recs"].ToString());
                prefixlist.Rows.Add(prefix);
            }

            string rJason = JsonConvert.SerializeObject(prefixlist.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetAccounts")]
        [HttpPost]
        public IActionResult GetAccounts([FromBody] AccountsInput model)
        {
            var Prefix = model.Prefix;
            var StartDate = model.StartDate + " 00:00:00";
            var EndDate = model.EndDate + " 23:59:59";

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate'; ";
            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( [ID] int null, [BET_TYPE] Nvarchar(200) null, [DrawTypeID] int null, [UpdateDate] datetime null, [Username] Nvarchar(200) null, [Bill_No_Ticket] Nvarchar(200) null ) ";
            sql = sql + "insert into #TempReport select a.ID, c.LotteryTypeName as BET_TYPE, DrawTypeID, a.UpdateDate, a.Username, a.CurrentPeriod as Bill_No_Ticket ";
            sql = sql + "from [dbo].[MPlayer] a ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID ";
            sql = sql + "where a.UpdateDate between @Date11 AND @Date22  and a.IsWin is not NULL ";

            sql = sql + "select ";
            sql = sql + "UserName, ";
            sql = sql + "count(*) as Recs ";
            sql = sql + "from #TempReport ";
            sql = sql + "where left(UserName, CHARINDEX('_', UserName) - 1) = '@dbprefix' ";
            sql = sql + "group by UserName ";
            sql = sql + "order by UserName ";

            string sql2 = sql.Replace("@dbip", db_master.ip)
                .Replace("@dbuser", db_master.userId)
                .Replace("@dbpwd", db_master.password)
                .Replace("@dbfullname", db_master.dbfullname)
                .Replace("@dbStartDate", StartDate)
                .Replace("@dbEndDate", EndDate)
                .Replace("@dbprefix", Prefix);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            int maxrows = myDataRows.Rows.Count;
            AccountsList prefixlist = new AccountsList();
            prefixlist.Rows = new List<Account>();
            Account prefix = null;
            for (int i = 0; i < maxrows; i++)
            {
                prefix = new Account();
                DataRow row = myDataRows.Rows[i];

                prefix.UserName = row["UserName"].ToString();
                prefix.Recs = int.Parse(row["Recs"].ToString());
                prefixlist.Rows.Add(prefix);
            }

            string rJason = JsonConvert.SerializeObject(prefixlist.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetBetTypes")]
        [HttpPost]
        public IActionResult GetBetTypes([FromBody] PrefixInput model)
        {
            var StartDate = model.StartDate + " 00:00:00";
            var EndDate = model.EndDate + " 23:59:59";

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate'; ";
            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( [ID] int null, [BET_TYPE] Nvarchar(200) null, [DrawTypeID] int null, [UpdateDate] datetime null, [Username] Nvarchar(200) null, [Bill_No_Ticket] Nvarchar(200) null ) ";
            sql = sql + "insert into #TempReport select a.ID, c.LotteryTypeName as BET_TYPE, DrawTypeID, a.UpdateDate, a.Username, a.CurrentPeriod as Bill_No_Ticket ";
            sql = sql + "from [dbo].[MPlayer] a ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID ";
            sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";

            sql = sql + "select ";
            sql = sql + "[BET_TYPE] as BetType, ";
            sql = sql + "count(*) as Recs ";
            sql = sql + "from #TempReport ";
            sql = sql + "group by ";
            sql = sql + "[BET_TYPE] ";
            sql = sql + "order by ";
            sql = sql + "[BET_TYPE] ";

            string sql2 = sql.Replace("@dbip", db_master.ip)
                .Replace("@dbuser", db_master.userId)
                .Replace("@dbpwd", db_master.password)
                .Replace("@dbStartDate", StartDate)
                .Replace("@dbEndDate", EndDate)
                .Replace("@dbfullname", db_master.dbfullname);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 300; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            int maxrows = myDataRows.Rows.Count;
            LotteryTypeList prefixlist = new LotteryTypeList();
            prefixlist.Rows = new List<LotteryType>();
            LotteryType prefix = null;
            for (int i = 0; i < maxrows; i++)
            {
                prefix = new LotteryType();
                DataRow row = myDataRows.Rows[i];

                prefix.BetType = row["BetType"].ToString();
                prefix.Recs = int.Parse(row["Recs"].ToString());
                prefixlist.Rows.Add(prefix);
            }

            string rJason = JsonConvert.SerializeObject(prefixlist.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetReportByUserName")]
        [HttpPost]
        public IActionResult GetReportByUserName([FromBody] ReportByUserNameInput model)
        {
            var StartDate = model.StartDate + " 00:00:00";
            var EndDate = model.EndDate + " 23:59:59";
            var UserName = model.UserName;

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate'; ";
            sql = sql + "declare @UserName nvarchar(max); ";
            sql = sql + "set @UserName = '@dbSelectedUserName' ";
            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( ";
            sql = sql + "[ID] int null, ";
            sql = sql + "[BET_TYPE] Nvarchar(200) null, ";
            sql = sql + "[DrawTypeID] int null, ";
            sql = sql + "[UpdateDate] datetime null, ";
            sql = sql + "[Username] Nvarchar(200) null, ";
            sql = sql + "[Bill_No_Ticket] Nvarchar(200) null, ";
            sql = sql + "[TOVER] Numeric (32, 4) null, ";
            sql = sql + "[Openning_Time] datetime null, ";
            sql = sql + "[BetTime] datetime null, ";
            sql = sql + "[Bet_1] Nvarchar(200) null, ";
            sql = sql + "[Bet_2] Nvarchar(200) null, ";
            sql = sql + "[Bet_3] Nvarchar(200) null, ";
            sql = sql + "[BetAmount] Numeric (32, 4) null, ";
            sql = sql + "[WinMoney] Numeric (32, 4) null, ";
            sql = sql + "[IsWin] int null, ";
            sql = sql + "Sum_winlose_AllLOST numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_4d numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_NOT4d numeric (32, 4) null, ";
            sql = sql + "calc_WL numeric (32, 4) null, ";
            sql = sql + "[WL] Numeric (32, 4) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempReport ";
            sql = sql + "select ";
            sql = sql + "a.ID, ";
            sql = sql + "c.LotteryTypeName as BET_TYPE, ";
            sql = sql + "DrawTypeID, ";
            sql = sql + "a.UpdateDate, ";
            sql = sql + "a.Username, ";
            sql = sql + "a.CurrentPeriod as Bill_No_Ticket, ";
            sql = sql + "isnull(Price, 0) as TOVER, ";
            sql = sql + "ShowResultDate as Openning_Time, ";
            sql = sql + "a.CreateDate as BetTime, ";
            sql = sql + "FamliyBigID as Bet_1, ";
            sql = sql + "b.LotteryInfoName as Bet_2, ";
            sql = sql + "SelectedNums as Bet_3, ";
            sql = sql + "isnull(DiscountPrice, 0) as BetAmount, ";
            sql = sql + "isnull(WinMoney, 0), ";
            sql = sql + "a.IsWin, ";
            sql = sql + "( ";
            sql = sql + "case ";
            sql = sql + "when ( a.UpdateDate between @Date11 AND @Date22 and WinMoney = '0.0000' and Iswin is not null ) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllLOST, ";
            sql = sql + "( ";
            sql = sql + "case when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_4d, ";
            sql = sql + "( ";
            sql = sql + "case when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID NOT between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then isnull( TRY_CONVERT(numeric(38,12),WinMoney), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_NOT4d, ";
            sql = sql + "0.00 as calc_WL, ";
            sql = sql + "0.00 as WL ";
            sql = sql + "from [dbo].[MPlayer] a ";
            sql = sql + "inner join[dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID ";
            sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";
            sql = sql + "and a.Username = @UserName ";
            sql = sql + "and a.IsWin is not null ";
            sql = sql + "update #TempReport ";
            sql = sql + "set WL = (Sum_winlose_AllLOST + Sum_winlose_AllWIN_4d + Sum_winlose_AllWIN_NOT4d) ";
            sql = sql + "select ";
            sql = sql + "UserName, ";
            sql = sql + "cast(Updatedate as date) as UpdateDate, ";
            sql = sql + "BET_TYPE, ";
            sql = sql + "Bill_No_Ticket, ";
            sql = sql + "count(*) as Recs, ";
            sql = sql + "sum(TOVER) as TOver, ";
            sql = sql + "sum(BetAmount) as BetAmount, ";
            sql = sql + "sum(WinMoney) as WinMoney, ";
            sql = sql + "sum(Sum_winlose_AllLOST) as Sum_winlose_AllLOST, ";
            sql = sql + "sum(Sum_winlose_AllWIN_4d) as Sum_winlose_AllWIN_4d, ";
            sql = sql + "sum(Sum_winlose_AllWIN_NOT4d) as Sum_winlose_AllWIN_NOT4d, ";
            sql = sql + "sum(WL) as [Win_Lose] ";
            sql = sql + "from #TempReport ";
            sql = sql + "where UserName = @UserName ";
            sql = sql + "group by UserName, cast(Updatedate as date), BET_TYPE , Bill_No_Ticket ";
            sql = sql + "order by cast(Updatedate as date), bet_type, bill_no_ticket ";

            string sql2 = sql.Replace("@dbip", db_master.ip)
                .Replace("@dbuser", db_master.userId)
                .Replace("@dbpwd", db_master.password)
                .Replace("@dbStartDate", StartDate)
                .Replace("@dbEndDate", EndDate)
                .Replace("@dbSelectedUserName", UserName)
                .Replace("@dbfullname", db_master.dbfullname);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 600; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            int maxrows = myDataRows.Rows.Count;
            ReportDataList prefixlist = new ReportDataList();
            prefixlist.Rows = new List<ReportData>();
            ReportData prefix = null;
            for (int i = 0; i < maxrows; i++)
            {
                prefix = new ReportData();
                DataRow row = myDataRows.Rows[i];

                prefix.UserName = row["UserName"].ToString();
                prefix.UpdateDate = row["UpdateDate"].ToString();
                prefix.BET_TYPE = row["BET_TYPE"].ToString();
                prefix.Bill_No_Ticket = row["Bill_No_Ticket"].ToString();
                prefix.Recs = int.Parse(row["Recs"].ToString());
                prefix.TOver = decimal.Parse(row["TOver"].ToString());
                prefix.BetAmount = decimal.Parse(row["BetAmount"].ToString());
                prefix.WinMoney = decimal.Parse(row["WinMoney"].ToString());
                prefix.Sum_winlose_AllLOST = decimal.Parse(row["Sum_winlose_AllLOST"].ToString());
                prefix.Sum_winlose_AllWIN_4d = decimal.Parse(row["Sum_winlose_AllWIN_4d"].ToString());
                prefix.Sum_winlose_AllWIN_NOT4d = decimal.Parse(row["Sum_winlose_AllWIN_NOT4d"].ToString());
                prefix.Win_Lose = decimal.Parse(row["Win_Lose"].ToString());

                prefixlist.Rows.Add(prefix);
            }

            string rJason = JsonConvert.SerializeObject(prefixlist.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("TestExcel")]
        [HttpPost]
        public IActionResult TestExcel([FromBody] InputModel model)
        {
            var newstr = model.InputText;

            List<ReportData> data = new List<ReportData>();
            data = JsonConvert.DeserializeObject<List<ReportData>>(newstr);

            var wwwRootPath = _env.WebRootPath;
            //var contentRootPath = _env.ContentRootPath;

            string AppLocation = "";
            AppLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            AppLocation = AppLocation.Replace("file:\\", "");
            string date = DateTime.Now.ToShortDateString();
            date = date.Replace("/", "_");
            string filename = "WinLoseReportByGame_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xlsx";
            string folder = "ExcelFiles";
            string filepath = wwwRootPath + "\\" + folder + "\\" + filename;

            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Sheet001");

                // user name column
                int i = 1;
                int c = 1;
                string cadd = "A" + i.ToString();
                ws.Cell(i, c).Value = "User Name";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                i = 2;
                foreach (ReportData d in data)
                {
                    cadd = "A" + i.ToString();
                    ws.Cell(i, c).Value = d.UserName;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");
                    //.Fill.BackgroundColor = XLColor.FromArgb(0xFF00FF);
                    i++;
                }

                // ---------------- UpdateDate column
                i = 1;
                c = 2;
                ws.Cell(i, c).Value = "Update Date";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                i = 2;
                foreach (ReportData d in data)
                {
                    ws.Cell(i, c).Value = DateTime.Parse(d.UpdateDate.ToString()).ToString("yyyy-MMM-dd");

                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");
                    i++;
                }

                // ---------------- Bet Type column
                i = 1;
                c = 3;
                ws.Cell(i, c).Value = "Bet Type";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                i = 2;
                foreach (ReportData d in data)
                {
                    ws.Cell(i, c).Value = d.BET_TYPE;

                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");
                    i++;
                }

                // ---------------- Bet Type column
                i = 1;
                c = 4;
                ws.Cell(i, c).Value = "Ticket No";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                i = 2;
                foreach (ReportData d in data)
                {
                    ws.Cell(i, c).Value = d.Bill_No_Ticket;

                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");
                    i++;
                }

                // ---------------- Recs column
                i = 1;
                c = 5;
                ws.Cell(i, c).Value = "Detail Row";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                int TotalRecs = 0;
                i = 2;
                foreach (ReportData d in data)
                {
                    ws.Cell(i, c).Value = d.Recs;

                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");
                    TotalRecs = TotalRecs + d.Recs;
                    i++;
                }
                ws.Cell(i, c).Value = TotalRecs;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                // ---------------- Turn Over column
                i = 1;
                c = 6;
                ws.Cell(i, c).Value = "Turn Over";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                decimal TotalTover = 0;
                var format = "#,##0.0000; (#,##0.0000)";
                i = 2;
                foreach (ReportData d in data)
                {
                    ws.Cell(i, c).Value = d.TOver;
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    //ws.Cell(i, c).Style.Alignment.SetHorizontal(XLDrawingHorizontalAlignment.Right);

                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalTover = TotalTover + d.TOver;
                    i++;
                }
                ws.Cell(i, c).Value = TotalTover;
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                // ---------------- Bet Amount column
                i = 1;
                c = 7;
                ws.Cell(i, c).Value = "Bet Amount";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                decimal TotalBetAmount = 0;
                //var format = "#,##0.0000; (#,##0.0000)";
                i = 2;
                foreach (ReportData d in data)
                {
                    ws.Cell(i, c).Value = d.BetAmount;
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    //ws.Cell(i, c).Style.Alignment.SetHorizontal(XLDrawingHorizontalAlignment.Right);

                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalBetAmount = TotalBetAmount + d.BetAmount;
                    i++;
                }
                ws.Cell(i, c).Value = TotalBetAmount;
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                // ---------------- Win Money column
                i = 1;
                c = 8;
                ws.Cell(i, c).Value = "Win Money";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                decimal TotalWinMoney = 0;
                //var format = "#,##0.0000; (#,##0.0000)";
                i = 2;
                foreach (ReportData d in data)
                {
                    ws.Cell(i, c).Value = d.WinMoney;
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    //ws.Cell(i, c).Style.Alignment.SetHorizontal(XLDrawingHorizontalAlignment.Right);

                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalWinMoney = TotalWinMoney + d.WinMoney;
                    i++;
                }
                ws.Cell(i, c).Value = TotalWinMoney;
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                // ---------------- TT WL All Lost column
                i = 1;
                c = 9;
                ws.Cell(i, c).Value = "TT WL All Lost";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                decimal TotalWinLoseAllLost = 0;
                //var format = "#,##0.0000; (#,##0.0000)";
                i = 2;
                foreach (ReportData d in data)
                {
                    ws.Cell(i, c).Value = d.Sum_winlose_AllLOST;
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    //ws.Cell(i, c).Style.Alignment.SetHorizontal(XLDrawingHorizontalAlignment.Right);

                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalWinLoseAllLost = TotalWinLoseAllLost + d.Sum_winlose_AllLOST;
                    i++;
                }
                ws.Cell(i, c).Value = TotalWinLoseAllLost;
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                // ---------------- TT WL All 4D Wins column
                i = 1;
                c = 10;
                ws.Cell(i, c).Value = "TT WL All 4D Wins";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                decimal TTWLAll4DWins = 0;
                //var format = "#,##0.0000; (#,##0.0000)";
                i = 2;
                foreach (ReportData d in data)
                {
                    ws.Cell(i, c).Value = d.Sum_winlose_AllWIN_4d;
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    //ws.Cell(i, c).Style.Alignment.SetHorizontal(XLDrawingHorizontalAlignment.Right);

                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TTWLAll4DWins = TTWLAll4DWins + d.Sum_winlose_AllWIN_4d;
                    i++;
                }
                ws.Cell(i, c).Value = TTWLAll4DWins;
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                // ---------------- "TT WL All Non 4D Wins column
                i = 1;
                c = 11;
                ws.Cell(i, c).Value = "TT WL All Non 4D Wins";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                decimal TTWLAllNon4DWins = 0;
                //var format = "#,##0.0000; (#,##0.0000)";
                i = 2;
                foreach (ReportData d in data)
                {
                    ws.Cell(i, c).Value = d.Sum_winlose_AllWIN_NOT4d;
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    //ws.Cell(i, c).Style.Alignment.SetHorizontal(XLDrawingHorizontalAlignment.Right);

                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TTWLAllNon4DWins = TTWLAllNon4DWins + d.Sum_winlose_AllWIN_NOT4d;
                    i++;
                }
                ws.Cell(i, c).Value = TTWLAllNon4DWins;
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                // ---------------- "TT WL All Non 4D Wins column
                i = 1;
                c = 12;
                ws.Cell(i, c).Value = "Total Win/Lose";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                decimal TotalWinLoss = 0;
                //var format = "#,##0.0000; (#,##0.0000)";
                i = 2;
                foreach (ReportData d in data)
                {
                    ws.Cell(i, c).Value = d.Win_Lose;
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    //ws.Cell(i, c).Style.Alignment.SetHorizontal(XLDrawingHorizontalAlignment.Right);

                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalWinLoss = TotalWinLoss + d.Win_Lose;
                    i++;
                }
                ws.Cell(i, c).Value = TotalWinLoss;
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial")
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                //----------------------------------------- end of columns population ------------------------

                ws.Columns().AdjustToContents();

                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                wb.SaveAs(filepath);
            }


            URLResponseList uRLResponseList = new URLResponseList();

            URLResponse res = new URLResponse();
            res.FileName = filename;
            res.FolderName = folder;

            uRLResponseList.Rows = new List<URLResponse>();
            uRLResponseList.Rows.Add(res);

            string rJason = JsonConvert.SerializeObject(uRLResponseList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetBetDetails")]
        [HttpPost]
        public IActionResult GetBetDetails([FromBody] BetDetailInput model)
        {
            string username = model.UserName;
            string currentperiod = model.CurrentPeriod;

            string sql = "";
            sql = sql + "select ";
            sql = sql + "a.CurrentPeriod, ";
            sql = sql + "a.CreateDate, ";
            sql = sql + "a.UserName, ";
            sql = sql + "b.AgentParentMap, ";
            sql = sql + "c.[FamliyBigID], ";
            sql = sql + "a.[SelectedNums], ";
            sql = sql + "a.Price, ";
            sql = sql + "a.DiscountPrice, ";
            sql = sql + "a.WinMoney, ";
            sql = sql + "a.IsWin, ";
            sql = sql + "case ";
            sql = sql + "when IsWin = 1 then cast(a.Price as decimal) * 1 ";
            sql = sql + "else cast(a.Price as decimal) * -1 ";
            sql = sql + "end ";
            sql = sql + "as WinLose ";
            sql = sql + "from MPlayer a ";
            sql = sql + "inner join GameDealerMemberShip b on a.GameDealerMemberID = b.MemberID ";
            sql = sql + "inner join LotteryInfo c on a.LotteryInfoID = c.LotteryInfoID ";
            sql = sql + "where a.CurrentPeriod = '@dbticket' ";
            sql = sql + "and a.UserName = '@dbaccount' ";
            sql = sql + "order by a.UpdateDate ";

            string sql2 = sql.Replace("@dbip", db_master.ip)
                .Replace("@dbuser", db_master.userId)
                .Replace("@dbpwd", db_master.password)
                .Replace("@dbticket", currentperiod)
                .Replace("@dbaccount", username)
                .Replace("@dbfullname", db_master.dbfullname);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 600; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            BetDetailList betDetailList = new BetDetailList();
            betDetailList.Rows = new List<BetDetail>();

            int maxrows = myDataRows.Rows.Count;

            for (int i = 0; i < maxrows; i++)
            {
                BetDetail res = new BetDetail();
                DataRow row = myDataRows.Rows[i];

                res.CurrentPeriod = row["CurrentPeriod"].ToString();
                res.CreateDate = DateTime.Parse(row["CreateDate"].ToString());
                res.UserName = row["UserName"].ToString();
                res.AgentParentMap = row["AgentParentMap"].ToString();
                res.FamilyBigID = row["FamliyBigID"].ToString();
                res.SelectedNums = row["SelectedNums"].ToString();
                res.Price = decimal.Parse(row["Price"].ToString());
                res.DiscountPrice = decimal.Parse(row["DiscountPrice"].ToString());
                res.WinMoney = decimal.Parse(row["WinMoney"].ToString());
                res.IsWin = bool.Parse(row["IsWin"].ToString());
                res.WinLose = decimal.Parse(row["WinLose"].ToString());

                betDetailList.Rows.Add(res);
            }

            string rJason = JsonConvert.SerializeObject(betDetailList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GenExcelBetDetails")]
        [HttpPost]
        public IActionResult GenExcelBetDetails([FromBody] InputModel model)
        {
            var newstr = model.InputText;

            List<BetDetail> data = new List<BetDetail>();
            data = JsonConvert.DeserializeObject<List<BetDetail>>(newstr);

            var wwwRootPath = _env.WebRootPath;
            //var contentRootPath = _env.ContentRootPath;

            string AppLocation = "";
            AppLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            AppLocation = AppLocation.Replace("file:\\", "");
            string date = DateTime.Now.ToShortDateString();
            date = date.Replace("/", "_");
            string filename = "WinLoseReportBetDetails_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xlsx";
            string folder = "ExcelFiles";
            string filepath = wwwRootPath + "\\" + folder + "\\" + filename;

            decimal TotalPrice = 0;
            decimal TotalDiscountPrice = 0;
            decimal TotalWinMoney = 0;
            decimal TotalWinLose = 0;

            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Sheet001");

                // user name column
                int i = 1; // row number 1 is the Report Title
                int c = 1;


                int datalen = data.Count;

                var format = "#,##0.0000; (#,##0.0000)";

                i = 3; // row 3 will start the column title
                c = 1; // first column is ticket no
                ws.Cell(i, c).Value = "Ticket No";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    BetDetail d = data[j];
                    ws.Cell(i, c).Value = d.CurrentPeriod;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    i++;
                }

                // column 2
                i = 3;
                c = 2;

                ws.Cell(i, c).Value = "Create Date";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    BetDetail d = data[j];
                    ws.Cell(i, c).Value = DateTime.Parse(d.CreateDate.ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");




                    i++;
                }

                // column 3: User Name
                i = 3;
                c = 3;

                ws.Cell(i, c).Value = "User Name";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    BetDetail d = data[j];
                    ws.Cell(i, c).Value = d.UserName;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    i++;
                }

                // column 4: AgentParentMap
                i = 3;
                c = 4;

                ws.Cell(i, c).Value = "Agent Parent Map";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    BetDetail d = data[j];
                    ws.Cell(i, c).Value = d.AgentParentMap;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    i++;
                }

                // column 5: Family Big ID + Selected Nums
                i = 3;
                c = 5;

                ws.Cell(i, c).Value = "Bet";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    BetDetail d = data[j];
                    ws.Cell(i, c).Value = d.FamilyBigID.ToString() + " " + d.SelectedNums;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    i++;
                }

                // column 6: Price
                i = 3;
                c = 6;

                ws.Cell(i, c).Value = "Price";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    BetDetail d = data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.Price.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    if (j >= 2)
                    {
                        string x = "123";
                    }

                    TotalPrice = TotalPrice + decimal.Parse(d.Price.ToString());

                    i++;
                }


                ws.Cell(i, c).Value = TotalPrice;
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // column 7: DiscountPrice
                i = 3;
                c = 7;

                ws.Cell(i, c).Value = "Discount Price";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    BetDetail d = data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.DiscountPrice.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    TotalDiscountPrice = TotalDiscountPrice + decimal.Parse(d.DiscountPrice.ToString());

                    i++;
                }

                ws.Cell(i, c).Value = TotalDiscountPrice;
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // column 8: winmoney
                i = 3;
                c = 8;

                ws.Cell(i, c).Value = "Win Money";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    BetDetail d = data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.WinMoney.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    TotalWinMoney = TotalWinMoney + decimal.Parse(d.WinMoney.ToString());

                    i++;
                }

                ws.Cell(i, c).Value = TotalWinMoney;
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // column 9: Is Win
                i = 3;
                c = 9;

                ws.Cell(i, c).Value = "Is Win";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    BetDetail d = data[j];
                    ws.Cell(i, c).Value = d.IsWin.ToString();
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    i++;
                }

                // column 10: Win Lose from User Perspective
                i = 3;
                c = 10;

                ws.Cell(i, c).Value = "Win/Lose";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    BetDetail d = data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.WinLose.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    TotalWinLose = TotalWinLose + decimal.Parse(d.WinLose.ToString());

                    i++;
                }

                ws.Cell(i, c).Value = TotalWinLose;
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                //----------------------------------------- end of columns population ------------------------

                ws.Columns().AdjustToContents();

                i = 1; // row number 1 is the Report Title
                c = 1;
                ws.Cell(i, c).Value = "Win Lose Report by Game for Bet Details";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial");
                //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                wb.SaveAs(filepath);
            }


            URLResponseList uRLResponseList = new URLResponseList();

            URLResponse res = new URLResponse();
            res.FileName = filename;
            res.FolderName = folder;

            uRLResponseList.Rows = new List<URLResponse>();
            uRLResponseList.Rows.Add(res);

            string rJason = JsonConvert.SerializeObject(uRLResponseList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetCommaNumber")]
        [HttpPost]
        public IActionResult GetCommaNumber([FromBody] InputModel model)
        {
            bool fool = false;
            decimal numbervalue = 0;
            fool = decimal.TryParse(model.InputText, out numbervalue);

            List<InputModel> list = new List<InputModel>();
            
            InputModel rt = new InputModel();
            rt.InputText = numbervalue.ToString("#,##0.0000");

            list.Add(rt);

            string rJason = JsonConvert.SerializeObject(list);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetOverallReport")]
        [HttpPost]
        public IActionResult GetOverallReport([FromBody] DateRange model)
        {
            var StartDate = model.StartDate;
            var EndDate = model.EndDate;
            var CurrentPeriod = model.CurrentPeriod;
            var UserName = model.UserName;
            var BetType = model.BetType;
            var StartTime = model.StartTime;
            var EndTime = model.EndTime;

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate @dbStartTime'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate @dbEndTime'; ";
            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( ";
            sql = sql + "[ID] int null, ";
            sql = sql + "[BET_TYPE] Nvarchar(200) null, ";
            sql = sql + "[LotteryTypeID] int null, ";
            sql = sql + "[DrawTypeID] int null, ";
            sql = sql + "[UpdateDate] datetime null, ";
            sql = sql + "[Username] Nvarchar(200) null, ";
            sql = sql + "[Bill_No_Ticket] Nvarchar(200) null, ";
            sql = sql + "[TOVER] Numeric (32, 4) null, ";
            sql = sql + "[Pending] numeric(32, 4) null, ";
            sql = sql + "[Openning_Time] datetime null, ";
            sql = sql + "[BetTime] datetime null, ";
            sql = sql + "[Bet_1] Nvarchar(200) null, ";
            sql = sql + "[Bet_2] Nvarchar(200) null, ";
            sql = sql + "[Bet_3] Nvarchar(200) null, ";
            sql = sql + "[BetAmount] Numeric (32, 4) null, ";
            sql = sql + "[WinMoney] Numeric (32, 4) null, ";
            sql = sql + "[IsWin] int null, ";
            sql = sql + "Sum_winlose_AllLOST numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_4d numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_NOT4d numeric (32, 4) null, ";
            sql = sql + "calc_WL numeric (32, 4) null, ";
            sql = sql + "[WL] Numeric (32, 4) null, ";
            sql = sql + "AgentWinLose numeric (32, 4) null, ";
            sql = sql + "ComWinLose numeric (32, 4) null, ";
            sql = sql + "MAWinLose numeric (32, 4) null, ";
            sql = sql + "SMWinLose numeric (32, 4) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempReport ";
            sql = sql + "select ";
            sql = sql + "a.ID, ";
            sql = sql + "c.LotteryTypeName as BET_TYPE, ";
            sql = sql + "b.LotteryTypeID, ";
            sql = sql + "DrawTypeID, ";
            sql = sql + "a.UpdateDate, ";
            sql = sql + "a.Username, ";
            sql = sql + "a.CurrentPeriod as Bill_No_Ticket, ";
            sql = sql + "isnull(Price, 0) as TOVER, ";
            sql = sql + "case ";
            sql = sql + "when (iswin is null) then TRY_CONVERT(numeric(38,12),Price) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + "as Pending, ";
            sql = sql + "ShowResultDate as Openning_Time, ";
            sql = sql + "a.CreateDate as BetTime, ";
            sql = sql + "FamliyBigID as Bet_1, ";
            sql = sql + "b.LotteryInfoName as Bet_2, ";
            sql = sql + "SelectedNums as Bet_3, ";
            sql = sql + "isnull(DiscountPrice, 0) as BetAmount, ";
            sql = sql + "isnull(WinMoney, 0), ";
            sql = sql + "a.IsWin, ";
            sql = sql + "( ";
            sql = sql + "case ";
            sql = sql + "when ( a.UpdateDate between @Date11 AND @Date22 and WinMoney = '0.0000' and Iswin is not null ) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllLOST, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_4d, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID NOT between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then isnull( TRY_CONVERT(numeric(38,12),WinMoney), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_NOT4d, ";
            sql = sql + "0.00 as calc_WL, ";
            sql = sql + "0.00 as WL, ";
            sql = sql + "0.00 as AgentWinLose, ";
            sql = sql + "0.00 as comWinLose, ";
            sql = sql + "0.00 as MAWinLose, ";
            sql = sql + "0.00 as SMWinLose ";
            sql = sql + "from [dbo].[MPlayer] a ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID ";

            if (CurrentPeriod == "")
            {
                sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";
            }
            else
            {
                sql = sql + "where a.CurrentPeriod = '"+ CurrentPeriod + "' ";
            }

            if (UserName != "")
            {
                sql = sql + "and a.UserName like '%" + UserName + "%' ";
            }

            if (BetType != "")
            {
                sql = sql + "and c.LotteryTypeName = '" + BetType + "' ";
            }

            sql = sql + "update #TempReport ";
            sql = sql + "set WL = (Sum_winlose_AllLOST + Sum_winlose_AllWIN_4d + Sum_winlose_AllWIN_NOT4d) ";
            sql = sql + "update #TempReport ";
            sql = sql + "set AgentWinLose = (WL * -1) * 0.900000000216845 ";
            sql = sql + ", ComWinLose = (WL * -1) * 0.1000 ";
            sql = sql + "select Bet_Type ";
            sql = sql + ", LotteryTypeID ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(Pending) as Pending ";
            sql = sql + ", sum(WL) as MemberWinLose ";
            sql = sql + ", sum(AgentWinLose) as AgentWinLose ";
            sql = sql + ", sum(ComWinLose) as ComWinLose ";
            sql = sql + ", sum(MAWinLose) as MAWinLose ";
            sql = sql + ", sum(SMWinLose) as SMWinLose ";
            sql = sql + "from #TempReport ";
            sql = sql + "group by ";
            sql = sql + "Bet_Type ";
            sql = sql + ", LotteryTypeID ";
            sql = sql + "order by ";
            sql = sql + "LotteryTypeID ";

            string sql2 = sql.Replace("@dbStartDate", StartDate)
                .Replace("@dbEndDate", EndDate)
                .Replace("@dbStartTime", StartTime)
                .Replace("@dbEndTime", EndTime);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 600; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            OverDataList overDataList = new OverDataList();
            overDataList.Rows = new List<OverallData>();

            int maxrows = myDataRows.Rows.Count;

            for (int i = 0; i < maxrows; i++)
            {
                DataRow row = myDataRows.Rows[i];

                OverallData overallData = new OverallData();
                overallData.BetType = row["Bet_Type"].ToString();
                overallData.LotteryTypeID =  row["LotteryTypeID"].ToString();
                overallData.TOver = row["TOver"].ToString();
                overallData.Pending = row["Pending"].ToString();
                overallData.MemberWinLose = row["MemberWinLose"].ToString();
                overallData.AgentWinLose =row["AgentWinLose"].ToString();
                overallData.ComWinLose =row["ComWinLose"].ToString();
                overallData.MAWinLose = row["MAWinLose"].ToString();
                overallData.SMWinLose =row["SMWinLose"].ToString();

                overDataList.Rows.Add(overallData);
            }

            string rJason = JsonConvert.SerializeObject(overDataList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetOverallByBetType")]
        [HttpPost]
        public IActionResult GetOverallByBetType([FromBody] OverallByBetTypeInput model)
        {
            string StartDate = model.StartDate;
            string EndDate = model.EndDate;
            string BetType = model.BetType;
            string CurrentPeriod = model.CurrentPeriod;
            string UserName = model.UserName;
            string StartTime = model.StartTime;
            string EndTime = model.EndTime;

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate';  ";
            sql = sql + "drop table if exists #TempReport  ";
            sql = sql + "create table #TempReport (  ";
            sql = sql + "	[ID] int null ";
            sql = sql + "	, [BET_TYPE] Nvarchar(200) null ";
            sql = sql + "	, [LotteryTypeID] int null ";
            sql = sql + "	, [DrawTypeID] int null ";
            sql = sql + "	, [UpdateDate] datetime null ";
            sql = sql + "	, [Username] Nvarchar(200) null ";
            sql = sql + "	, [Bill_No_Ticket] Nvarchar(200) null ";
            sql = sql + "	, [TOVER] Numeric (32, 4) null ";
            sql = sql + "	, [Pending] numeric(32, 4) null ";
            sql = sql + "	, [Openning_Time] datetime null ";
            sql = sql + "	, [BetTime] datetime null ";
            sql = sql + "	, [Bet_1] Nvarchar(200) null ";
            sql = sql + "	, [Bet_2] Nvarchar(200) null ";
            sql = sql + "	, [Bet_3] Nvarchar(200) null ";
            sql = sql + "	, [BetAmount] Numeric (32, 4) null ";
            sql = sql + "	, [WinMoney] Numeric (32, 4) null ";
            sql = sql + "	, [IsWin] int null ";
            sql = sql + "	, Sum_winlose_AllLOST numeric (32, 4) null ";
            sql = sql + "	, Sum_winlose_AllWIN_4d numeric (32, 4) null ";
            sql = sql + "	, Sum_winlose_AllWIN_NOT4d numeric (32, 4) null ";
            sql = sql + "	, [WL] Numeric (32, 4) null ";
            sql = sql + "	, [Lost] numeric (32, 4) null ";
            sql = sql + "	, [Win] numeric (32, 4) null  ";
            sql = sql + "	, [Win_WO_Capital] numeric (32, 4) null  ";
            sql = sql + "	, [LotteryTypeName] nvarchar(200) null ";
            sql = sql + ")  ";
            sql = sql + "insert into #TempReport  ";
            sql = sql + "select  ";
            sql = sql + "	a.ID ";
            sql = sql + "	, c.LotteryTypeName as BET_TYPE ";
            sql = sql + "	, b.LotteryTypeID ";
            sql = sql + "	, DrawTypeID ";
            sql = sql + "	, a.UpdateDate ";
            sql = sql + "	, a.Username ";
            sql = sql + "	, a.CurrentPeriod as Bill_No_Ticket ";
            sql = sql + "	, isnull(Price, 0) as TOVER ";
            sql = sql + "	, ( case  ";
            sql = sql + "			when (iswin is null) then TRY_CONVERT(numeric(38,12),Price)  ";
            sql = sql + "			else 0  ";
            sql = sql + "		end ) as Pending ";
            sql = sql + "	, ShowResultDate as Openning_Time ";
            sql = sql + "	, a.CreateDate as BetTime ";
            sql = sql + "	, FamliyBigID as Bet_1 ";
            sql = sql + "	, b.LotteryInfoName as Bet_2 ";
            sql = sql + "	, SelectedNums as Bet_3 ";
            sql = sql + "	, isnull(DiscountPrice, 0) as BetAmount ";
            sql = sql + "	, isnull(WinMoney, 0) ";
            sql = sql + "	, IsWin ";
            sql = sql + "	, ( case  ";
            sql = sql + "			when ( a.UpdateDate between @Date11 AND @Date22 and WinMoney = '0.0000' and Iswin is not null ) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0)  ";
            sql = sql + "			else 0  ";
            sql = sql + "		end ) as Sum_winlose_AllLOST ";
            sql = sql + "	, ( case	 ";
            sql = sql + "			when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0)  ";
            sql = sql + "			else 0  ";
            sql = sql + "		end ) as Sum_winlose_AllWIN_4d ";
            sql = sql + "	, ( case	 ";
            sql = sql + "			when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID NOT between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then isnull( TRY_CONVERT(numeric(38,12),WinMoney), 0)  ";
            sql = sql + "			else 0  ";
            sql = sql + "		end ) as Sum_winlose_AllWIN_NOT4d ";
            sql = sql + "	, 0.00 as WL ";
            sql = sql + "	, 0.00 as Lost ";
            sql = sql + "	, 0.00 as Win  ";
            sql = sql + "	, 0.00 as Win_WO_Capital ";
            sql = sql + "	, c.LotteryTypeName ";
            sql = sql + "from [dbo].[MPlayer] a  ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID  ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID  ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID  ";


            if (CurrentPeriod == "")
            {
                sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";
            }
            else
            {
                sql = sql + "where a.CurrentPeriod = '" + CurrentPeriod + "' ";
            }

            if (UserName != "")
            {
                sql = sql + "and a.UserName like '%" + UserName + "%' ";
            }

            if (BetType != "")
            {
                sql = sql + "and c.LotteryTypeName = '@dbBetType'  ";
            }


            sql = sql + "update #TempReport  ";
            sql = sql + "set  ";
            sql = sql + "	WL = (Sum_winlose_AllLOST + Sum_winlose_AllWIN_4d + Sum_winlose_AllWIN_NOT4d)  ";
            sql = sql + "	, Lost = case	  ";
            sql = sql + "				when (DrawTypeID between 142 and 152 and WinMoney <> 0 and Iswin is not null) then ISNULL(WinMoney, 0)- isnull(BetAmount, 0) else 0  ";
            sql = sql + "				end  ";
            sql = sql + "	, Win = case  ";
            sql = sql + "				when (DrawTypeID between 142 and 152 and WinMoney <> 0 and Iswin is not null) then ISNULL(WinMoney, 0)-isnull(BetAmount, 0)  ";
            sql = sql + "				when (DrawTypeID NOT between 142 and 152 and WinMoney <> 0 and Iswin is not null) then isnull( WinMoney, 0) else 0  ";
            sql = sql + "				end  ";
            sql = sql + "	, Sum_winlose_AllLOST = Sum_winlose_AllLOST * -1  ";
            sql = sql + "	, Win_WO_Capital = WinMoney - BetAmount ";
            sql = sql + "drop table if exists #TempSummary  ";
            sql = sql + "Create table #TempSummary (  ";
            sql = sql + "	Bet_Type nvarchar(max) null ";
            sql = sql + "	, DrawTypeID int null ";
            sql = sql + "	, UserName nvarchar(max) null ";
            sql = sql + "	, Bill_No_Ticket nvarchar(max) null ";
            sql = sql + "	, Openning_Time datetime null ";
            sql = sql + "	, [UpdateDate] datetime null ";
            sql = sql + "	, IsWin int null ";
            sql = sql + "	, TOver numeric (32, 12) null ";
            sql = sql + "	, Pending numeric (32, 12) null ";
            sql = sql + "	, BetAmount numeric (32, 12) null ";
            sql = sql + "	, WL numeric (32, 12) null ";
            sql = sql + "	, Sum_AllLost numeric (32, 12) null ";
            sql = sql + "	, Sum_AllWin_4d numeric (32, 12) null ";
            sql = sql + "	, Sum_AllWin_Not4D numeric (32, 12) null ";
            sql = sql + "	, TotalWin numeric (32, 12) null ";
            sql = sql + "	, TotalLost numeric (32, 12) null ";
            sql = sql + "	, Margin numeric (32, 12) null  ";
            sql = sql + "	, Total_Win_WO_Capital numeric (32, 12) null  ";
            sql = sql + "	, Capital numeric (32, 12) null  ";
            sql = sql + "	, LotteryTypeName nvarchar(max) null ";
            sql = sql + ")  ";
            sql = sql + "insert into #TempSummary  ";
            sql = sql + "( ";
            sql = sql + "	LotterytypeName ";
            sql = sql + "	, Bet_Type  ";
            sql = sql + "	, DrawTypeID  ";
            sql = sql + "	, UserName  ";
            sql = sql + "	, Bill_No_Ticket  ";
            sql = sql + "	, Openning_Time ";
            sql = sql + "	, [UpdateDate]  ";
            sql = sql + "	, IsWin  ";
            sql = sql + "	, TOver  ";
            sql = sql + "	, Pending  ";
            sql = sql + "	, BetAmount  ";
            sql = sql + "	, WL  ";
            sql = sql + "	, Sum_AllLOST  ";
            sql = sql + "	, Sum_AllWIN_4d  ";
            sql = sql + "	, Sum_AllWIN_NOT4d  ";
            sql = sql + "	, TotalWin ";
            sql = sql + "	, TotalLost ";
            sql = sql + "	, Margin ";
            sql = sql + "	, Total_Win_WO_Capital ";
            sql = sql + "	, Capital ";
            sql = sql + ") ";
            sql = sql + "select  ";
            sql = sql + "LotterytypeName ";
            sql = sql + ", Bet_Type  ";
            sql = sql + ", DrawTypeID  ";
            sql = sql + ", UserName  ";
            sql = sql + ", Bill_No_Ticket  ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", [UpdateDate]  ";
            sql = sql + ", IsWin  ";
            sql = sql + ", sum(TOver) as TOver  ";
            sql = sql + ", sum(Pending) as Pending ";
            sql = sql + ", sum(BetAmount) as BetAmount  ";
            sql = sql + ", sum(WL) as MemberWinLose  ";
            sql = sql + ", sum(Sum_winlose_AllLOST) as Sum_AllLOST  ";
            sql = sql + ", sum(Sum_winlose_AllWIN_4d) as Sum_AllWIN_4d  ";
            sql = sql + ", sum(Sum_winlose_AllWIN_NOT4d) as Sum_AllWIN_NOT4d  ";
            sql = sql + ", TotalWin = case  ";
            sql = sql + "				when (#TempReport.isWin = 1) then sum(Sum_winlose_AllWIN_4d) + sum(Sum_winlose_AllWIN_NOT4d) + sum(BetAmount)  ";
            sql = sql + "				else 0  ";
            sql = sql + "			end  ";
            sql = sql + ", TotalLost = sum(Sum_winlose_AllLOST)   ";
            sql = sql + ", Margin =  sum(BetAmount) - sum(Sum_winlose_AllLOST)    ";
            sql = sql + ", sum(win_wo_capital) ";
            sql = sql + ", sum(BetAmount) ";
            sql = sql + "from #TempReport  ";
            sql = sql + "group by  ";
            sql = sql + "LotteryTypeName ";
            sql = sql + ", Bet_Type  ";
            sql = sql + ", DrawTypeID  ";
            sql = sql + ", UserName  ";
            sql = sql + ", Bill_No_Ticket  ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", [UpdateDate]  ";
            sql = sql + ", IsWin  ";
            sql = sql + "order by  ";
            sql = sql + "UserName ";
            sql = sql + ", Bill_No_Ticket  ";
            sql = sql + "drop table if exists #TempSummary2  ";
            sql = sql + "create table #TempSummary2 (  ";
            sql = sql + "	LotteryTypeName nvarchar(max) null ";
            sql = sql + "	, Bet_Type nvarchar(max) null ";
            sql = sql + "	, DrawTypeID int null ";
            sql = sql + "	, UserName nvarchar(max) null ";
            sql = sql + "	, Bill_No_Ticket nvarchar(max) null ";
            sql = sql + "	, Openning_Time datetime null ";
            sql = sql + "	, [UpdateDate] datetime null ";
            sql = sql + "	, TOver numeric (32, 12) null ";
            sql = sql + "	, Pending numeric (32, 12) null ";
            sql = sql + "	, BetAmount numeric (32, 12) null ";
            sql = sql + "	, WL numeric (32, 12) null ";
            sql = sql + "	, Sum_AllLost numeric (32, 12) null ";
            sql = sql + "	, Sum_AllWin_4d numeric (32, 12) null ";
            sql = sql + "	, Sum_AllWin_Not4D numeric (32, 12) null ";
            sql = sql + "	, TotalWin numeric (32, 12) null ";
            sql = sql + "	, TotalLost numeric (32, 12) null ";
            sql = sql + "	, Margin numeric (32, 12) null  ";
            sql = sql + "	, Total_Win_WO_Capital numeric (32, 12) null  ";
            sql = sql + "	, Total_Capital numeric (32, 12) null  ";
            sql = sql + ")  ";
            sql = sql + "insert into #TempSummary2  ";
            sql = sql + "select  ";
            sql = sql + "LotteryTypeName ";
            sql = sql + ", Bet_Type  ";
            sql = sql + ", DrawTypeID  ";
            sql = sql + ", UserName  ";
            sql = sql + ", Bill_No_Ticket  ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", [UpdateDate]  ";
            sql = sql + ", sum(TOver) as TOver  ";
            sql = sql + ", sum(Pending) as Pending  ";
            sql = sql + ", sum(BetAmount) as BetAmount  ";
            sql = sql + ", sum(WL) as MemberWinLose  ";
            sql = sql + ", sum(Sum_AllLOST) as Sum_AllLOST  ";
            sql = sql + ", sum(Sum_AllWIN_4d) as Sum_AllWIN_4d  ";
            sql = sql + ", sum(Sum_AllWIN_NOT4d) as Sum_AllWIN_NOT4d  ";
            sql = sql + ", case  ";
            sql = sql + "	when (DrawTypeID NOT between 142 and 152) then sum(TotalWin) - sum(Margin)  ";
            sql = sql + "	else sum(TotalWin)  ";
            sql = sql + "  end as TotalWin  ";
            sql = sql + ", sum(TotalLost) as TotalLost  ";
            sql = sql + ", sum(Margin) as TotalMargin  ";
            sql = sql + ", sum(Total_Win_WO_Capital) ";
            sql = sql + ", sum(Capital) ";
            sql = sql + "from #TempSummary  ";
            sql = sql + "group by  ";
            sql = sql + "LotteryTypeName ";
            sql = sql + ", Bet_Type  ";
            sql = sql + ", DrawTypeID  ";
            sql = sql + ", UserName  ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", [UpdateDate]  ";
            sql = sql + ", Openning_Time  ";
            sql = sql + "order by  ";
            sql = sql + "Bill_No_Ticket desc  ";

            sql = sql + "select  ";
            sql = sql + "LotteryTypeName ";
            sql = sql + ", UserName  ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", [UpdateDate] ";
            sql = sql + ", sum(TOver) as TOver  ";
            sql = sql + ", sum(TotalWin) as TotalWin  ";
            sql = sql + ", sum(TotalLost) as TotalLost  ";
            sql = sql + ", sum(Pending) as TotalPending  ";
            sql = sql + ", Win_WO_Capital = sum(Total_Win_WO_Capital) ";
            sql = sql + ", sum(Total_Capital) as Capital ";
            sql = sql + ", sum(WL) as WL ";
            sql = sql + "from #TempSummary2  ";
            sql = sql + "group by  ";
            sql = sql + "LotteryTypeName ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket  ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", [UpdateDate] ";
            sql = sql + "order by  ";
            sql = sql + "UserName ";
            sql = sql + ", Bill_No_Ticket  ";
            sql = sql + ", Openning_Time  ";

            string sql2 = sql.Replace("@dbStartDate", StartDate)
                .Replace("@dbEndDate", EndDate)
                .Replace("@dbBetType", BetType)
                .Replace("@dbStartTime", StartTime)
                .Replace("@dbEndTime", EndTime);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 600; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            OverallByAccountList overbyaccList = new OverallByAccountList();
            overbyaccList.Rows = new List<OverallByAccount>();

            int maxrows = myDataRows.Rows.Count;

            for (int i = 0; i < maxrows; i++)
            {
                DataRow row = myDataRows.Rows[i];

                OverallByAccount dat = new OverallByAccount();
                dat.UserName = row["UserName"].ToString();
                dat.BillNo = row["Bill_No_Ticket"].ToString();
                dat.UpdateDate = DateTime.Parse( row["UpdateDate"].ToString() );
                dat.ShowResultDate = DateTime.Parse( row["Openning_Time"].ToString() );
                dat.TurnOver = decimal.Parse( row["TOver"].ToString() );
                dat.TotalWin = decimal.Parse( row["TotalWin"].ToString() );
                dat.TotalLost = decimal.Parse(row["TotalLost"].ToString());
                dat.TotalPending = decimal.Parse(row["TotalPending"].ToString());
                dat.Win_WO_Capital = decimal.Parse(row["Win_WO_Capital"].ToString());
                dat.Capital = decimal.Parse(row["Capital"].ToString());
                dat.WL = decimal.Parse(row["WL"].ToString());

                overbyaccList.Rows.Add(dat);
            }

            string rJason = JsonConvert.SerializeObject(overbyaccList.Rows);
            return Ok(rJason);
        }

        // --- new with filters -----------------------
        [EnableCors("AllowAll")]
        [Route("GetOverallByBetTypeWithFilter")]
        [HttpPost]
        public IActionResult GetOverallByBetTypeWithFilter([FromBody] OverallWith3ParamsInput model)
        {
            string StartDate = model.StartDate;
            string EndDate = model.EndDate;
            string BetType = model.BetType;
            string TicketNo = model.TicketNo;
            string Account = model.Account;

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate'; ";
            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( ";
            sql = sql + "[ID] int null, ";
            sql = sql + "[BET_TYPE] Nvarchar(200) null, ";
            sql = sql + "[LotteryTypeID] int null, ";
            sql = sql + "[DrawTypeID] int null, ";
            sql = sql + "[UpdateDate] datetime null, ";
            sql = sql + "[Username] Nvarchar(200) null, ";
            sql = sql + "[Bill_No_Ticket] Nvarchar(200) null, ";
            sql = sql + "[TOVER] Numeric (32, 4) null, ";
            sql = sql + "[Pending] numeric(32, 4) null, ";
            sql = sql + "[Openning_Time] datetime null, ";
            sql = sql + "[BetTime] datetime null, ";
            sql = sql + "[Bet_1] Nvarchar(200) null, ";
            sql = sql + "[Bet_2] Nvarchar(200) null, ";
            sql = sql + "[Bet_3] Nvarchar(200) null, ";
            sql = sql + "[BetAmount] Numeric (32, 4) null, ";
            sql = sql + "[WinMoney] Numeric (32, 4) null, ";
            sql = sql + "[IsWin] int null, ";
            sql = sql + "Sum_winlose_AllLOST numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_4d numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_NOT4d numeric (32, 4) null, ";
            sql = sql + "[WL] Numeric (32, 4) null, ";
            sql = sql + "[Lost] numeric (32, 4) null, ";
            sql = sql + "[Win] numeric (32, 4) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempReport ";
            sql = sql + "select ";
            sql = sql + "a.ID, ";
            sql = sql + "c.LotteryTypeName as BET_TYPE, ";
            sql = sql + "b.LotteryTypeID, ";
            sql = sql + "DrawTypeID, ";
            sql = sql + "a.UpdateDate, ";
            sql = sql + "a.Username, ";
            sql = sql + "a.CurrentPeriod as Bill_No_Ticket, ";
            sql = sql + "isnull(Price, 0) as TOVER, ";
            sql = sql + "( ";
            sql = sql + "case ";
            sql = sql + "when (iswin is null) then TRY_CONVERT(numeric(38,12),Price) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Pending, ";
            sql = sql + "ShowResultDate as Openning_Time, ";
            sql = sql + "a.CreateDate as BetTime, ";
            sql = sql + "FamliyBigID as Bet_1, ";
            sql = sql + "b.LotteryInfoName as Bet_2, ";
            sql = sql + "SelectedNums as Bet_3, ";
            sql = sql + "isnull(DiscountPrice, 0) as BetAmount, ";
            sql = sql + "isnull(WinMoney, 0), ";
            sql = sql + "IsWin, ";
            sql = sql + "( ";
            sql = sql + "case ";
            sql = sql + "when ( a.UpdateDate between @Date11 AND @Date22 and WinMoney = '0.0000' and Iswin is not null ) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllLOST, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_4d, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID NOT between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then isnull( TRY_CONVERT(numeric(38,12),WinMoney), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_NOT4d, ";
            sql = sql + "0.00 as WL, ";
            sql = sql + "0.00 as Lost, ";
            sql = sql + "0.00 as Win ";
            sql = sql + "from [dbo].[MPlayer] a ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID ";
            sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";

            if (BetType != "")
            {
                sql = sql + "and c.LotteryTypeName = '@dbBetType' ";
            }

            if (Account != "")
            {
                sql = sql + "and a.Username = '@dbAccount' ";
            }

            if (TicketNo != "")
            {
                sql = sql + "and a.CurrentPeriod = '@dbTicket' ";
            }

            sql = sql + "update #TempReport ";
            sql = sql + "set WL = (Sum_winlose_AllLOST + Sum_winlose_AllWIN_4d + Sum_winlose_AllWIN_NOT4d) ";
            sql = sql + ", Lost = case	 ";
            sql = sql + "when (DrawTypeID between 142 and 152 and WinMoney <> 0 and Iswin is not null) then ISNULL(WinMoney, 0)- isnull(BetAmount, 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ", Win = case ";
            sql = sql + "when (DrawTypeID between 142 and 152 and WinMoney <> 0 and Iswin is not null) then ISNULL(WinMoney, 0)-isnull(BetAmount, 0) ";
            sql = sql + "when (DrawTypeID NOT between 142 and 152 and WinMoney <> 0 and Iswin is not null) then isnull( WinMoney, 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ", Sum_winlose_AllLOST = Sum_winlose_AllLOST * -1 ";
            sql = sql + "drop table if exists #TempSummary ";
            sql = sql + "Create table #TempSummary ( ";
            sql = sql + "Bet_Type nvarchar(max) null, ";
            sql = sql + "DrawTypeID int null, ";
            sql = sql + "UserName nvarchar(max) null, ";
            sql = sql + "Bill_No_Ticket nvarchar(max) null, ";
            sql = sql + "Openning_Time datetime null, ";
            sql = sql + "IsWin int null, ";
            sql = sql + "TOver numeric (32, 12) null, ";
            sql = sql + "Pending numeric (32, 12) null, ";
            sql = sql + "BetAmount numeric (32, 12) null, ";
            sql = sql + "WL numeric (32, 12) null, ";
            sql = sql + "Sum_AllLost numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_4d numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_Not4D numeric (32, 12) null, ";
            sql = sql + "TotalWin numeric (32, 12) null, ";
            sql = sql + "TotalLost numeric (32, 12) null, ";
            sql = sql + "Margin numeric (32, 12) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempSummary ";
            sql = sql + "select Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", IsWin ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(Pending) as Pending ";
            sql = sql + ", sum(BetAmount) as BetAmount ";
            sql = sql + ", sum(WL) as MemberWinLose ";
            sql = sql + ", sum(Sum_winlose_AllLOST) as Sum_AllLOST ";
            sql = sql + ", sum(Sum_winlose_AllWIN_4d) as Sum_AllWIN_4d ";
            sql = sql + ", sum(Sum_winlose_AllWIN_NOT4d) as Sum_AllWIN_NOT4d ";
            sql = sql + ", TotalWin = case ";
            sql = sql + "when (#TempReport.isWin = 1) then sum(Sum_winlose_AllWIN_4d) + sum(Sum_winlose_AllWIN_NOT4d) + sum(BetAmount) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ", TotalLost = sum(Sum_winlose_AllLOST)  ";
            sql = sql + ", Margin =  sum(BetAmount) - sum(Sum_winlose_AllLOST)   ";
            sql = sql + "from #TempReport ";
            sql = sql + "group by ";
            sql = sql + "Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", IsWin ";
            sql = sql + "order by ";
            sql = sql + "UserName, Bill_No_Ticket ";
            sql = sql + "drop table if exists #TempSummary2 ";
            sql = sql + "create table #TempSummary2 ";
            sql = sql + "( ";
            sql = sql + "Bet_Type nvarchar(max) null, ";
            sql = sql + "DrawTypeID int null, ";
            sql = sql + "UserName nvarchar(max) null, ";
            sql = sql + "Bill_No_Ticket nvarchar(max) null, ";
            sql = sql + "Openning_Time datetime null, ";
            sql = sql + "TOver numeric (32, 12) null, ";
            sql = sql + "Pending numeric (32, 12) null, ";
            sql = sql + "BetAmount numeric (32, 12) null, ";
            sql = sql + "WL numeric (32, 12) null, ";
            sql = sql + "Sum_AllLost numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_4d numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_Not4D numeric (32, 12) null, ";
            sql = sql + "TotalWin numeric (32, 12) null, ";
            sql = sql + "TotalLost numeric (32, 12) null, ";
            sql = sql + "Margin numeric (32, 12) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempSummary2 ";
            sql = sql + "select ";
            sql = sql + "Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(Pending) as Pending ";
            sql = sql + ", sum(BetAmount) as BetAmount ";
            sql = sql + ", sum(WL) as MemberWinLose ";
            sql = sql + ", sum(Sum_AllLOST) as Sum_AllLOST ";
            sql = sql + ", sum(Sum_AllWIN_4d) as Sum_AllWIN_4d ";
            sql = sql + ", sum(Sum_AllWIN_NOT4d) as Sum_AllWIN_NOT4d ";
            sql = sql + ", case ";
            sql = sql + "when (DrawTypeID NOT between 142 and 152) then sum(TotalWin) - sum(Margin) ";
            sql = sql + "else sum(TotalWin) ";
            sql = sql + "end as TotalWin ";
            sql = sql + ", sum(TotalLost) as TotalLost ";
            sql = sql + ", sum(Margin) as TotalMargin ";
            sql = sql + "from #TempSummary ";
            sql = sql + "group by ";
            sql = sql + "Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + "order by ";
            sql = sql + "Bill_No_Ticket desc ";
            sql = sql + "select ";
            sql = sql + "UserName ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(TotalWin) as TotalWin ";
            sql = sql + ", sum(TotalLost) as TotalLost ";
            sql = sql + ", sum(Pending) as TotalPending ";
            sql = sql + "from #TempSummary2 ";
            sql = sql + "group by ";
            sql = sql + "UserName ";
            sql = sql + "order by ";
            sql = sql + "UserName ";

            string sql2 = sql.Replace("@dbStartDate", StartDate)
                .Replace("@dbEndDate", EndDate)
                .Replace("@dbBetType", BetType)
                .Replace("@dbAccount", Account)
                .Replace("@dbTicket", TicketNo);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 600; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            OverallByAccountList overbyaccList = new OverallByAccountList();
            overbyaccList.Rows = new List<OverallByAccount>();

            int maxrows = myDataRows.Rows.Count;

            for (int i = 0; i < maxrows; i++)
            {
                DataRow row = myDataRows.Rows[i];

                OverallByAccount dat = new OverallByAccount();
                dat.UserName = row["UserName"].ToString();
                dat.TurnOver = decimal.Parse(row["TOver"].ToString());
                dat.TotalWin = decimal.Parse(row["TotalWin"].ToString());
                dat.TotalLost = decimal.Parse(row["TotalLost"].ToString());
                dat.TotalPending = decimal.Parse(row["TotalPending"].ToString());

                overbyaccList.Rows.Add(dat);
            }

            string rJason = JsonConvert.SerializeObject(overbyaccList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetOverallByUser")]
        [HttpPost]
        public IActionResult GetOverallByUser([FromBody] OverallByUserNameInput model)
        {
            string StartDate = model.StartDate;
            string EndDate = model.EndDate;
            string BetType = model.BetType;
            string UserName = model.UserName;
            string CurrentPeriod = model.CurrentPeriod;

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate'; ";
            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( ";
            sql = sql + "[ID] int null, ";
            sql = sql + "[BET_TYPE] Nvarchar(200) null, ";
            sql = sql + "[LotteryTypeID] int null, ";
            sql = sql + "[DrawTypeID] int null, ";
            sql = sql + "[UpdateDate] datetime null, ";
            sql = sql + "[Username] Nvarchar(200) null, ";
            sql = sql + "[Bill_No_Ticket] Nvarchar(200) null, ";
            sql = sql + "[TOVER] Numeric (32, 4) null, ";
            sql = sql + "[Pending] numeric(32, 4) null, ";
            sql = sql + "[Openning_Time] datetime null, ";
            sql = sql + "[BetTime] datetime null, ";
            sql = sql + "[Bet_1] Nvarchar(200) null, ";
            sql = sql + "[Bet_2] Nvarchar(200) null, ";
            sql = sql + "[Bet_3] Nvarchar(200) null, ";
            sql = sql + "[BetAmount] Numeric (32, 4) null, ";
            sql = sql + "[WinMoney] Numeric (32, 4) null, ";
            sql = sql + "[IsWin] int null, ";
            sql = sql + "Sum_winlose_AllLOST numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_4d numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_NOT4d numeric (32, 4) null, ";
            sql = sql + "[WL] Numeric (32, 4) null, ";
            sql = sql + "[Lost] numeric (32, 4) null, ";
            sql = sql + "[Win] numeric (32, 4) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempReport ";
            sql = sql + "select ";
            sql = sql + "a.ID, ";
            sql = sql + "c.LotteryTypeName as BET_TYPE, ";
            sql = sql + "b.LotteryTypeID, ";
            sql = sql + "DrawTypeID, ";
            sql = sql + "a.UpdateDate, ";
            sql = sql + "a.Username, ";
            sql = sql + "a.CurrentPeriod as Bill_No_Ticket, ";
            sql = sql + "isnull(Price, 0) as TOVER, ";
            sql = sql + "( ";
            sql = sql + "case ";
            sql = sql + "when (iswin is null) then TRY_CONVERT(numeric(38,12),Price) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Pending, ";
            sql = sql + "ShowResultDate as Openning_Time, ";
            sql = sql + "a.CreateDate as BetTime, ";
            sql = sql + "FamliyBigID as Bet_1, ";
            sql = sql + "b.LotteryInfoName as Bet_2, ";
            sql = sql + "SelectedNums as Bet_3, ";
            sql = sql + "isnull(DiscountPrice, 0) as BetAmount, ";
            sql = sql + "isnull(WinMoney, 0), ";
            sql = sql + "IsWin, ";
            sql = sql + "( ";
            sql = sql + "case ";
            sql = sql + "when ( a.UpdateDate between @Date11 AND @Date22 and WinMoney = '0.0000' and Iswin is not null ) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllLOST, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_4d, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID NOT between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then isnull( TRY_CONVERT(numeric(38,12),WinMoney), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_NOT4d, ";
            sql = sql + "0.00 as WL, ";
            sql = sql + "0.00 as Lost, ";
            sql = sql + "0.00 as Win ";
            sql = sql + "from [dbo].[MPlayer] a ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID ";

            if (CurrentPeriod == "")
            {
                sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";
            }
            else
            {
                sql = sql + "where a.CurrentPeriod = '" + CurrentPeriod + "' ";
            }

            if (UserName != "")
            {
                sql = sql + "and a.UserName like '%" + UserName + "%' ";
            }

            if (BetType != "")
            {
                sql = sql + "and c.LotteryTypeName = '" + BetType + "' ";
            }

            //sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";

            sql = sql + "and c.LotteryTypeName = '@dbBetType' ";
            sql = sql + "and a.UserName = '@dbAccount' ";
            sql = sql + "update #TempReport ";
            sql = sql + "set WL = (Sum_winlose_AllLOST + Sum_winlose_AllWIN_4d + Sum_winlose_AllWIN_NOT4d) ";
            sql = sql + ", Lost = case ";
            sql = sql + "when (DrawTypeID between 142 and 152 and WinMoney <> 0 and Iswin is not null) then ISNULL(WinMoney, 0)- isnull(BetAmount, 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ", Win = case ";
            sql = sql + "when (DrawTypeID between 142 and 152 and WinMoney <> 0 and Iswin is not null) then ISNULL(WinMoney, 0)-isnull(BetAmount, 0) ";
            sql = sql + "when (DrawTypeID NOT between 142 and 152 and WinMoney <> 0 and Iswin is not null) then isnull( WinMoney, 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ", Sum_winlose_AllLOST = Sum_winlose_AllLOST * -1 ";
            sql = sql + "drop table if exists #TempSummary ";
            sql = sql + "Create table #TempSummary ( ";
            sql = sql + "Bet_Type nvarchar(max) null, ";
            sql = sql + "DrawTypeID int null, ";
            sql = sql + "UserName nvarchar(max) null, ";
            sql = sql + "Bill_No_Ticket nvarchar(max) null, ";
            sql = sql + "Openning_Time datetime null, ";
            sql = sql + "IsWin int null, ";
            sql = sql + "TOver numeric (32, 12) null, ";
            sql = sql + "Pending numeric (32, 12) null, ";
            sql = sql + "BetAmount numeric (32, 12) null, ";
            sql = sql + "WL numeric (32, 12) null, ";
            sql = sql + "Sum_AllLost numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_4d numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_Not4D numeric (32, 12) null, ";
            sql = sql + "TotalWin numeric (32, 12) null, ";
            sql = sql + "TotalLost numeric (32, 12) null, ";
            sql = sql + "Margin numeric (32, 12) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempSummary ";
            sql = sql + "select Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", IsWin ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(Pending) as Pending ";
            sql = sql + ", sum(BetAmount) as BetAmount ";
            sql = sql + ", sum(WL) as MemberWinLose ";
            sql = sql + ", sum(Sum_winlose_AllLOST) as Sum_AllLOST ";
            sql = sql + ", sum(Sum_winlose_AllWIN_4d) as Sum_AllWIN_4d ";
            sql = sql + ", sum(Sum_winlose_AllWIN_NOT4d) as Sum_AllWIN_NOT4d ";
            sql = sql + ", TotalWin = case ";
            sql = sql + "when (#TempReport.isWin = 1) then sum(Sum_winlose_AllWIN_4d) + sum(Sum_winlose_AllWIN_NOT4d) + sum(BetAmount) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ", TotalLost = sum(Sum_winlose_AllLOST) ";
            sql = sql + ", Margin =  sum(BetAmount) - sum(Sum_winlose_AllLOST) ";
            sql = sql + "from #TempReport ";
            sql = sql + "group by ";
            sql = sql + "Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", IsWin ";
            sql = sql + "order by ";
            sql = sql + "UserName, Bill_No_Ticket ";
            sql = sql + "drop table if exists #TempSummary2 ";
            sql = sql + "create table #TempSummary2 ";
            sql = sql + "( ";
            sql = sql + "Bet_Type nvarchar(max) null, ";
            sql = sql + "DrawTypeID int null, ";
            sql = sql + "UserName nvarchar(max) null, ";
            sql = sql + "Bill_No_Ticket nvarchar(max) null, ";
            sql = sql + "Openning_Time datetime null, ";
            sql = sql + "TOver numeric (32, 12) null, ";
            sql = sql + "Pending numeric (32, 12) null, ";
            sql = sql + "BetAmount numeric (32, 12) null, ";
            sql = sql + "WL numeric (32, 12) null, ";
            sql = sql + "Sum_AllLost numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_4d numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_Not4D numeric (32, 12) null, ";
            sql = sql + "TotalWin numeric (32, 12) null, ";
            sql = sql + "TotalLost numeric (32, 12) null, ";
            sql = sql + "Margin numeric (32, 12) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempSummary2 ";
            sql = sql + "select ";
            sql = sql + "Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(Pending) as Pending ";
            sql = sql + ", sum(BetAmount) as BetAmount ";
            sql = sql + ", sum(WL) as MemberWinLose ";
            sql = sql + ", sum(Sum_AllLOST) as Sum_AllLOST ";
            sql = sql + ", sum(Sum_AllWIN_4d) as Sum_AllWIN_4d ";
            sql = sql + ", sum(Sum_AllWIN_NOT4d) as Sum_AllWIN_NOT4d ";
            sql = sql + ", case ";
            sql = sql + "when (DrawTypeID NOT between 142 and 152) then sum(TotalWin) - sum(Margin) ";
            sql = sql + "else sum(TotalWin) ";
            sql = sql + "end as TotalWin ";
            sql = sql + ", sum(TotalLost) as TotalLost ";
            sql = sql + ", sum(Margin) as TotalMargin ";
            sql = sql + "from #TempSummary ";
            sql = sql + "group by ";
            sql = sql + "Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + "order by ";
            sql = sql + "Bill_No_Ticket desc ";
            sql = sql + "select ";
            sql = sql + "Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(WL) as TotalWinLose ";
            sql = sql + "from #TempSummary2 ";
            sql = sql + "group by ";
            sql = sql + "Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + "order by ";
            sql = sql + "Bill_No_Ticket ";

            string sql2 = sql.Replace("@dbStartDate", StartDate)
                .Replace("@dbEndDate", EndDate)
                .Replace("@dbAccount", UserName)
                .Replace("@dbBetType", BetType);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 600; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            OverallByUserNameList overallbyunlist = new OverallByUserNameList();
            overallbyunlist.Rows = new List<OverallByUserName>();

            int maxrows = myDataRows.Rows.Count;

            for (int i = 0; i < maxrows; i++)
            {
                DataRow row = myDataRows.Rows[i];

                OverallByUserName dat = new OverallByUserName();
                dat.TicketNo = row["Bill_No_Ticket"].ToString();
                dat.OpeningTime = row["Openning_Time"].ToString();
                dat.TurnOver = decimal.Parse(row["TOver"].ToString());
                dat.TotalWinLose = decimal.Parse(row["TotalWinLose"].ToString());

                overallbyunlist.Rows.Add(dat);
            }

            string rJason = JsonConvert.SerializeObject(overallbyunlist.Rows);
            return Ok(rJason);
        }

        //---- new with filters

        [EnableCors("AllowAll")]
        [Route("GetOverallByUserWithFilter")]
        [HttpPost]
        public IActionResult GetOverallByUserWithFilter([FromBody] OverallWith3ParamsInput model)
        {
            string StartDate = model.StartDate;
            string EndDate = model.EndDate;
            string BetType = model.BetType;
            string TicketNo = model.TicketNo;
            string Account = model.Account;

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbStartDate'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbEndDate'; ";
            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( ";
            sql = sql + "[ID] int null, ";
            sql = sql + "[BET_TYPE] Nvarchar(200) null, ";
            sql = sql + "[LotteryTypeID] int null, ";
            sql = sql + "[DrawTypeID] int null, ";
            sql = sql + "[UpdateDate] datetime null, ";
            sql = sql + "[Username] Nvarchar(200) null, ";
            sql = sql + "[Bill_No_Ticket] Nvarchar(200) null, ";
            sql = sql + "[TOVER] Numeric (32, 4) null, ";
            sql = sql + "[Pending] numeric(32, 4) null, ";
            sql = sql + "[Openning_Time] datetime null, ";
            sql = sql + "[BetTime] datetime null, ";
            sql = sql + "[Bet_1] Nvarchar(200) null, ";
            sql = sql + "[Bet_2] Nvarchar(200) null, ";
            sql = sql + "[Bet_3] Nvarchar(200) null, ";
            sql = sql + "[BetAmount] Numeric (32, 4) null, ";
            sql = sql + "[WinMoney] Numeric (32, 4) null, ";
            sql = sql + "[IsWin] int null, ";
            sql = sql + "Sum_winlose_AllLOST numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_4d numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_NOT4d numeric (32, 4) null, ";
            sql = sql + "[WL] Numeric (32, 4) null, ";
            sql = sql + "[Lost] numeric (32, 4) null, ";
            sql = sql + "[Win] numeric (32, 4) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempReport ";
            sql = sql + "select ";
            sql = sql + "a.ID, ";
            sql = sql + "c.LotteryTypeName as BET_TYPE, ";
            sql = sql + "b.LotteryTypeID, ";
            sql = sql + "DrawTypeID, ";
            sql = sql + "a.UpdateDate, ";
            sql = sql + "a.Username, ";
            sql = sql + "a.CurrentPeriod as Bill_No_Ticket, ";
            sql = sql + "isnull(Price, 0) as TOVER, ";
            sql = sql + "( ";
            sql = sql + "case ";
            sql = sql + "when (iswin is null) then TRY_CONVERT(numeric(38,12),Price) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Pending, ";
            sql = sql + "ShowResultDate as Openning_Time, ";
            sql = sql + "a.CreateDate as BetTime, ";
            sql = sql + "FamliyBigID as Bet_1, ";
            sql = sql + "b.LotteryInfoName as Bet_2, ";
            sql = sql + "SelectedNums as Bet_3, ";
            sql = sql + "isnull(DiscountPrice, 0) as BetAmount, ";
            sql = sql + "isnull(WinMoney, 0), ";
            sql = sql + "IsWin, ";
            sql = sql + "( ";
            sql = sql + "case ";
            sql = sql + "when ( a.UpdateDate between @Date11 AND @Date22 and WinMoney = '0.0000' and Iswin is not null ) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllLOST, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_4d, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID NOT between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then isnull( TRY_CONVERT(numeric(38,12),WinMoney), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_NOT4d, ";
            sql = sql + "0.00 as WL, ";
            sql = sql + "0.00 as Lost, ";
            sql = sql + "0.00 as Win ";
            sql = sql + "from [dbo].[MPlayer] a ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID ";
            sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";

            if (BetType != "")
            {
                sql = sql + "and c.LotteryTypeName = '@dbBetType' ";
            }

            if (Account != "")
            {
                sql = sql + "and a.Username = '@dbAccount' ";
            }

            if (TicketNo != "")
            {
                sql = sql + "and a.CurrentPeriod = '@dbTicket' ";
            }

            sql = sql + "update #TempReport ";
            sql = sql + "set WL = (Sum_winlose_AllLOST + Sum_winlose_AllWIN_4d + Sum_winlose_AllWIN_NOT4d) ";
            sql = sql + ", Lost = case ";
            sql = sql + "when (DrawTypeID between 142 and 152 and WinMoney <> 0 and Iswin is not null) then ISNULL(WinMoney, 0)- isnull(BetAmount, 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ", Win = case ";
            sql = sql + "when (DrawTypeID between 142 and 152 and WinMoney <> 0 and Iswin is not null) then ISNULL(WinMoney, 0)-isnull(BetAmount, 0) ";
            sql = sql + "when (DrawTypeID NOT between 142 and 152 and WinMoney <> 0 and Iswin is not null) then isnull( WinMoney, 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ", Sum_winlose_AllLOST = Sum_winlose_AllLOST * -1 ";
            sql = sql + "drop table if exists #TempSummary ";
            sql = sql + "Create table #TempSummary ( ";
            sql = sql + "Bet_Type nvarchar(max) null, ";
            sql = sql + "DrawTypeID int null, ";
            sql = sql + "UserName nvarchar(max) null, ";
            sql = sql + "Bill_No_Ticket nvarchar(max) null, ";
            sql = sql + "Openning_Time datetime null, ";
            sql = sql + "IsWin int null, ";
            sql = sql + "TOver numeric (32, 12) null, ";
            sql = sql + "Pending numeric (32, 12) null, ";
            sql = sql + "BetAmount numeric (32, 12) null, ";
            sql = sql + "WL numeric (32, 12) null, ";
            sql = sql + "Sum_AllLost numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_4d numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_Not4D numeric (32, 12) null, ";
            sql = sql + "TotalWin numeric (32, 12) null, ";
            sql = sql + "TotalLost numeric (32, 12) null, ";
            sql = sql + "Margin numeric (32, 12) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempSummary ";
            sql = sql + "select Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", IsWin ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(Pending) as Pending ";
            sql = sql + ", sum(BetAmount) as BetAmount ";
            sql = sql + ", sum(WL) as MemberWinLose ";
            sql = sql + ", sum(Sum_winlose_AllLOST) as Sum_AllLOST ";
            sql = sql + ", sum(Sum_winlose_AllWIN_4d) as Sum_AllWIN_4d ";
            sql = sql + ", sum(Sum_winlose_AllWIN_NOT4d) as Sum_AllWIN_NOT4d ";
            sql = sql + ", TotalWin = case ";
            sql = sql + "when (#TempReport.isWin = 1) then sum(Sum_winlose_AllWIN_4d) + sum(Sum_winlose_AllWIN_NOT4d) + sum(BetAmount) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ", TotalLost = sum(Sum_winlose_AllLOST) ";
            sql = sql + ", Margin =  sum(BetAmount) - sum(Sum_winlose_AllLOST) ";
            sql = sql + "from #TempReport ";
            sql = sql + "group by ";
            sql = sql + "Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", IsWin ";
            sql = sql + "order by ";
            sql = sql + "UserName, Bill_No_Ticket ";
            sql = sql + "drop table if exists #TempSummary2 ";
            sql = sql + "create table #TempSummary2 ";
            sql = sql + "( ";
            sql = sql + "Bet_Type nvarchar(max) null, ";
            sql = sql + "DrawTypeID int null, ";
            sql = sql + "UserName nvarchar(max) null, ";
            sql = sql + "Bill_No_Ticket nvarchar(max) null, ";
            sql = sql + "Openning_Time datetime null, ";
            sql = sql + "TOver numeric (32, 12) null, ";
            sql = sql + "Pending numeric (32, 12) null, ";
            sql = sql + "BetAmount numeric (32, 12) null, ";
            sql = sql + "WL numeric (32, 12) null, ";
            sql = sql + "Sum_AllLost numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_4d numeric (32, 12) null, ";
            sql = sql + "Sum_AllWin_Not4D numeric (32, 12) null, ";
            sql = sql + "TotalWin numeric (32, 12) null, ";
            sql = sql + "TotalLost numeric (32, 12) null, ";
            sql = sql + "Margin numeric (32, 12) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempSummary2 ";
            sql = sql + "select ";
            sql = sql + "Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(Pending) as Pending ";
            sql = sql + ", sum(BetAmount) as BetAmount ";
            sql = sql + ", sum(WL) as MemberWinLose ";
            sql = sql + ", sum(Sum_AllLOST) as Sum_AllLOST ";
            sql = sql + ", sum(Sum_AllWIN_4d) as Sum_AllWIN_4d ";
            sql = sql + ", sum(Sum_AllWIN_NOT4d) as Sum_AllWIN_NOT4d ";
            sql = sql + ", case ";
            sql = sql + "when (DrawTypeID NOT between 142 and 152) then sum(TotalWin) - sum(Margin) ";
            sql = sql + "else sum(TotalWin) ";
            sql = sql + "end as TotalWin ";
            sql = sql + ", sum(TotalLost) as TotalLost ";
            sql = sql + ", sum(Margin) as TotalMargin ";
            sql = sql + "from #TempSummary ";
            sql = sql + "group by ";
            sql = sql + "Bet_Type ";
            sql = sql + ", DrawTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + "order by ";
            sql = sql + "Bill_No_Ticket desc ";
            sql = sql + "select ";
            sql = sql + "Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(WL) as TotalWinLose ";
            sql = sql + "from #TempSummary2 ";
            sql = sql + "group by ";
            sql = sql + "Bill_No_Ticket ";
            sql = sql + ", Openning_Time ";
            sql = sql + "order by ";
            sql = sql + "Bill_No_Ticket ";

            string sql2 = sql.Replace("@dbStartDate", StartDate)
                .Replace("@dbEndDate", EndDate)
                .Replace("@dbBetType", BetType)
                .Replace("@dbAccount", Account)
                .Replace("@dbTicket", TicketNo);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 600; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            OverallByUserNameList overallbyunlist = new OverallByUserNameList();
            overallbyunlist.Rows = new List<OverallByUserName>();

            int maxrows = myDataRows.Rows.Count;

            for (int i = 0; i < maxrows; i++)
            {
                DataRow row = myDataRows.Rows[i];

                OverallByUserName dat = new OverallByUserName();
                dat.TicketNo = row["Bill_No_Ticket"].ToString();
                dat.OpeningTime = row["Openning_Time"].ToString();
                dat.TurnOver = decimal.Parse(row["TOver"].ToString());
                dat.TotalWinLose = decimal.Parse(row["TotalWinLose"].ToString());

                overallbyunlist.Rows.Add(dat);
            }

            string rJason = JsonConvert.SerializeObject(overallbyunlist.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetOverallBy_BetType_Ticket_User")]
        [HttpPost]
        public IActionResult GetOverallBy_BetType_Ticket_User([FromBody] OverallWith3ParamsInput model)
        {
            string StartDate = model.StartDate;
            string EndDate = model.EndDate;
            string BetType = model.BetType;
            string TicketNo = model.TicketNo;
            string Account = model.Account;

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbDateS'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbDateE'; ";
            sql = sql + "declare @Ticket nvarchar(max), @Account nvarchar(max), @BetType nvarchar(max) ";
            sql = sql + "set @Ticket = '@dbTicket' ";
            sql = sql + "set @BetType = '@dbBetType' ";
            sql = sql + "set @Account = '@dbAccount' ";
            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( ";
            sql = sql + "[ID] int null, ";
            sql = sql + "[BET_TYPE] Nvarchar(200) null, ";
            sql = sql + "[LotteryTypeID] int null, ";
            sql = sql + "[DrawTypeID] int null, ";
            sql = sql + "[UpdateDate] datetime null, ";
            sql = sql + "[Username] Nvarchar(200) null, ";
            sql = sql + "[Bill_No_Ticket] Nvarchar(200) null, ";
            sql = sql + "[TOVER] Numeric (32, 4) null, ";
            sql = sql + "[Pending] numeric(32, 4) null, ";
            sql = sql + "[Openning_Time] datetime null, ";
            sql = sql + "[BetTime] datetime null, ";
            sql = sql + "[Bet_1] Nvarchar(200) null, ";
            sql = sql + "[Bet_2] Nvarchar(200) null, ";
            sql = sql + "[Bet_3] Nvarchar(200) null, ";
            sql = sql + "[BetAmount] Numeric (32, 4) null, ";
            sql = sql + "[WinMoney] Numeric (32, 4) null, ";
            sql = sql + "[IsWin] int null, ";
            sql = sql + "Sum_winlose_AllLOST numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_4d numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_NOT4d numeric (32, 4) null, ";
            sql = sql + "calc_WL numeric (32, 4) null, ";
            sql = sql + "[WL] Numeric (32, 4) null, ";
            sql = sql + "AgentWinLose numeric (32, 4) null, ";
            sql = sql + "ComWinLose numeric (32, 4) null, ";
            sql = sql + "MAWinLose numeric (32, 4) null, ";
            sql = sql + "SMWinLose numeric (32, 4) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempReport ";
            sql = sql + "select ";
            sql = sql + "a.ID, ";
            sql = sql + "c.LotteryTypeName as BET_TYPE, ";
            sql = sql + "b.LotteryTypeID, ";
            sql = sql + "DrawTypeID, ";
            sql = sql + "a.UpdateDate, ";
            sql = sql + "a.Username, ";
            sql = sql + "a.CurrentPeriod as Bill_No_Ticket, ";
            sql = sql + "isnull(Price, 0) as TOVER, ";
            sql = sql + "case ";
            sql = sql + "when (iswin is null) then TRY_CONVERT(numeric(38,12),Price) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + "as Pending, ";
            sql = sql + "ShowResultDate as Openning_Time, ";
            sql = sql + "a.CreateDate as BetTime, ";
            sql = sql + "FamliyBigID as Bet_1, ";
            sql = sql + "b.LotteryInfoName as Bet_2, ";
            sql = sql + "SelectedNums as Bet_3, ";
            sql = sql + "isnull(DiscountPrice, 0) as BetAmount, ";
            sql = sql + "isnull(WinMoney, 0), ";
            sql = sql + "a.IsWin, ";
            sql = sql + "( ";
            sql = sql + "case ";
            sql = sql + "when ( a.UpdateDate between @Date11 AND @Date22 and WinMoney = '0.0000' and Iswin is not null ) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllLOST, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_4d, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID NOT between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then isnull( TRY_CONVERT(numeric(38,12),WinMoney), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_NOT4d, ";
            sql = sql + "0.00 as calc_WL, ";
            sql = sql + "0.00 as WL, ";
            sql = sql + "0.00 as AgentWinLose, ";
            sql = sql + "0.00 as comWinLose, ";
            sql = sql + "0.00 as MAWinLose, ";
            sql = sql + "0.00 as SMWinLose ";
            sql = sql + "from [dbo].[MPlayer] a ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID ";

            
            sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";

            if (BetType != "")
            {
                sql = sql + "and c.LotteryTypeName = @BetType ";
            }

            if (Account != "")
            {
                sql = sql + "and a.Username = @Account ";
            }

            if (TicketNo != "")
            {
                sql = sql + "and a.CurrentPeriod = @Ticket ";
            }
            sql = sql + "update #TempReport ";
            sql = sql + "set WL = (Sum_winlose_AllLOST + Sum_winlose_AllWIN_4d + Sum_winlose_AllWIN_NOT4d) ";
            sql = sql + "update #TempReport ";
            sql = sql + "set AgentWinLose = (WL * -1) * 0.90008961941362 ";
            sql = sql + ", ComWinLose = (WL * -1) * 0.0998655718072884 ";
            sql = sql + "select Bet_Type ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", LotteryTypeID ";
            sql = sql + ", UserName ";
            sql = sql + ", sum(TOver) as TOver ";
            sql = sql + ", sum(Pending) as Pending ";
            sql = sql + ", sum(WL) as MemberWinLose ";
            sql = sql + ", sum(AgentWinLose) as AgentWinLose ";
            sql = sql + ", sum(ComWinLose) as ComWinLose ";
            sql = sql + ", sum(MAWinLose) as MAWinLose ";
            sql = sql + ", sum(SMWinLose) as SMWinLose ";
            //sql = sql + ", [AgentWLPerc] = sum(AgentWinLose) / (sum(WL) * -1) ";
            //sql = sql + ", [ComWLPerc] = sum(ComWinLose) / (sum(WL) * -1) ";
            sql = sql + "from #TempReport ";
            sql = sql + "group by ";
            sql = sql + "Bet_Type ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", LotteryTypeID ";
            sql = sql + ", UserName ";
            sql = sql + "order by ";
            sql = sql + "UserName ";

            string sql2 = sql.Replace("@dbDateS", StartDate)
                .Replace("@dbDateE", EndDate)
                .Replace("@dbAccount", Account)
                .Replace("@dbTicket", TicketNo)
                .Replace("@dbBetType", BetType);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 600; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            OverallWith_BetType_TicketNo_User_List overbyaccList = new OverallWith_BetType_TicketNo_User_List();
            overbyaccList.Rows = new List<OverallWith_BetType_TicketNo_User>();

            int maxrows = myDataRows.Rows.Count;

            for (int i = 0; i < maxrows; i++)
            {
                DataRow row = myDataRows.Rows[i];

                OverallWith_BetType_TicketNo_User dat = new OverallWith_BetType_TicketNo_User();
                dat.UserName = row["UserName"].ToString();
                dat.BetType = row["Bet_Type"].ToString();
                dat.TOver = decimal.Parse(row["TOver"].ToString());
                dat.Pending = decimal.Parse( row["Pending"].ToString() );
                dat.MemberWinLose = row["MemberWinLose"].ToString();
                dat.AgentWinLose = row["AgentWinLose"].ToString();
                dat.ComWinLose = row["ComWinLose"].ToString();
                dat.MAWinLose = row["MAWinLose"].ToString();
                dat.SMWinLose = row["SMWinLose"].ToString();

                overbyaccList.Rows.Add(dat);
            }

            string rJason = JsonConvert.SerializeObject(overbyaccList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GenExcel_OverallL1")]
        [HttpPost]
        public IActionResult GenExcel_OverallL1([FromBody] TwoParams model)
        {
            var newstr = model.Val1;
            var ReportTitle = model.Val2;
            List<L1class> data = JsonConvert.DeserializeObject<List<L1class>>(newstr);
            //var cc = tt.Count;

            //data = JsonConvert.DeserializeObject<L1class>(newstr);
            // JsonConvert.DeserializeObject<ExcelData_OverallL1>(newstr);
            //var myjson = JsonConvert.DeserializeObject<ExcelData_OverallL1>(newstr);
            //OverallWith_BetType_TicketNo_User_List data = new OverallWith_BetType_TicketNo_User_List();

            
            var wwwRootPath = _env.WebRootPath;
            //var contentRootPath = _env.ContentRootPath;

            string AppLocation = "";
            AppLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            AppLocation = AppLocation.Replace("file:\\", "");
            string date = DateTime.Now.ToShortDateString();
            date = date.Replace("/", "_");
            string filename = "WinLoseReport_Overall_L1_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xlsx";
            string folder = "ExcelFiles";
            string filepath = wwwRootPath + "\\" + folder + "\\" + filename;

            decimal TotalTOver = 0;
            decimal TotalPending = 0;
            decimal TotalMemberWL = 0;
            decimal TotalWinLose = 0;
            decimal TotalComWL = 0;
            decimal TotalAgentWL = 0;
            decimal TotalMAWL = 0;
            decimal TotalSMWL = 0;

            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Sheet001");

                // user name column
                int i = 1; // row number 1 is the Report Title
                int c = 1;


                int datalen = data.Count;

                var format = "#,##0.0000; (#,##0.0000)";

                i = 3; // row 3 will start the column title
                c = 1; // first column is ticket no
                ws.Cell(i, c).Value = "Ticket No";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    L1class d = data[j];
                    ws.Cell(i, c).Value = d.LotteryTypeName;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    i++;
                }

                ws.Cell(i, c).Value = "Total";
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 2
                i = 3;
                c = 2;

                
                ws.Cell(i, c).Value = "Turn Over";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    L1class d = data[j];
                    ws.Cell(i, c).Value = decimal.Parse( d.TOVer.ToString() );
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalTOver = TotalTOver + decimal.Parse(d.TOVer.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalTOver;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 3
                i = 3;
                c = 3;

                ws.Cell(i, c).Value = "Pending";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    L1class d = data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.Pending.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalPending = TotalPending + decimal.Parse(d.Pending.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalPending;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 4
                i = 3;
                c = 4;

                ws.Cell(i, c).Value = "Member W/L";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    L1class d = data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.WL.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalMemberWL = TotalMemberWL + decimal.Parse(d.WL.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalMemberWL;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 5
                i = 3;
                c = 5;

                ws.Cell(i, c).Value = "Agent W/L";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    L1class d = data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.Agent_WL.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalAgentWL = TotalAgentWL + decimal.Parse(d.Agent_WL.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalAgentWL;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 6
                i = 3;
                c = 6;

                ws.Cell(i, c).Value = "Com W/L";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    L1class d = data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.Com_WL.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalComWL += decimal.Parse(d.Com_WL.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalComWL;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 7
                i = 3;
                c = 7;

                ws.Cell(i, c).Value = "MA W/L";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    L1class d = data[j];
                    ws.Cell(i, c).Value = decimal.Parse((0).ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalMAWL += decimal.Parse((0).ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalMAWL;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 8
                i = 3;
                c = 8;

                ws.Cell(i, c).Value = "SM W/L";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    L1class d = data[j];
                    ws.Cell(i, c).Value = decimal.Parse((0).ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalSMWL += decimal.Parse((0).ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalSMWL;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;
                //----------------------------------------- end of columns population ------------------------

                ws.Columns().AdjustToContents();

                i = 1; // row number 1 is the Report Title
                c = 1;
                ws.Cell(i, c).Value = ReportTitle; // newly added
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial");
                //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                wb.SaveAs(filepath);
            }
            
            URLResponseList uRLResponseList = new URLResponseList();
            //var filename = "";
            //var folder = "";
            URLResponse res = new URLResponse();
            res.FileName =  filename;
            res.FolderName =  folder;

            uRLResponseList.Rows = new List<URLResponse>();
            uRLResponseList.Rows.Add(res);
            
            string rJason = JsonConvert.SerializeObject(uRLResponseList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GenExcel_OverallL2")]
        [HttpPost]
        public IActionResult GenExcel_OverallL2([FromBody] InputModel model)
        {
            var newstr = model.InputText;

            var myjson = JsonConvert.DeserializeObject<ExcelData_OverallL2>(newstr);
            OverallByAccountList data = new OverallByAccountList();

            List<L2ExcelFields> SearchList = new List<L2ExcelFields>();

            //--- prepare data for merging ------
            for (int j = 0; j < myjson.Data.Count; j++)
            {
                OverallByAccount dd = myjson.Data[j];
                L2ExcelFields edt = new L2ExcelFields();
                edt.UserName = dd.UserName;
                edt.BillNo = dd.BillNo;
                edt.OpeningTime = dd.ShowResultDate;
                edt.UpdateDate = dd.UpdateDate;
                edt.TurnOver = dd.TurnOver;
                edt.TotalWin = dd.TotalWin;
                edt.TotalLost = dd.TotalLost;
                edt.TotalPending = dd.TotalPending;
                edt.ab = dd.UserName + "-" + dd.BillNo;
                edt.abc = dd.UserName + "-" + dd.BillNo + "-" + dd.ShowResultDate;
                SearchList.Add(edt);
            }

            //var test = myjson.Data.Count.ToString() + ":" + SearchList.Count.ToString();

            var wwwRootPath = _env.WebRootPath;

            string AppLocation = "";
            AppLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            AppLocation = AppLocation.Replace("file:\\", "");
            string date = DateTime.Now.ToShortDateString();
            date = date.Replace("/", "_");
            string filename = "WinLoseReport_Overall_L2_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xlsx";
            string folder = "ExcelFiles";
            string filepath = wwwRootPath + "\\" + folder + "\\" + filename;

            decimal TotalTOver = 0;
            decimal TotalPending = 0;
            decimal TotalMemberWL = 0;
            decimal TotalWinLose = 0;
            decimal TotalComWL = 0;
            decimal TotalAgentWL = 0;
            decimal TotalMAWL = 0;
            decimal TotalSMWL = 0;
            decimal TotalWin = 0;
            decimal TotalLost = 0;
            decimal TotalMemberWinLost = 0;

            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Sheet001");

                // user name column
                int i = 1; // row number 1 is the Report Title
                int c = 1;


                int datalen = myjson.Data.Count;

                var format = "#,##0.0000; (#,##0.0000)";
                var dtformat = "yyyy-MMM-dd hh:mm:ss";

                i = 3; // row 3 will start the column title
                c = 1; // first column is ticket no
                ws.Cell(i, c).Value = "User Name";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                i++;

                var prevUser = "";
                var prevBill = "";
                int rowext = 0;
                bool tomerge = false;
                bool ticketMerge = false;

                int rowTicket = 0;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByAccount d = myjson.Data[j];
                    

                    if (prevUser != d.UserName)
                    {
                        prevUser = d.UserName;
                        prevBill = d.BillNo;

                        for (int k = 0; k < datalen; k++) {
                            if (SearchList[k].UserName == prevUser) {
                                rowext++;
                                tomerge = true;
                            }
                        }

                    }

                    if (tomerge)
                    {
                        ws.Range(ws.Cell(i, c), ws.Cell(i + rowext, c)).Merge();
                        ws.Cell(i, c).Value = d.UserName;
                        ws.Cell(i, c).Style.Font.SetFontSize(12).Font.SetFontName("Arial");
                        ws.Cell(i, c).Style.Font.SetFontSize(12);
                        ws.Cell(i, c).Style.Font.SetFontColor(XLColor.Blue);
                        ws.Cell(i, c).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);

                        prevUser = d.UserName;
                    }
                    else
                    {
                        ws.Cell(i, c).Value = d.UserName;
                        ws.Cell(i, c).Style.Font.SetFontSize(12).Font.SetFontName("Arial");
                        ws.Cell(i, c).Style.Font.SetFontSize(12);
                        ws.Cell(i, c).Style.Font.SetFontColor(XLColor.Red);
                    }
                    i++;

                    
                }

                ws.Cell(i, c).Value = "Total";
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // new field bill no ======================================================================================
                i = 3;
                c = 2;

                ws.Cell(i, c).Value = "Bill No";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                i++;

                tomerge = false;
                for (int j = 0; j < datalen; j++)
                {
                    OverallByAccount d = myjson.Data[j];

                    if (prevUser != d.UserName)
                    {
                        prevUser = d.UserName;
                        prevBill = d.BillNo;

                        tomerge = true;
                    }

                    if (tomerge)
                    {
                        rowTicket = 0;

                        for (int k = 0; k < datalen; k++)
                        {
                            if (SearchList[k].UserName == prevUser && SearchList[k].BillNo == prevBill)
                            {
                                rowTicket++;
                                ticketMerge = true;
                            }
                        }
                    }

                    if (ticketMerge)
                    {
                        ws.Range(ws.Cell(i, c), ws.Cell(i + rowTicket, c)).Merge();

                        ws.Cell(i, c).Value = d.BillNo.ToString();
                        //ws.Cell(i, c).Value = decimal.Parse(d.TurnOver.ToString());
                        //ws.Cell(i, c).Style.NumberFormat.Format = format;
                        ws.Cell(i, c).Style.Font.SetFontSize(12)
                        .Font.SetFontName("Arial");
                        ws.Cell(i, c).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
                    }

                    else
                    {
                        ws.Cell(i, c).Value = d.BillNo.ToString();
                        //ws.Cell(i, c).Value = decimal.Parse(d.TurnOver.ToString());
                        //ws.Cell(i, c).Style.NumberFormat.Format = format;
                        ws.Cell(i, c).Style.Font.SetFontSize(12)
                        .Font.SetFontName("Arial");
                        ws.Cell(i, c).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);

                    }

                    TotalTOver = TotalTOver + decimal.Parse(d.TurnOver.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalTOver;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // new field Opening Time ======================================================================================
                i = 3;
                c = 3;

                ws.Cell(i, c).Value = "Opening Time";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByAccount d = myjson.Data[j];
                    ws.Cell(i, c).Value = DateTime.Parse(d.ShowResultDate.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = dtformat;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    //TotalTOver = TotalTOver + decimal.Parse(d.TurnOver.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalTOver;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // new field Update Time ======================================================================================
                i = 3;
                c = 4;

                ws.Cell(i, c).Value = "Update Time";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByAccount d = myjson.Data[j];
                    ws.Cell(i, c).Value = DateTime.Parse(d.UpdateDate.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = dtformat;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    //TotalTOver = TotalTOver + decimal.Parse(d.TurnOver.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalTOver;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;


                // column 2 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 5; // to be updated later


                ws.Cell(i, c).Value = "Turn Over";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByAccount d = myjson.Data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.TurnOver.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalTOver = TotalTOver + decimal.Parse(d.TurnOver.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalTOver;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 3 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 6;

                ws.Cell(i, c).Value = "Total Win";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByAccount d = myjson.Data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.TotalWin.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalWin = TotalWin + decimal.Parse(d.TotalWin.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalWin;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 4 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 7;

                ws.Cell(i, c).Value = "Total Lose";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByAccount d = myjson.Data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.TotalLost.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalLost = TotalLost + decimal.Parse(d.TotalLost.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalLost;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // new field
                // column 5 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 8;

                ws.Cell(i, c).Value = "Member W/L";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByAccount d = myjson.Data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.WL.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalMemberWinLost = TotalMemberWinLost + decimal.Parse(d.WL.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalMemberWinLost;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;




                // column 6 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 9;

                ws.Cell(i, c).Value = "Total Pending";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByAccount d = myjson.Data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.TotalPending.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalPending = TotalPending + decimal.Parse(d.TotalPending.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalPending;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                //----------------------------------------- end of columns population ------------------------

                ws.Columns().AdjustToContents();

                i = 1; // row number 1 is the Report Title
                c = 1;
                ws.Cell(i, c).Value = myjson.ReportTitle; // newly added
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial");
                //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                wb.SaveAs(filepath);
            }

            URLResponseList uRLResponseList = new URLResponseList();

            URLResponse res = new URLResponse();
            res.FileName = filename;
            res.FolderName = folder;

            uRLResponseList.Rows = new List<URLResponse>();
            uRLResponseList.Rows.Add(res);

            string rJason = JsonConvert.SerializeObject(uRLResponseList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GenExcel_OverallL2_V2")]
        [HttpPost]
        public IActionResult GenExcel_OverallL2_V2([FromBody] InputModel model)
        {
            var newstr = model.InputText;

            var myjson = JsonConvert.DeserializeObject<ExcelData_OverallL2>(newstr);
            OverallByAccountList data = new OverallByAccountList();

            List<L2ExcelFields> SearchList = new List<L2ExcelFields>();

            //--- prepare data for merging ------
            for (int j = 0; j < myjson.Data.Count; j++)
            {
                OverallByAccount dd = myjson.Data[j];
                L2ExcelFields edt = new L2ExcelFields();
                edt.UserName = dd.UserName;
                edt.BillNo = dd.BillNo;
                edt.OpeningTime = dd.ShowResultDate;
                edt.UpdateDate = dd.UpdateDate;
                edt.TurnOver = dd.TurnOver;
                edt.TotalWin = dd.TotalWin;
                edt.TotalLost = dd.TotalLost;
                edt.TotalPending = dd.TotalPending;
                edt.WL = dd.WL;
                edt.ab = dd.UserName + "-" + dd.BillNo;
                edt.abc = dd.UserName + "-" + dd.BillNo + "-" + dd.ShowResultDate;
                SearchList.Add(edt);
            }

            //var test = myjson.Data.Count.ToString() + ":" + SearchList.Count.ToString();

            var wwwRootPath = _env.WebRootPath;

            string AppLocation = "";
            AppLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            AppLocation = AppLocation.Replace("file:\\", "");
            string date = DateTime.Now.ToShortDateString();
            date = date.Replace("/", "_");
            string filename = "WinLoseReport_Overall_L2_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xlsx";
            string folder = "ExcelFiles";
            string filepath = wwwRootPath + "\\" + folder + "\\" + filename;

            decimal TotalTOver = 0;
            decimal TotalPending = 0;
            decimal TotalMemberWL = 0;
            decimal TotalWinLose = 0;
            decimal TotalComWL = 0;
            decimal TotalAgentWL = 0;
            decimal TotalMAWL = 0;
            decimal TotalSMWL = 0;
            decimal TotalWin = 0;
            decimal TotalLost = 0;
            decimal TotalWL = 0;

            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Sheet001");

                //--- this is the Header row ----
                // user name column
                int i = 1; // row number 1 is the Report Title
                int c = 1;


                int datalen = myjson.Data.Count;

                var format = "#,##0.0000; (#,##0.0000)";
                var dtformat = "yyyy-MMM-dd hh:mm:ss";

                i = 3; 
                c = 1; 
                ws.Cell(i, c).Value = "UserName";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 2; 
                ws.Cell(i, c).Value = "BillNo";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 3; 
                ws.Cell(i, c).Value = "OpeningTime";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 4; 
                ws.Cell(i, c).Value = "UpdateTime";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 5;
                ws.Cell(i, c).Value = "TOver";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 6;
                ws.Cell(i, c).Value = "Win";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 7;
                ws.Cell(i, c).Value = "Lose";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 8;
                ws.Cell(i, c).Value = "Member W/L";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.Bold = true;


                c = 9;
                ws.Cell(i, c).Value = "Pending";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.Bold = true;


                // ---- adding data row ----------
                var prevUser = "";
                var prevTicket = "";
                var prevShowTime = "";
                var prevAB = "";
                var prevABC = "";

                int rowUser = 0;
                int rowTicket = 0;
                int rowShowTime = 0;

                bool mergeUser = false;
                bool mergeTicket = false;
                bool mergeShowTime = false;

                i = 4;
                for (int j = 0; j < datalen; j++)
                {
                    OverallByAccount d = myjson.Data[j];

                    c = 1;

                    rowUser = 0;

                    if (prevUser != d.UserName)
                    {
                        prevUser = d.UserName;
                        prevAB = d.UserName + "-" + d.BillNo;
                        mergeUser = true;
                        
                        for (int k = 0; k < datalen; k++) {
                            if (SearchList[k].UserName == prevUser)
                            {
                                rowUser++;
                            }
                        }

                        int newrow = i + rowUser - 1;
                        ws.Range(ws.Cell(i, c), ws.Cell(newrow, c)).Merge();
                        ws.Cell(i, c).Value = prevUser;
                        ws.Cell(i, c).Style.Font.SetFontSize(12);
                        //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                        ws.Cell(i, c).Style.Font.FontColor = XLColor.Black; // this is the merged field on User Name
                        ws.Cell(i, c).Style.Font.SetFontName("Arial");
                        ws.Cell(i, c).Style.Font.Bold = true;
                        ws.Cell(i, c).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);

                        rowTicket = 0;
                        for (int k = 0; k < datalen; k++)
                        {
                            if (SearchList[k].ab == prevAB)
                            {
                                rowTicket++;
                            }
                        }
                        c = 2;
                        int newrow2 = i + rowTicket - 1;

                        ws.Range(ws.Cell(i, c), ws.Cell(newrow2, c)).Merge();
                        ws.Cell(i, c).Value = d.BillNo;
                        ws.Cell(i, c).Style.Font.SetFontSize(12);
                        //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                        ws.Cell(i, c).Style.Font.FontColor = XLColor.Black;
                        ws.Cell(i, c).Style.Font.SetFontName("Arial");
                        ws.Cell(i, c).Style.Font.Bold = true;
                        ws.Cell(i, c).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);

                        c = 3;
                        ws.Range(ws.Cell(i, c), ws.Cell(newrow2, c)).Merge();
                        ws.Cell(i, c).Value = DateTime.Parse(d.ShowResultDate.ToString());
                        ws.Cell(i, c).Style.NumberFormat.Format = dtformat;
                        ws.Cell(i, c).Style.Font.SetFontSize(12);
                        //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                        ws.Cell(i, c).Style.Font.FontColor = XLColor.Black;
                        ws.Cell(i, c).Style.Font.SetFontName("Arial");
                        ws.Cell(i, c).Style.Font.Bold = true;
                        ws.Cell(i, c).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
                    }


                    c = 4;
                    ws.Cell(i, c).Value = DateTime.Parse(d.UpdateDate.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = dtformat;
                    ws.Cell(i, c).Style.Font.SetFontSize(12);
                    //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                    ws.Cell(i, c).Style.Font.FontColor = XLColor.Black;
                    ws.Cell(i, c).Style.Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Font.Bold = false;

                    c = 5;
                    ws.Cell(i, c).Value = decimal.Parse(d.TurnOver.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12);
                    //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                    ws.Cell(i, c).Style.Font.FontColor = XLColor.Black;
                    ws.Cell(i, c).Style.Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(i, c).Style.Font.Bold = false;

                    TotalTOver = TotalTOver + decimal.Parse(d.TurnOver.ToString());

                    c = 6;
                    ws.Cell(i, c).Value = decimal.Parse(d.TotalWin.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12);
                    //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                    ws.Cell(i, c).Style.Font.FontColor = XLColor.Black;
                    ws.Cell(i, c).Style.Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(i, c).Style.Font.Bold = false;

                    TotalWin = TotalWin + decimal.Parse(d.TotalWin.ToString());

                    c = 7;
                    ws.Cell(i, c).Value = decimal.Parse(d.TotalLost.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12);
                    //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                    ws.Cell(i, c).Style.Font.FontColor = XLColor.Black;
                    ws.Cell(i, c).Style.Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(i, c).Style.Font.Bold = false;

                    TotalLost = TotalLost + decimal.Parse(d.TotalLost.ToString());

                    c = 8;
                    ws.Cell(i, c).Value = decimal.Parse(d.WL.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12);
                    //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                    ws.Cell(i, c).Style.Font.FontColor = XLColor.Black;
                    ws.Cell(i, c).Style.Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(i, c).Style.Font.Bold = false;

                    TotalWL = TotalWL + decimal.Parse(d.WL.ToString());

                    c = 9;
                    ws.Cell(i, c).Value = decimal.Parse(d.TotalPending.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12);
                    //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                    ws.Cell(i, c).Style.Font.FontColor = XLColor.Black;
                    ws.Cell(i, c).Style.Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(i, c).Style.Font.Bold = false;

                    TotalPending = TotalPending + decimal.Parse(d.TotalPending.ToString());

                    i++;
                }


                // ---- writing the final total line -------------------------

                i = datalen + 4;

                c = 1;
                ws.Cell(i, c).Value = "Total";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 2;
                ws.Cell(i, c).Value = "";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 3;
                ws.Cell(i, c).Value = "";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 4;
                ws.Cell(i, c).Value = "";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 5;
                ws.Cell(i, c).Value = decimal.Parse(TotalTOver.ToString());
                ws.Cell(i, c).Style.NumberFormat.Format = format;

                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 6;
                ws.Cell(i, c).Value = decimal.Parse(TotalWin.ToString());
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 7;
                ws.Cell(i, c).Value = decimal.Parse(TotalLost.ToString());
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 8;
                ws.Cell(i, c).Value = decimal.Parse(TotalWL.ToString());
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.Bold = true;

                c = 9;
                ws.Cell(i, c).Value = decimal.Parse(TotalPending.ToString());
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.Bold = true;

                //----------------------------------------- end of columns population ------------------------

                ws.Columns().AdjustToContents();

                i = 1; // row number 1 is the Report Title
                c = 1;
                ws.Cell(i, c).Value = myjson.ReportTitle; // newly added
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial");
                //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                wb.SaveAs(filepath);

                
            }

            URLResponseList uRLResponseList = new URLResponseList();

            URLResponse res = new URLResponse();
            res.FileName = filename;
            res.FolderName = folder;

            uRLResponseList.Rows = new List<URLResponse>();
            uRLResponseList.Rows.Add(res);

            string rJason = JsonConvert.SerializeObject(uRLResponseList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GenExcel_OverallL3")]
        [HttpPost]
        public IActionResult GenExcel_OverallL3([FromBody] InputModel model)
        {
            var newstr = model.InputText;

            var myjson = JsonConvert.DeserializeObject<ExcelData_OverallL3>(newstr);
            OverallByTicketList data = new OverallByTicketList();

            var wwwRootPath = _env.WebRootPath;

            string AppLocation = "";
            AppLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            AppLocation = AppLocation.Replace("file:\\", "");
            string date = DateTime.Now.ToShortDateString();
            date = date.Replace("/", "_");
            string filename = "WinLoseReport_Overall_L3_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xlsx";
            string folder = "ExcelFiles";
            string filepath = wwwRootPath + "\\" + folder + "\\" + filename;

            decimal TotalTOver = 0;
            decimal TotalPending = 0;
            decimal TotalMemberWL = 0;
            decimal TotalWinLose = 0;
            decimal TotalComWL = 0;
            decimal TotalAgentWL = 0;
            decimal TotalMAWL = 0;
            decimal TotalSMWL = 0;
            decimal TotalWin = 0;
            decimal TotalLost = 0;

            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Sheet001");

                // user name column
                int i = 1; // row number 1 is the Report Title
                int c = 1;


                int datalen = myjson.Data.Count;

                var format = "#,##0.0000; (#,##0.0000)";

                var formatdt = "yyyy-MM-dd HH:mm:ss";

                i = 3; // row 3 will start the column title
                c = 1; // first column is ticket no
                ws.Cell(i, c).Value = "Ticket No";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByTicket d = myjson.Data[j];
                    ws.Cell(i, c).Value = d.TicketNo;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    i++;
                }

                ws.Cell(i, c).Value = "";
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 2 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 2;


                ws.Cell(i, c).Value = "Opening Time";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByTicket d = myjson.Data[j];
                    ws.Cell(i, c).Value = d.OpeningTime.ToString();
                    ws.Cell(i, c).Style.NumberFormat.Format = formatdt;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    i++;
                }

                ws.Cell(i, c).Value = "Total";
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 3 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 3;

                ws.Cell(i, c).Value = "Total TOver";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByTicket d = myjson.Data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.TotalTurnOver.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalTOver = TotalTOver + decimal.Parse(d.TotalTurnOver.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalTOver;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 4 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 4;

                ws.Cell(i, c).Value = "Total Win/Lose";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByTicket d = myjson.Data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.TotalWinLose.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalWinLose = TotalWinLose + decimal.Parse(d.TotalWinLose.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalWinLose;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                //----------------------------------------- end of columns population ------------------------

                ws.Columns().AdjustToContents();

                i = 1; // row number 1 is the Report Title
                c = 1;
                ws.Cell(i, c).Value = myjson.ReportTitle; // newly added
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial");
                //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                wb.SaveAs(filepath);
            }

            URLResponseList uRLResponseList = new URLResponseList();

            URLResponse res = new URLResponse();
            res.FileName = filename;
            res.FolderName = folder;

            uRLResponseList.Rows = new List<URLResponse>();
            uRLResponseList.Rows.Add(res);

            string rJason = JsonConvert.SerializeObject(uRLResponseList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GenExcel_OverallL4")]
        [HttpPost]
        public IActionResult GenExcel_OverallL4([FromBody] InputModel model)
        {
            var newstr = model.InputText;

            var myjson = JsonConvert.DeserializeObject<ExcelData_OverallL4>(newstr);
            OverallByBetDetailList data = new OverallByBetDetailList();

            var wwwRootPath = _env.WebRootPath;

            string AppLocation = "";
            AppLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            AppLocation = AppLocation.Replace("file:\\", "");
            string date = DateTime.Now.ToShortDateString();
            date = date.Replace("/", "_");
            string filename = "WinLoseReport_Overall_L4_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xlsx";
            string folder = "ExcelFiles";
            string filepath = wwwRootPath + "\\" + folder + "\\" + filename;

            decimal TotalTOver = 0;
            decimal TotalPending = 0;
            decimal TotalMemberWL = 0;
            decimal TotalWinLose = 0;
            decimal TotalComWL = 0;
            decimal TotalAgentWL = 0;
            decimal TotalMAWL = 0;
            decimal TotalSMWL = 0;
            decimal TotalWin = 0;
            decimal TotalLost = 0;
            decimal TotalBetAmount = 0;

            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Sheet001");

                // user name column
                int i = 1; // row number 1 is the Report Title
                int c = 1;


                int datalen = myjson.Data.Count;

                var format = "#,##0.0000; (#,##0.0000)";

                var formatdt = "yyyy-MM-dd HH:mm:ss";

                i = 3; // row 3 will start the column title
                c = 1; // first column is ticket no
                ws.Cell(i, c).Value = "Bet Type";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByBetDetail d = myjson.Data[j];
                    ws.Cell(i, c).Value = d.BetType;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    i++;
                }

                ws.Cell(i, c).Value = "";
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 2 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 2;


                ws.Cell(i, c).Value = "Bet Time";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByBetDetail d = myjson.Data[j];
                    ws.Cell(i, c).Value = d.BetTime.ToString();
                    ws.Cell(i, c).Style.NumberFormat.Format = formatdt;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    i++;
                }

                ws.Cell(i, c).Value = "";
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 3 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 3;

                ws.Cell(i, c).Value = "Bet";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByBetDetail d = myjson.Data[j];
                    ws.Cell(i, c).Value = d.Bet.ToString();
                    //ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    i++;
                }

                ws.Cell(i, c).Value = "Total";
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 4 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 4;

                ws.Cell(i, c).Value = "Bet Amount";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByBetDetail d = myjson.Data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.BetAmount.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalBetAmount = TotalBetAmount + decimal.Parse(d.BetAmount.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalBetAmount;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                // column 5 ---------------------------------------------------------------------------------------------------
                i = 3;
                c = 5;

                ws.Cell(i, c).Value = "Win/Lose";
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.Font.Bold = true;
                ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                i++;

                for (int j = 0; j < datalen; j++)
                {
                    OverallByBetDetail d = myjson.Data[j];
                    ws.Cell(i, c).Value = decimal.Parse(d.WinLose.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12)
                    .Font.SetFontName("Arial");

                    TotalWinLose = TotalWinLose + decimal.Parse(d.WinLose.ToString());
                    i++;
                }

                ws.Cell(i, c).Value = TotalWinLose;
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.FontColor = XLColor.White;
                ws.Cell(i, c).Style.Font.SetFontName("Arial");
                ws.Cell(i, c).Style.NumberFormat.Format = format;
                ws.Cell(i, c).Style.Font.Bold = true;

                //----------------------------------------- end of columns population ------------------------

                ws.Columns().AdjustToContents();

                i = 1; // row number 1 is the Report Title
                c = 1;
                ws.Cell(i, c).Value = myjson.ReportTitle; // newly added
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial");
                //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                wb.SaveAs(filepath);
            }

            URLResponseList uRLResponseList = new URLResponseList();

            URLResponse res = new URLResponse();
            res.FileName = filename;
            res.FolderName = folder;

            uRLResponseList.Rows = new List<URLResponse>();
            uRLResponseList.Rows.Add(res);

            string rJason = JsonConvert.SerializeObject(uRLResponseList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("DrillOverall_Level4")]
        [HttpPost]
        public IActionResult DrillOverall_Level4([FromBody] OverallWith3ParamsInput model)
        {
            string StartDate = model.StartDate;
            string EndDate = model.EndDate;
            string BetType = model.BetType;
            string TicketNo = model.TicketNo;
            string Account = model.Account;
            string CurrentPeriod = model.CurrentPeriod;

            string sql = "";
            sql = sql + "declare @Date11 DATETIME = '@dbDateS'; ";
            sql = sql + "declare @Date22 DATETIME = '@dbDateE'; ";
            sql = sql + "declare @Ticket nvarchar(max), @Account nvarchar(max), @BetType nvarchar(max) ";
            sql = sql + "set @Ticket = '@dbTicketNo' ";
            sql = sql + "set @BetType = '@dbBetType' ";
            sql = sql + "set @Account = '@dbAccount' ";
            sql = sql + "drop table if exists #TempReport ";
            sql = sql + "create table #TempReport ( ";
            sql = sql + "[ID] int null, ";
            sql = sql + "[BET_TYPE] Nvarchar(200) null, ";
            sql = sql + "[LotteryTypeID] int null, ";
            sql = sql + "[DrawTypeID] int null, ";
            sql = sql + "[UpdateDate] datetime null, ";
            sql = sql + "[Username] Nvarchar(200) null, ";
            sql = sql + "[Bill_No_Ticket] Nvarchar(200) null, ";
            sql = sql + "[TOVER] Numeric (32, 4) null, ";
            sql = sql + "[Pending] numeric(32, 4) null, ";
            sql = sql + "[Openning_Time] datetime null, ";
            sql = sql + "[BetTime] datetime null, ";
            sql = sql + "[Bet_1] Nvarchar(200) null, ";
            sql = sql + "[Bet_2] Nvarchar(200) null, ";
            sql = sql + "[Bet_3] Nvarchar(200) null, ";
            sql = sql + "[BetAmount] Numeric (32, 4) null, ";
            sql = sql + "[WinMoney] Numeric (32, 4) null, ";
            sql = sql + "[IsWin] int null, ";
            sql = sql + "Sum_winlose_AllLOST numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_4d numeric (32, 4) null, ";
            sql = sql + "Sum_winlose_AllWIN_NOT4d numeric (32, 4) null, ";
            sql = sql + "calc_WL numeric (32, 4) null, ";
            sql = sql + "[WL] Numeric (32, 4) null, ";
            sql = sql + "AgentWinLose numeric (32, 4) null, ";
            sql = sql + "ComWinLose numeric (32, 4) null, ";
            sql = sql + "MAWinLose numeric (32, 4) null, ";
            sql = sql + "SMWinLose numeric (32, 4) null ";
            sql = sql + ") ";
            sql = sql + "insert into #TempReport ";
            sql = sql + "select ";
            sql = sql + "a.ID, ";
            sql = sql + "c.LotteryTypeName as BET_TYPE, ";
            sql = sql + "b.LotteryTypeID, ";
            sql = sql + "DrawTypeID, ";
            sql = sql + "a.UpdateDate, ";
            sql = sql + "a.Username, ";
            sql = sql + "a.CurrentPeriod as Bill_No_Ticket, ";
            sql = sql + "isnull(Price, 0) as TOVER, ";
            sql = sql + "case ";
            sql = sql + "when (iswin is null) then TRY_CONVERT(numeric(38,12),Price) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + "as Pending, ";
            sql = sql + "ShowResultDate as Openning_Time, ";
            sql = sql + "a.CreateDate as BetTime, ";
            sql = sql + "FamliyBigID as Bet_1, ";
            sql = sql + "b.LotteryInfoName as Bet_2, ";
            sql = sql + "SelectedNums as Bet_3, ";
            sql = sql + "isnull(DiscountPrice, 0) as BetAmount, ";
            sql = sql + "isnull(WinMoney, 0), ";
            sql = sql + "a.IsWin, ";
            sql = sql + "( ";
            sql = sql + "case ";
            sql = sql + "when ( a.UpdateDate between @Date11 AND @Date22 and WinMoney = '0.0000' and Iswin is not null ) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllLOST, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then ISNULL(TRY_CONVERT(numeric(38,12),WinMoney)-TRY_CONVERT(numeric(38,12),DiscountPrice), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_4d, ";
            sql = sql + "( ";
            sql = sql + "case	when (a.UpdateDate between @Date11 AND @Date22 and DrawTypeID NOT between 142 and 152 and WinMoney <> '0.0000' and Iswin is not null) then isnull( TRY_CONVERT(numeric(38,12),WinMoney), 0) ";
            sql = sql + "else 0 ";
            sql = sql + "end ";
            sql = sql + ") as Sum_winlose_AllWIN_NOT4d, ";
            sql = sql + "0.00 as calc_WL, ";
            sql = sql + "0.00 as WL, ";
            sql = sql + "0.00 as AgentWinLose, ";
            sql = sql + "0.00 as comWinLose, ";
            sql = sql + "0.00 as MAWinLose, ";
            sql = sql + "0.00 as SMWinLose ";
            sql = sql + "from [dbo].[MPlayer] a ";
            sql = sql + "inner join [dbo].[LotteryInfo] b on a.LotteryInfoID = b.LotteryInfoID ";
            sql = sql + "inner join [dbo].[LotteryType] c on b.LotteryTypeID = c.LotteryTypeID ";
            sql = sql + "inner join [dbo].GameDealerMemberShip d on a.GameDealerMemberID = d.MemberID ";

            // sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";

            if (CurrentPeriod == "")
            {
                sql = sql + "where a.UpdateDate between @Date11 AND @Date22 ";
            }
            else
            {
                sql = sql + "where a.CurrentPeriod = '" + CurrentPeriod + "' ";
            }

            if (Account != "")
            {
                sql = sql + "and a.UserName like '%" + Account + "%' ";
            }

            if (BetType != "")
            {
                sql = sql + "and c.LotteryTypeName = '" + BetType + "' ";
            }

            sql = sql + "and c.LotteryTypeName = @BetType ";
            sql = sql + "and a.Username = @Account ";
            sql = sql + "and a.CurrentPeriod = @Ticket ";
            sql = sql + "update #TempReport ";
            sql = sql + "set WL = (Sum_winlose_AllLOST + Sum_winlose_AllWIN_4d + Sum_winlose_AllWIN_NOT4d) ";
            sql = sql + "update #TempReport ";
            sql = sql + "set AgentWinLose = (WL * -1) * 0.90008961941362 ";
            sql = sql + ", ComWinLose = (WL * -1) * 0.0998655718072884 ";
            sql = sql + "select  ";
            sql = sql + "Bet_Type ";
            sql = sql + ", BetTime ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", concat('(', Bet_1 , ')', ' (', Bet_3, ')') as Bet ";
            sql = sql + ", sum(BetAmount) as BetAmount ";
            sql = sql + ", sum(WL) as WL ";
            sql = sql + "from #TempReport ";
            sql = sql + "group by Bet_Type ";
            sql = sql + ", BetTime ";
            sql = sql + ", Bill_No_Ticket ";
            sql = sql + ", concat('(', Bet_1 , ')', ' (', Bet_3, ')') ";
            sql = sql + "order by concat('(', Bet_1 , ')', ' (', Bet_3, ')') ";

            string sql2 = sql.Replace("@dbDateS", StartDate)
                .Replace("@dbDateE", EndDate)
                .Replace("@dbAccount", Account)
                .Replace("@dbTicketNo", TicketNo)
                .Replace("@dbBetType", BetType);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 600; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            OverallLevel4DataList overbyaccList = new OverallLevel4DataList();
            overbyaccList.Rows = new List<OverallLevel4Data>();

            int maxrows = myDataRows.Rows.Count;

            for (int i = 0; i < maxrows; i++)
            {
                DataRow row = myDataRows.Rows[i];

                OverallLevel4Data dat = new OverallLevel4Data();
                dat.BetType = row["Bet_Type"].ToString();
                dat.BetTime = row["BetTime"].ToString();
                dat.Bet = row["Bet"].ToString();
                dat.Bill_No_Ticket = row["Bill_No_Ticket"].ToString();
                dat.BetAmount = decimal.Parse(row["BetAmount"].ToString());
                dat.WinLose = decimal.Parse(row["WL"].ToString());

                overbyaccList.Rows.Add(dat);
            }

            string rJason = JsonConvert.SerializeObject(overbyaccList.Rows);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetYesterday")]
        [HttpPost]
        public IActionResult GetYesterday()
        {
            DateTime today = DateTime.Now;
            DateTime yesterday = today.AddDays(-1);

            ReturnModel rm = new ReturnModel();
            rm.ReturnText = yesterday.ToString("yyyy") + "-" + yesterday.ToString("MM") + "-" + yesterday.ToString("dd");

            string rJason = JsonConvert.SerializeObject(rm);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("Get7Days")]
        [HttpPost]
        public IActionResult Get7Days()
        {
            DateTime today = DateTime.Now;
            DateTime yesterday = today.AddDays(-7);

            DateRange dr = new DateRange();
            dr.StartDate = yesterday.ToString("yyyy") + "-" + yesterday.ToString("MM") + "-" + yesterday.ToString("dd");
            dr.EndDate = today.ToString("yyyy") + "-" + today.ToString("MM") + "-" + today.ToString("dd");

            string rJason = JsonConvert.SerializeObject(dr);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("Get14Days")]
        [HttpPost]
        public IActionResult Get14Days()
        {
            DateTime today = DateTime.Now;
            DateTime yesterday = today.AddDays(-14);

            DateRange dr = new DateRange();
            dr.StartDate = yesterday.ToString("yyyy") + "-" + yesterday.ToString("MM") + "-" + yesterday.ToString("dd");
            dr.EndDate = today.ToString("yyyy") + "-" + today.ToString("MM") + "-" + today.ToString("dd");

            string rJason = JsonConvert.SerializeObject(dr);
            return Ok(rJason);
        }


        [EnableCors("AllowAll")]
        [Route("GetCurrentPeriodDateRange")]
        [HttpPost]
        public IActionResult GetCurrentPeriodDateRange([FromBody] InputModel model)
        {
            string CurrentPeriod = model.InputText;

            string sql = "";
            sql = sql + "declare @CurrentPeriod nvarchar(max); ";
            sql = sql + "set @CurrentPeriod = 'SGPTO_0302' ";
            sql = sql + "select  ";
            sql = sql + "[StartDate] = (select top 1 cast(UpdateDate as Date) from MPlayer where CurrentPeriod = @CurrentPeriod order by UpdateDate asc) ";
            sql = sql + ", [EndDate] = (select top 1 cast(UpdateDate as Date) from MPlayer where CurrentPeriod = @CurrentPeriod order by UpdateDate desc) ";

            string sql2 = sql.Replace("@dbCurrentPeriod", CurrentPeriod);

            SqlConnection connection = new SqlConnection(db_master.connStr);
            DataTable myDataRows = new DataTable();
            SqlCommand command = new SqlCommand(sql2, connection);
            command.CommandTimeout = 600; // 5 minutes (60 seconds X 5)
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(myDataRows);
            connection.Close();

            dateRangeOnly dr = new dateRangeOnly();

            for (int i = 0; i < myDataRows.Rows.Count; i++)
            {
                DataRow drow = myDataRows.Rows[i];

                var sd = DateTime.Parse(drow["StartDate"].ToString().Replace("T", " "));
                var ed = DateTime.Parse(drow["EndDate"].ToString().Replace("T", " "));
                dr.StartDate = sd.ToString("yyyy") + "-" + sd.ToString("MM") + "-" + sd.ToString("dd");
                dr.EndDate = ed.ToString("yyyy") + "-" + ed.ToString("MM") + "-" + ed.ToString("dd");
            }

            string rJason = JsonConvert.SerializeObject(dr);
            return Ok(rJason);
        }

        

    }

    
}
