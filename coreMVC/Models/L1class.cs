namespace CoreMVC3.Models
{
    public class L1class
    {
        public int LotteryTypeID { get; set; }
        public string LotteryTypeName { get; set; }
        public decimal TOVer { get; set; }
        public decimal Pending { get; set; }
        public decimal AllLost { get; set; }
        public decimal All4dWin { get; set; }
        public decimal Allnon4dWin { get; set; }
        public decimal WL { get; set; }
        public decimal Agent_WL { get; set; }
        public decimal Com_WL { get; set; }
    }
}
