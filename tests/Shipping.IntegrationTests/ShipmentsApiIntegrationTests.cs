using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shipping.Application.DTOs;
using Xunit;

namespace Shipping.IntegrationTests;

public class ShipmentsApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ShipmentsApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_ReturnsSuccessAndSeedData()
    {
        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
        customers.Should().NotBeNull();
        customers.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetShipments_ReturnsSuccessAndList()
    {
        // Act
        var response = await _client.GetAsync("/api/shipments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var shipments = await response.Content.ReadFromJsonAsync<List<ShipmentDto>>();
        shipments.Should().NotBeNull();
    }
}
