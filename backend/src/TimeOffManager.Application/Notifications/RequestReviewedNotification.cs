namespace TimeOffManager.Application.Notifications;

/// <summary>Integration event published when a request is approved or rejected.
/// Serialized to the queue and consumed by the email worker.</summary>
public sealed record RequestReviewedNotification(
    string RecipientEmail,
    string RecipientName,
    string RequestType,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);
