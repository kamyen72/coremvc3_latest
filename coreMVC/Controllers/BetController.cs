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
using System.Text.Json;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

namespace CoreMVC3.Controllers
{
    [Route("/[controller]")]

    public class BetController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public BetController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [EnableCors("AllowAll")]
        [Route("TestBetTrans")]
        [HttpPost]
        public IActionResult TestBetTrans([FromBody] InputModel model)
        {
            var test = model.InputText;

            ReturnModel rm = new ReturnModel();
            rm.ReturnText = "ok";
            string rJason = JsonConvert.SerializeObject(rm);
            return Ok(rJason);
        }


        [EnableCors("AllowAll")]
        [Route("GetMPlayerRecs")]
        [HttpPost]
        public IActionResult GetMPlayerRecs([FromBody] twodates model)
        {
            twodates td = new twodates();
            td.dateStart = model.dateStart;
            td.dateEnd = model.dateEnd;

            List<MPlayerID> mybets = new List<MPlayerID>();

            DBUtil2 dbu = new DBUtil2();

            mybets = dbu.GetMPlayer(td.dateStart, td.dateEnd);
            var tt = mybets.Count;

            StringBuilder sj = new StringBuilder();

            sj.Append("[");
            int[] myIDs = new int[tt];

            for (int x = 0; x < tt; x++)
            {
                MPlayerID singleTran = mybets[x];
                string singleJ = System.Text.Json.JsonSerializer.Serialize(singleTran);
                if (x > 0)
                {
                    sj.Append(',');
                }
                sj.Append(singleJ);
                //sj.Append(',');
            }
            sj.Append(']');

            var test = sj;

            ReturnModel rm = new ReturnModel();
            rm.ReturnText = mybets.Count.ToString();
            //string rJason = JsonConvert.SerializeObject(mybets);
            string rJason = System.Text.Json.JsonSerializer.Serialize(mybets);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetMPlayerBetInfo")]
        [HttpPost]
        public IActionResult GetMPlayerBetInfo([FromBody] twodates mymodel)
        {
            var xid = mymodel.dateStart;

            BetTrans bt = new BetTrans();
            DBUtil2 dbu = new DBUtil2();
            bt = dbu.GetMPlayerInfo(xid);

            //var x = bt.Username;

            string rJason = JsonConvert.SerializeObject( bt );
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetLotteryTypeSummaryLevel")]
        [HttpPost]
        public IActionResult GetLotteryTypeSummaryLevel([FromBody] twodates mymodel)
        {
            var date1 = mymodel.dateStart;
            var date2 = mymodel.dateEnd;

            List<LotteryTypeSummary> finalsumm = null;

            DBUtil2 dbu = new DBUtil2();

            finalsumm = dbu.Get_LotteryTypeSummary(date1, date2);

            for (int i = 0; i < finalsumm.Count; i++)
            {
                var tt = finalsumm[i];
                int myid = tt.LotteryTypeID;
                tt.LotteryTypeName = dbu.GetLotteryTypeName(myid.ToString());
            }

            string rJason = JsonConvert.SerializeObject(finalsumm);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetCalculatedFields")]
        [HttpPost]
        public IActionResult GetCalculatedFields([FromBody] twodates mymodel)
        {
            var date1 = mymodel.dateStart;
            var date2 = mymodel.dateEnd;
            var LotteryTypeID = int.Parse( mymodel.LotteryTypeID.ToString() );

            DBUtil2 dbu = new DBUtil2();

            MPlayerCalcFields mpc = dbu.Get_MPlayer_CalcFields(date1, date2, LotteryTypeID);

            string rJason = JsonConvert.SerializeObject(mpc);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetMPlayerUsers")]
        [HttpPost]
        public IActionResult GetMPlayerUsers([FromBody] MPlayerUsersInput mymodel)
        {
            var date1 = mymodel.dateStart;
            var date2 = mymodel.dateEnd;
            var LotteryTypeID = int.Parse(mymodel.LotteryTypeID.ToString());
            var BetType = mymodel.BetType;
            var Level1_ID = mymodel.Level1_ID;

            DBUtil2 dbu = new DBUtil2();
            List<MPlayerUser> users = dbu.GetMPlayerUsersV3b(date1.ToString(), date2.ToString(), LotteryTypeID, Level1_ID);

            string rJason = JsonConvert.SerializeObject(users);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GenExcel_OverallL2_V3")]
        [HttpPost]
        public IActionResult GenExcel_OverallL2_V3([FromBody] MPlayerUsersInputWTitle mymodel)
        {
            var date1 = mymodel.dateStart;
            var date2 = mymodel.dateEnd;
            var LotteryTypeID = int.Parse(mymodel.LotteryTypeID.ToString());
            var BetType = mymodel.BetType;
            var ReportTitle = mymodel.ReportTitle;
            var Level1_ID = mymodel.Level1_ID;

            
            //var myjson = JsonConvert.DeserializeObject<ExcelData_OverallL2>(newstr);
            //OverallByAccountList data = new OverallByAccountList();
            DBUtil2 dbu = new DBUtil2();
            List<MPlayerUser> data = dbu.GetMPlayerUsersV3b(date1.ToString(), date2.ToString(), LotteryTypeID, Level1_ID); // using the rt_mplayer
            List<L2ExcelFields> SearchList = new List<L2ExcelFields>();
            //--- prepare data for merging ------
            for (int j = 0; j < data.Count; j++)
            {
                MPlayerUser dd = data[j];
                L2ExcelFields edt = new L2ExcelFields();
                edt.UserName = dd.UserName;
                edt.BillNo = dd.CurrentPeriod;
                edt.OpeningTime = DateTime.Parse( dd.ShowResultDate );
                edt.UpdateDate = DateTime.Parse(dd.UpdateDate);
                edt.TurnOver = dd.TOver;
                edt.TotalWin = dd.TotalWin;
                edt.TotalLost = dd.TotalLost;
                edt.TotalPending = dd.TotalPending;
                edt.WL = dd.WL;
                edt.ab = dd.UserName + "-" + dd.CurrentPeriod;
                edt.abc = dd.UserName + "-" + dd.CurrentPeriod + "-" + dd.ShowResultDate;
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


                int datalen = data.Count;

                var format = "#,##0.0000; (#,##0.0000)";
                var dtformat = "yyyy-MMM-dd hh:mm:ss.000";

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
                    MPlayerUser d = data[j];

                    c = 1;

                    rowUser = 0;

                    if (prevUser != d.UserName)
                    {
                        prevUser = d.UserName;
                        prevAB = d.UserName + "-" + d.CurrentPeriod;
                        mergeUser = true;

                        for (int k = 0; k < datalen; k++)
                        {
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
                        ws.Cell(i, c).Value = d.CurrentPeriod;
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
                    ws.Cell(i, c).Value = decimal.Parse(d.TOver.ToString());
                    ws.Cell(i, c).Style.NumberFormat.Format = format;
                    ws.Cell(i, c).Style.Font.SetFontSize(12);
                    //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                    ws.Cell(i, c).Style.Font.FontColor = XLColor.Black;
                    ws.Cell(i, c).Style.Font.SetFontName("Arial");
                    ws.Cell(i, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(i, c).Style.Font.Bold = false;

                    TotalTOver = TotalTOver + decimal.Parse(d.TOver.ToString());

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
                ws.Cell(i, c).Value = ReportTitle; // newly added
                ws.Cell(i, c).Style.Font.SetFontSize(12)
                .Font.SetFontName("Arial");
                //.Fill.BackgroundColor = XLColor.FromArgb(0x8c8c8c);
                ws.Cell(i, c).Style.Font.Bold = true;

                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                wb.SaveAs(filepath);


            }
            

            //var filename = "nothing.xlsx";
            //var folder = "";
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
        [Route("GetLotteryTypeSummaryLevelV2")]
        [HttpPost]
        public IActionResult GetLotteryTypeSummaryLevelV2([FromBody] twodates mymodel)
        {
            var date1 = mymodel.dateStart;
            var date2 = mymodel.dateEnd;

            List<LotteryTypeSummary> finalsumm = null;

            DBUtil2 dbu = new DBUtil2();

            finalsumm = dbu.Get_LotteryTypeSummaryV2(date1, date2);

            for (int i = 0; i < finalsumm.Count; i++)
            {
                var tt = finalsumm[i];
                int myid = tt.LotteryTypeID;
                tt.LotteryTypeName = dbu.GetLotteryTypeName(myid.ToString());
            }

            string rJason = JsonConvert.SerializeObject(finalsumm);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("Get_Month_ID")]
        [HttpPost]
        public IActionResult Get_Month_ID([FromBody] twodates mymodel)
        {
            var date1 = mymodel.dateStart;
            var date2 = mymodel.dateEnd;

            DBUtil2 dbu = new DBUtil2();

            ReturnModel rm = new ReturnModel();
            rm.ReturnText = dbu.Get_Month_ID(date1, date2 );
            string rJason = JsonConvert.SerializeObject(rm);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetLotteryTypeSummaryLevelV3")]
        [HttpPost]
        public IActionResult GetLotteryTypeSummaryLevelV3([FromBody] twodates mymodel)
        {
            var date1 = mymodel.dateStart;
            var date2 = mymodel.dateEnd;

            List<LotteryTypeSummary> finalsumm = null;

            DBUtil2 dbu = new DBUtil2();

            finalsumm = dbu.Get_LotteryTypeSummaryV3b(date1, date2);

            for (int i = 0; i < finalsumm.Count; i++)
            {
                var tt = finalsumm[i];
                int myid = tt.LotteryTypeID;
                tt.LotteryTypeName = dbu.GetLotteryTypeName(myid.ToString());
            }

            string rJason = JsonConvert.SerializeObject(finalsumm);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("GetLotteryTypeSummaryLevelCount")]
        [HttpPost]
        public IActionResult GetLotteryTypeSummaryLevelCount([FromBody] MPlayerUsersInput mymodel)
        {
            var date1 = mymodel.dateStart;
            var date2 = mymodel.dateEnd;
            var LotteryTypeID = int.Parse(mymodel.LotteryTypeID.ToString());
            var BetType = mymodel.BetType;

            int RecCount = 0;

            DBUtil2 dbu = new DBUtil2();

            RecCount = dbu.GetMPlayerUsersCountV2(date1, date2, LotteryTypeID);

            ReturnModel rm = new ReturnModel();
            rm.ReturnText = RecCount.ToString();
            string rJason = JsonConvert.SerializeObject(rm);
            return Ok(rJason);
        }


        [EnableCors("AllowAll")]
        [Route("ReadTextFile")]
        [HttpPost]
        public IActionResult ReadTextFile([FromBody] InputModel mymodel)
        {
            var wwwRootPath = _env.WebRootPath;
            string AppLocation = "";
            AppLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            AppLocation = AppLocation.Replace("file:\\", "");
            string date = DateTime.Now.ToShortDateString();
            date = date.Replace("/", "_");
            string filename = "robots.txt";
            string folder = "ExcelFiles";
            string filepath = wwwRootPath + "\\" + folder + "\\" + filename;

            //string _sampleFilePath = "/ExcelFiles/robots.txt";

            using StreamWriter sw = new StreamWriter(filepath);
            StringBuilder mySB = new StringBuilder("");

            mySB.AppendLine("<table border=1 cellpadding='0' cellspacing='0'>");
            for (int i = 0; i < 180000; i++)
            {
                string txt = "";
                txt = txt + "<tr>";
                txt = txt + "<td>#" + i.ToString() + "</td>";
                txt = txt + "<td>Field 1</td>";
                txt = txt + "<td>Field 2</td>";
                txt = txt + "<td>Field 3</td>";
                txt = txt + "<td>Field 4</td>";
                txt = txt + "<td>Field 5</td>";
                txt = txt + "<td>Field 6</td>";
                txt = txt + "<td>Field 7</td>";
                txt = txt + "<td>Field 8</td>";
                txt = txt + "<td>Field 9</td>";
                txt = txt + "<td>Field 10</td>";
                txt = txt + "</tr>";
                mySB.AppendLine(txt);
            }

            sw.WriteLine(mySB.ToString());
            sw.Close();
            using var streamReader = new StreamReader(filepath);

            ReturnModel rm = new ReturnModel();
            rm.ReturnText = streamReader.ReadToEnd();
            string rJason = JsonConvert.SerializeObject(rm);
            return Ok(rJason);
        }

        [EnableCors("AllowAll")]
        [Route("ReadJson")]
        [HttpPost]
        public IActionResult ReadJson([FromBody] InputModel mymodel)
        {
            var wwwRootPath = _env.WebRootPath;
            string AppLocation = "";
            AppLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            AppLocation = AppLocation.Replace("file:\\", "");
            string date = DateTime.Now.ToShortDateString();
            date = date.Replace("/", "_");
            string filename = "robots.txt";
            string folder = "ExcelFiles";
            string filepath = wwwRootPath + "\\" + folder + "\\" + filename;

            //string _sampleFilePath = "/ExcelFiles/robots.txt";

            using StreamWriter sw = new StreamWriter(filepath);
            StringBuilder mySB = new StringBuilder("");

            mySB.AppendLine("<table border=1 cellpadding='0' cellspacing='0'>");
            for (int i = 0; i < 180000; i++)
            {
                string txt = "";
                txt = txt + "<tr>";
                txt = txt + "<td>#" + i.ToString() + "</td>";
                txt = txt + "<td>Field 1</td>";
                txt = txt + "<td>Field 2</td>";
                txt = txt + "<td>Field 3</td>";
                txt = txt + "<td>Field 4</td>";
                txt = txt + "<td>Field 5</td>";
                txt = txt + "<td>Field 6</td>";
                txt = txt + "<td>Field 7</td>";
                txt = txt + "<td>Field 8</td>";
                txt = txt + "<td>Field 9</td>";
                txt = txt + "<td>Field 10</td>";
                txt = txt + "</tr>";
                mySB.AppendLine(txt);
            }

            sw.WriteLine(mySB.ToString());
            sw.Close();
            using var streamReader = new StreamReader(filepath);

            ReturnModel rm = new ReturnModel();
            rm.ReturnText = streamReader.ReadToEnd();
            string rJason = JsonConvert.SerializeObject(rm);
            return Ok(rJason);
        }
    }
}
