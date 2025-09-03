using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.Starter;

public class MentorInfo(
    string mentorId,
    string mentorName,
    string mentorEmail,
    string? mentorPhone = null,
    string? mentorPhoto = null,
    DateTime? nextMeetingDate = null,
    string? meetingUrl = null,
    int sessionsCompleted = 0,
    int sessionsTotal = 0) : ValueObject
{
    public string MentorId { get; private set; } = mentorId ?? throw new ArgumentNullException(nameof(mentorId));
    public string MentorName { get; private set; } = mentorName ?? throw new ArgumentNullException(nameof(mentorName));
    public string MentorEmail { get; private set; } = mentorEmail ?? throw new ArgumentNullException(nameof(mentorEmail));
    public string? MentorPhone { get; private set; } = mentorPhone;
    public string? MentorPhoto { get; private set; } = mentorPhoto;
    public DateTime? NextMeetingDate { get; private set; } = nextMeetingDate;
    public string? MeetingUrl { get; private set; } = meetingUrl;
    public int SessionsCompleted { get; private set; } = sessionsCompleted >= 0 ? sessionsCompleted : 0;
    public int SessionsTotal { get; private set; } = sessionsTotal >= 0 ? sessionsTotal : 0;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MentorId;
        yield return MentorName;
        yield return MentorEmail;
        yield return MentorPhone ?? string.Empty;
        yield return MentorPhoto ?? string.Empty;
        yield return NextMeetingDate ?? DateTime.MinValue;
        yield return MeetingUrl ?? string.Empty;
        yield return SessionsCompleted;
        yield return SessionsTotal;
    }
}