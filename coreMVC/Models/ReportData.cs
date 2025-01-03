namespace CoreMVC3.Models
{
    public class ReportData
    {
        public string UserName { get; set; }
        public string UpdateDate { get; set; }
        public string BET_TYPE { get; set; }
        public string Bill_No_Ticket { get; set; }
        public int Recs { get; set; }
        public decimal TOver { get; set; }
        public decimal BetAmount { get; set; }
        public decimal WinMoney { get; set; }
        public decimal Sum_winlose_AllLOST { get; set; }
        public decimal Sum_winlose_AllWIN_4d { get; set; }
        public decimal Sum_winlose_AllWIN_NOT4d { get; set; }
        public decimal Win_Lose { get; set; }
    }
}

