namespace CoreMVC3.Models
{
    public class BetTrans
    {
        public int ID { get; set; }
        public string Bet_Type { get; set; }
        public int LotteryTypeID { get; set; }
        public int DrawTypeID { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Username { get; set; }
        public string Bill_No_Ticket { get; set; } // CurrentPeriod
        public Decimal? TOver { get; set; }
        public Decimal? Capital { get; set; }
        public Decimal Pending { get; set; }
        public DateTime Openning_Time { get; set; } // ShowResultDate
        public DateTime BetTime { get; set; } // CreateDate
        public string Bet_1 { get; set; } // FamliyBigID from LotteryInfo
        public string Bet_2 { get; set; } // LotteryInfoName
        public string Bet_3 { get; set; } // SelectedNums
        public Decimal BetAmount { get; set; } // DiscountPrice
        public Decimal WinMoney { get; set; } // DiscountPrice
        public bool IsWin { get; set; }
        public Decimal Sum_winlose_AllLOST { get; set; }
        public Decimal Sum_winlose_AllWIN_4d { get; set; }
        public Decimal Sum_winlose_AllWIN_NOT4d { get; set; }
        public Decimal WL { get; set; }
        public Decimal Lost { get; set; }
        public Decimal Win { get; set; }
        public Decimal Win_WO_Capital { get; set; }
        public string LotteryTypeName { get; set; }
        public int GameDealerMemberID { get; set; }
    }
}
