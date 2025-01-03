namespace CoreMVC3.Models
{
    public class MPlayerUser
    {
        public string UserName { get; set; }
        public string CurrentPeriod { get; set; }
        public string ShowResultDate { get; set; }
        public string UpdateDate {  get; set; }
        public decimal TOver { get; set; }
        public decimal BetAmount { get; set; }
        public decimal All4DWin { get; set; }
        public decimal Allnon4DWin { get; set; }
        public decimal AllLose { get; set; }
        public decimal WL { get; set; }
        public int IsWin { get; set; }
        public decimal WinMoney { get; set; }
        public int DrawTypeID { get; set; }
        public decimal Margin { get; set; }
        public decimal TotalWin { get; set; }
        public decimal TotalLost { get; set; }
        public decimal TotalPending { get; set; }
        public string Level2_ID { get; set; }
    }
}
