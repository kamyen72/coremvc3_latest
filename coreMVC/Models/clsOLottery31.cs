namespace CoreMVC.Models
{
    public class clsOLottery31
    {
            public int OLottery31ID { get; set; }
            public string CurrentPeriod { get; set; }
            public string RealCloseTime { get; set; }
            public string CloseTime { get; set; }
            public string CloseDate { get; set; }
            public bool IsOpen { get; set; }
            public long LotteryTypeID { get; set; }
            public string LotteryTypeName { get; set; }
            public string Result { get; set; }

            public int TimesChanged { get; set; }
    }
}

