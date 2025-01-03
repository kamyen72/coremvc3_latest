namespace CoreMVC3.Models
{
    public class OverallByBetTypeInput
    {
        public string BetType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CurrentPeriod { get; set; }
        public string UserName { get; set; }

        public string StartTime { get; set; }
        public string EndTime { get; set; }  
    }
}
