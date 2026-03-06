namespace RCA_StudyManagementSystem.Client.Utilities
{
    public static class DateTimeExtensions
    {
        public static Quarter GetQuarter(this DateTime date)
        {
            int quarter = (date.Month - 1) / 3 + 1;
            return new Quarter(date.Year, quarter);
        }

        public static Quarter GetQuarter(this DateOnly date)
        {
            int quarter = (date.Month - 1) / 3 + 1;
            return new Quarter(date.Year, quarter);
        }
    }
}
