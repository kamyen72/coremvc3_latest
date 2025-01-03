namespace CoreMVC3.Models
{
    public class LotteryTypeSummary
    {
        public string LotteryTypeName { get; set; }
        public int LotteryTypeID { get; set; }
        public int LotteryInfoID { get; set; }
        public string DateStart { get; set; }
        public string DateEnd { get; set; }
        public decimal AllLost { get; set; }
        public decimal All4DWin { get; set; }
        public decimal TOver {  get; set; }
        public decimal Pending {  get; set; }
        public decimal Member_WL { get; set; }
        public decimal Agent_WL { get; set; }
        public decimal MA_WL { get; set; }
        public decimal SM_WL { get; set; }
        public decimal Com_WL { get; set; }
        public string Level1_ID { get; set; }
    }
}
