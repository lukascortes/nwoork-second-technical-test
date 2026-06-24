using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TimeOffManager.Api.IntegrationTests;

public class BackendSmokeTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private const string AdminEmail = "admin@timeoff.dev";
    private const string AdminPassword = "Admin123!";
    private const string EmployeeEmail = "emma@timeoff.dev";
    private const string EmployeePassword = "Employee123!";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public BackendSmokeTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUsers_Anonymous_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/users");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // S1 closed
    }

    [Fact]
    public async Task Login_AsAdmin_ReturnsTokenAndRoleAsString()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = AdminEmail, password = AdminPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("role").GetString().Should().Be("Admin"); // enum serialized as string
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = AdminEmail, password = "totally-wrong" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsSeededUsers()
    {
        var client = await AuthenticatedClientAsync(AdminEmail, AdminPassword);

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<JsonElement>();
        users.GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task GetUsers_AsEmployee_ReturnsForbidden()
    {
        var client = await AuthenticatedClientAsync(EmployeeEmail, EmployeePassword);

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden); // role enforcement
    }

    [Fact]
    public async Task Register_CreatesEmployee_RegardlessOfInput()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { email = "newbie@corp.com", password = "Passw0rd", fullName = "New Bie" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("role").GetString().Should().Be("Employee"); // S3: always employee
    }

    [Fact]
    public async Task Employee_CanCreateAndListOwnRequest()
    {
        var client = await AuthenticatedClientAsync(EmployeeEmail, EmployeePassword);
        var start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(90);

        var create = await client.PostAsJsonAsync("/api/timeoffrequests", new
        {
            startDate = start.ToString("yyyy-MM-dd"),
            endDate = start.AddDays(2).ToString("yyyy-MM-dd"),
            type = "Vacation",
            reason = "Conference"
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);

        var mine = await client.GetAsync("/api/timeoffrequests/me");
        mine.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await mine.Content.ReadFromJsonAsync<JsonElement>();
        list.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Employee_CanReadVacationBalance()
    {
        var client = await AuthenticatedClientAsync(EmployeeEmail, EmployeePassword);

        var response = await client.GetAsync("/api/timeoffrequests/balance");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("annualAllowance").GetInt32().Should().Be(20); // Emma is seeded with 20
        body.GetProperty("usedDays").GetInt32().Should().Be(0);         // no approved vacation in seed
        body.GetProperty("remainingDays").GetInt32().Should().Be(20);
    }

    private async Task<HttpClient> AuthenticatedClientAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.GetProperty("accessToken").GetString();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
