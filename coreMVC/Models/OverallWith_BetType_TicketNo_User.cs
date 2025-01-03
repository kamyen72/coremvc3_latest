namespace CoreMVC3.Models
{
    public class OverallWith_BetType_TicketNo_User
    {
        public string BetType { get; set; }
        public int LotteryTypeID { get; set; }
        public decimal TOver {  get; set; }
        public decimal Pending { get; set; }
        public string MemberWinLose { get; set; }
        public string AgentWinLose { get; set; }
        public string ComWinLose { get; set; }
        public string MAWinLose { get; set; }
        public string SMWinLose { get; set; }
        public string UserName { get; set; }
    }
}
