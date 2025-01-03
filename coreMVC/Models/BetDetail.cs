namespace CoreMVC3.Models
{
    public class BetDetail
    {
        public string CurrentPeriod { get; set; }
        public DateTime CreateDate { get; set; }
        public string UserName { get; set; }
        public string AgentParentMap { get; set; }
        public string FamilyBigID {  get; set; }
        public string SelectedNums { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal WinMoney { get; set; }
        public bool IsWin {  get; set; }

        public decimal WinLose { get; set; }

    }
}
