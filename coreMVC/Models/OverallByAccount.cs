namespace CoreMVC3.Models
{
    public class OverallByAccount
    {
        public string UserName { get; set; }
        public string BillNo { get; set; }
        public DateTime ShowResultDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public decimal TurnOver {  get; set; }
        public decimal TotalWin { get; set; }
        public decimal TotalLost { get; set; }
        public decimal TotalPending { get; set; }
        public decimal Win_WO_Capital { get; set; }
        public decimal Capital {  get; set; }
        public decimal WL {  get; set; }
    }
}
