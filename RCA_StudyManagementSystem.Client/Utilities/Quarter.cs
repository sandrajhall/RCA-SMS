namespace RCA_StudyManagementSystem.Client.Utilities
{
    public readonly struct Quarter
    {
        public int Year { get; }
        public int QuarterNumber { get; }

        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public Quarter(int year, int quarter)
        {
            if (quarter < 1 || quarter > 4)
                throw new ArgumentOutOfRangeException(nameof(quarter), "Quarter must be between 1 and 4.");

            Year = year;
            QuarterNumber = quarter;

            int startMonth = (quarter - 1) * 3 + 1;
            StartDate = new DateTime(year, startMonth, 1);
            EndDate = StartDate.AddMonths(3).AddDays(-1);
        }
    }
}
