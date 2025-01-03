using CoreMVC.Models;
using CoreMVC.Repository;
using Microsoft.AspNetCore.Mvc;
using ResultModificationApp.Models;
using System.Diagnostics;
using Newtonsoft.Json.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using CoreMVC3.Models;

namespace CoreMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public JsonResult AjaxMethod(string startDate, string endDate)
        {
            DateRange person = new DateRange
            {
                StartDate = startDate,
                EndDate = endDate
            };
            return Json(person);
        }

        [HttpPost]
        public String GetLotteryTypes(string startDate, string endDate, string LType, bool ToReverse)
        {
            DateRange dateRange = new DateRange
            {
                StartDate = startDate,
                EndDate = endDate
            };

            clsOLotteryTypeRepository oLotteryTypeRepository = new clsOLotteryTypeRepository();

            List<clsOLotteryType> mylist = oLotteryTypeRepository.GetLotteryTypes(dateRange.StartDate, dateRange.EndDate, LType, ToReverse);

            var jlist = JsonConvert.SerializeObject(mylist);

            return jlist;
        }

        [HttpPost]
        public String GetOLotteryResults(string startDate, string endDate, long LotteryTypeID)
        {
            DateRange dateRange = new DateRange
            {
                StartDate = startDate,
                EndDate = endDate
            };

            clsOLottery31Repository oLottery31Repository = new clsOLottery31Repository();

            List<clsOLottery31> mylist = oLottery31Repository.GetOLotteryResults(dateRange.StartDate, dateRange.EndDate, LotteryTypeID);

            var jlist = JsonConvert.SerializeObject(mylist);

            return jlist;
        }

        [HttpPost]
        public bool UpdateResult(long Lottery31ID, string NewValue, string OldValue)
        {
            clsOLottery31Repository oLottery31Repository = new clsOLottery31Repository();
            bool status = false;
            status = oLottery31Repository.UpdateResult(Lottery31ID, NewValue, OldValue);

            return status;
        }

        [HttpPost]
        public bool IsModified(long Lottery31ID)
        {
            clsOLottery31Repository oLottery31Repository = new clsOLottery31Repository();
            bool status = false;
            status = oLottery31Repository.IsModified(Lottery31ID);

            //Console.WriteLine(status);

            return status;
        }

        [Route("Home/ ckyText")]
        [HttpPost]
        public string ckyText(string inputText)
        {
            string result = "You told me: " + inputText + "!!";
            return result;
        }


    }
}
