using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Schedulas.Application.Common.Interfaces;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Schedulas.API.Tests;

public class ApiBoundaryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiBoundaryTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void ApplicationStartup_IsValid()
    {
        // Act & Assert
        // If the DI container is invalid, CreateClient() will throw an exception during startup
        var client = _factory.CreateClient();
        client.Should().NotBeNull();
    }

    [Fact]
    public async Task UnauthorizedUser_Receives401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        // Accessing an endpoint that requires authorization
        // Replace "/api/v1/departments" with a known protected route.
        var response = await client.GetAsync("/api/v1/institutions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Additional tests for 403 Forbidden, Multi-tenancy, and 400 Validation
    // would require creating a mock JWT token generator and sending authenticated requests
    // with different tenant IDs and roles. We are keeping it lightweight for the boundary verification.
}
