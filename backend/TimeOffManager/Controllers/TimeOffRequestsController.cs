using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeOffManager.Data;
using TimeOffManager.Models;
using System.Security.Claims;
using TimeOffManager.DTOs;
using TimeOffManager.Validators;

namespace TimeOffManager.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TimeOffRequestsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public TimeOffRequestsController(AppDbContext context, DbContextOptions<AppDbContext> dbContextOptions)
    {
        _context = context;
        _dbContextOptions = dbContextOptions;
    }

    // Employee Endpoints can see only their own requests
    [HttpGet("my")]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var requests = await _context.TimeOffRequests
            .Where(r => r.UserId == userId)
            .Select(r => new TimeOffRequestDto
            {
                Id = r.Id,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Reason = r.Reason,
                Type = r.Type.ToString(),
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
                // User dont need to be included here, as it's the same user making the request

            })
            .ToListAsync();

        return Ok(requests);
    }

    // Employee Endpoints can create a new request
    [HttpPost]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> Create(TimeOffRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");


            var validationResult = await TimeOffRequestValidator.ValidateCreateRequest(_context, request, userId);
            if (validationResult != null)
            {
                return validationResult;
            }
            request.UserId = userId;
            request.Status = RequestStatus.Pending;
            request.CreatedAt = DateTime.UtcNow;

            _context.TimeOffRequests.Add(request);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync(); // Confirmar explícitamente

            // Verificación inmediata
            var savedRequest = await _context.TimeOffRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == request.Id);

            if (savedRequest == null)
            {
                throw new Exception("Failed to save the request.");
            }

            return Ok(savedRequest);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "Error creating request");
        }
    }

    // Admin Endpoints can see all requests
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var rawCount = await _context.TimeOffRequests.CountAsync();
        Console.WriteLine($"Total requests in DB: {rawCount}");
        var requests = await _context.TimeOffRequests
           .AsNoTracking() // Better performance for read-only queries
           .Include(r => r.User)
           .OrderByDescending(r => r.CreatedAt) // Order by creation date
           .Select(r => new TimeOffRequestDto
           {
               Id = r.Id,
               StartDate = r.StartDate,
               EndDate = r.EndDate,
               Reason = r.Reason,
               Type = r.Type.ToString(),
               Status = r.Status.ToString(),
               CreatedAt = r.CreatedAt,
               User = r.User != null ? new UserBasicDto
               {
                   Id = r.User.Id,
                   Email = r.User.Email,
                   Role = r.User.Role.ToString()
               } : null
           })
           .ToListAsync();

        return Ok(requests);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var timeOffRequest = await _context.TimeOffRequests.FindAsync(id);
            if (timeOffRequest == null)
                return NotFound();

            // Validación
            var validationResult = TimeOffRequestValidator.ValidateUpdateStatus(timeOffRequest, request.Status);
            if (validationResult != null)
            {
                return validationResult;
            }

            timeOffRequest.Status = request.Status;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(timeOffRequest);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "Error updating request status");
        }
    }

    // Añade esta clase en el mismo archivo o en tus DTOs
    public class UpdateStatusRequest
    {
        public RequestStatus Status { get; set; }
    }
}
