using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeOffManager.Data;
using TimeOffManager.Models;

namespace TimeOffManager.Validators;

public static class TimeOffRequestValidator
{
    public static async Task<IActionResult> ValidateCreateRequest(AppDbContext context, TimeOffRequest request, int userId)
    {
        // validate past dates
        if (request.StartDate < DateTime.Today || request.EndDate < DateTime.Today)
            return new BadRequestObjectResult("Cant request time off for past dates");

        // validate start date before end date
        if (request.StartDate > request.EndDate)
            return new BadRequestObjectResult("Start date must be before end date");

        // validate overlap
        bool hasOverlap = await context.TimeOffRequests
            .Where(r => r.UserId == userId &&
                       r.Status != RequestStatus.Rejected &&
                       r.Type == request.Type &&
                       (r.StartDate <= request.EndDate && r.EndDate >= request.StartDate))
            .AnyAsync();

        if (hasOverlap)
            return new BadRequestObjectResult("You already have an approved/pending request for these dates");

        return null; // no errors
    }

    public static IActionResult ValidateUpdateStatus(TimeOffRequest existingRequest, RequestStatus newStatus)
    {
        // Validate that only pending requests can be modified
        if (existingRequest.Status != RequestStatus.Pending)
        {
            return new BadRequestObjectResult("Only pending requests can be modified");
        }

        // Validate that the new status is Approve or Reject
        if (newStatus != RequestStatus.Approved && newStatus != RequestStatus.Rejected)
        {
            return new BadRequestObjectResult("Only Approved or Rejected status can be set");
        }

        return null; // No hay errores
    }
}