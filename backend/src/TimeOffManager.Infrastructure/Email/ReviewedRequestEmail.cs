using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Application.Notifications;

namespace TimeOffManager.Infrastructure.Email;

/// <summary>Builds the email for a reviewed-request notification. Shared by the
/// RabbitMQ and Azure Service Bus consumers so the template lives in one place.</summary>
public static class ReviewedRequestEmail
{
    public static EmailMessage From(RequestReviewedNotification n)
    {
        var subject = $"Your {n.RequestType.ToLowerInvariant()} request was {n.Status.ToLowerInvariant()}";
        var body =
            $"Hi {n.RecipientName},\n\n" +
            $"Your {n.RequestType.ToLowerInvariant()} request from {n.StartDate:yyyy-MM-dd} to {n.EndDate:yyyy-MM-dd} " +
            $"has been {n.Status.ToLowerInvariant()}.\n\n" +
            "— TimeOff Manager";
        return new EmailMessage(n.RecipientEmail, subject, body);
    }
}
