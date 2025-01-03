namespace CoreMVC3.Models
{
    public class OverallByUserNameInput
    {
        public string BetType { get; set; }
        public string UserName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public string CurrentPeriod { get; set; }
        public string Level2_ID { get; set; }
    }
}
