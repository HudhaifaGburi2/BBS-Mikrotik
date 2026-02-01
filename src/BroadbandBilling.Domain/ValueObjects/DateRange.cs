namespace BroadbandBilling.Domain.ValueObjects;

public sealed class DateRange : IEquatable<DateRange>
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    private DateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date");

        StartDate = startDate;
        EndDate = endDate;
    }

    public static DateRange Create(DateTime startDate, DateTime endDate)
    {
        return new DateRange(startDate, endDate);
    }

    public static DateRange CreateMonthly(DateTime startDate)
    {
        return new DateRange(startDate, startDate.AddMonths(1).AddDays(-1));
    }

    public static DateRange CreateYearly(DateTime startDate)
    {
        return new DateRange(startDate, startDate.AddYears(1).AddDays(-1));
    }

    public int DurationInDays => (EndDate - StartDate).Days + 1;

    public bool IsActive(DateTime date)
    {
        return date >= StartDate && date <= EndDate;
    }

    public bool IsExpired(DateTime currentDate)
    {
        return currentDate > EndDate;
    }

    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    public bool Equals(DateRange? other)
    {
        if (other is null) return false;
        return StartDate == other.StartDate && EndDate == other.EndDate;
    }

    public override bool Equals(object? obj)
    {
        return obj is DateRange range && Equals(range);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StartDate, EndDate);
    }

    public override string ToString()
    {
        return $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
    }
}
