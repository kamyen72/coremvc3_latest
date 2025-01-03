namespace CoreMVC3.Models
{
    public class DateRange
    {
        ///<summary>
        /// Gets or sets Name.
        ///</summary>
        public string StartDate { get; set; }

        ///<summary>
        /// Gets or sets DateTime.
        ///</summary>
        public string EndDate { get; set; }

        public string CurrentPeriod { get; set; }
        public string UserName { get; set; }
        public string BetType { get; set; }

        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
