using AppointmentScheduler.Domain.Exceptions;

namespace AppointmentScheduler.Domain.Appointments;

/// <summary>
/// Value object representing a half-open interval of time ([Start, End)).
/// Immutable, and equal to any other TimeSlot with the same Start and End.
/// This is where "do these two ranges overlap" logic lives - it belongs to
/// the concept of a time range, not to whatever service happens to compare
/// two appointments.
/// </summary>
public sealed record TimeSlot
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public TimeSlot(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new DomainException("A time slot's end must be after its start.");

        Start = start;
        End = end;
    }

    public TimeSpan Duration => End - Start;

    /// <summary>
    /// Two slots overlap when one starts before the other ends, in both
    /// directions. Touching slots (one ends exactly when the other starts)
    /// do not count as overlapping.
    /// </summary>
    public bool Overlaps(TimeSlot other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Start < other.End && other.Start < End;
    }

    public bool IsInPast(DateTime asOfUtc) => Start < asOfUtc;

    public override string ToString() => $"{Start:u} - {End:u}";
}
