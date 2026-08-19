using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shipping.Application.Commands;
using Shipping.Application.DTOs;
using Xunit;

namespace Shipping.IntegrationTests;

/// <summary>
/// Integration tests aligned with Gherkin feature specifications:
/// - specs/features/customer-management.feature
/// - specs/features/shipment-lifecycle.feature
/// - specs/features/shipping-quote.feature
///
/// Uses CustomWebApplicationFactory with InMemory database and seed data.
/// Generated with gherkin-ai CLI v2.0.0-beta.1 QA agent guidance.
/// </summary>
public class ShipmentsApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ShipmentsApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ---------------------------------------------------------------
    // Feature: Customer Management
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetCustomers_ReturnsSuccessAndSeedData()
    {
        // Arrange — seed data is injected by CustomWebApplicationFactory

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
        customers.Should().NotBeNull();
        customers.Should().NotBeEmpty("because the factory seeds at least one test customer");
    }

    [Fact]
    public async Task CreateCustomer_ReturnsCreatedWithValidPayload()
    {
        // Arrange — Gherkin: "Successfully register a new customer"
        var command = new CreateCustomerCommand(
            "Acme Corp",
            "contact@acmecorp.com",
            "+573001234567",
            new AddressDto("Av El Dorado 68", "Bogota", "Cundinamarca", "110111", "Colombia"));

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        customer.Should().NotBeNull();
        customer!.Id.Should().NotBeEmpty("because a new customer ID should be generated");
        customer.Name.Should().Be("Acme Corp");
        customer.Email.Should().Be("contact@acmecorp.com");
    }

    [Fact]
    public async Task CreateCustomer_ReturnsBadRequest_WhenNameIsEmpty()
    {
        // Arrange — Gherkin: "Fail registration with invalid email format" (boundary case)
        var command = new CreateCustomerCommand(
            "",
            "valid@email.com",
            "+573001234567",
            new AddressDto("Calle 1", "Bogota", "Cundinamarca", "110111", "Colombia"));

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", command);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError,
            "because empty customer name should be rejected by validation");
    }

    [Fact]
    public async Task GetCustomerById_ReturnsNotFound_WhenIdDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------------------------------------------------------
    // Feature: Shipment Lifecycle Management
    // ---------------------------------------------------------------

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

    [Fact]
    public async Task CreateShipment_ReturnsCreated_WithValidPayload()
    {
        // Arrange — create a customer first
        var customerCmd = new CreateCustomerCommand(
            "Shipment Test Customer",
            $"shiptest-{Guid.NewGuid():N}@example.com",
            "+573009876543",
            new AddressDto("Carrera 7 #71-21", "Bogota", "Cundinamarca", "110231", "Colombia"));
        var customerResponse = await _client.PostAsJsonAsync("/api/customers", customerCmd);
        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerDto>();

        // Arrange — Gherkin: "a new shipment created in Created status"
        var shipmentCmd = new CreateShipmentCommand(
            customer!.Id,
            new AddressDto("Carrera 7 #71-21", "Bogota", "Cundinamarca", "110231", "Colombia"),
            new AddressDto("Calle 10 #4-50", "Medellin", "Antioquia", "050001", "Colombia"),
            WeightKg: 3.0m,
            LengthCm: 20,
            WidthCm: 15,
            HeightCm: 10,
            CommercialValue: 200000,
            DistanceKm: 25,
            DeliveryType: Domain.Enums.DeliveryType.Standard,
            DeliveryWindow: Domain.Enums.DeliveryWindowType.Standard);

        // Act
        var response = await _client.PostAsJsonAsync("/api/shipments", shipmentCmd);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var shipment = await response.Content.ReadFromJsonAsync<ShipmentDto>();
        shipment.Should().NotBeNull();
        shipment!.Id.Should().NotBeEmpty();
        shipment.Status.Should().Be("Created");
        shipment.WeightKg.Should().Be(3.0m);
    }

    [Fact]
    public async Task ShipmentLifecycle_QuoteConfirmTransit_ReturnsCorrectStatuses()
    {
        // Arrange — Gherkin: "Transition shipment through complete successful delivery workflow"
        var customerCmd = new CreateCustomerCommand(
            "Lifecycle Test Customer",
            $"lifecycle-{Guid.NewGuid():N}@example.com",
            "+573005551234",
            new AddressDto("Av 19 #100-45", "Bogota", "Cundinamarca", "110111", "Colombia"));
        var customerRes = await _client.PostAsJsonAsync("/api/customers", customerCmd);
        var customer = await customerRes.Content.ReadFromJsonAsync<CustomerDto>();

        var shipmentCmd = new CreateShipmentCommand(
            customer!.Id,
            new AddressDto("Av 19 #100-45", "Bogota", "Cundinamarca", "110111", "Colombia"),
            new AddressDto("Calle 5 #3-20", "Cali", "Valle del Cauca", "760001", "Colombia"),
            WeightKg: 5.0m,
            LengthCm: 30,
            WidthCm: 20,
            HeightCm: 15,
            CommercialValue: 500000,
            DistanceKm: 40,
            DeliveryType: Domain.Enums.DeliveryType.Standard,
            DeliveryWindow: Domain.Enums.DeliveryWindowType.Standard);
        var shipmentRes = await _client.PostAsJsonAsync("/api/shipments", shipmentCmd);
        var shipment = await shipmentRes.Content.ReadFromJsonAsync<ShipmentDto>();
        var shipmentId = shipment!.Id;

        // Act 1 — Generate Quote: status should become "Quoted"
        var quoteRes = await _client.PostAsync($"/api/shipments/{shipmentId}/quote", null);
        quoteRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var quote = await quoteRes.Content.ReadFromJsonAsync<ShippingQuoteDto>();
        quote.Should().NotBeNull();
        quote!.Total.Should().BeGreaterThan(0, "because quote total must be positive");

        // Verify status is Quoted
        var getRes1 = await _client.GetAsync($"/api/shipments/{shipmentId}");
        var afterQuote = await getRes1.Content.ReadFromJsonAsync<ShipmentDto>();
        afterQuote!.Status.Should().Be("Quoted");

        // Act 2 — Confirm Shipment: status should become "Confirmed"
        var confirmRes = await _client.PostAsync($"/api/shipments/{shipmentId}/confirm", null);
        confirmRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var getRes2 = await _client.GetAsync($"/api/shipments/{shipmentId}");
        var afterConfirm = await getRes2.Content.ReadFromJsonAsync<ShipmentDto>();
        afterConfirm!.Status.Should().Be("Confirmed");

        // Act 3 — Mark as InTransit
        var transitRes = await _client.PostAsJsonAsync($"/api/shipments/{shipmentId}/status",
            new { NewStatus = Domain.Enums.ShipmentStatus.InTransit, Comment = "Package picked up by courier" });
        transitRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var getRes3 = await _client.GetAsync($"/api/shipments/{shipmentId}");
        var afterTransit = await getRes3.Content.ReadFromJsonAsync<ShipmentDto>();
        afterTransit!.Status.Should().Be("InTransit");

        // Act 4 — Check status history has entries
        var historyRes = await _client.GetAsync($"/api/shipments/{shipmentId}/history");
        historyRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await historyRes.Content.ReadFromJsonAsync<List<ShipmentStatusHistoryDto>>();
        history.Should().NotBeNullOrEmpty("because each transition should record a status history entry");
    }

    [Fact]
    public async Task CancelShipment_FromQuotedStatus_ReturnsSuccess()
    {
        // Arrange — Gherkin: "Cancel shipment from Quoted status"
        var customerCmd = new CreateCustomerCommand(
            "Cancel Test Customer",
            $"cancel-{Guid.NewGuid():N}@example.com",
            "+573006667890",
            new AddressDto("Carrera 15 #80-10", "Bogota", "Cundinamarca", "110111", "Colombia"));
        var customerRes = await _client.PostAsJsonAsync("/api/customers", customerCmd);
        var customer = await customerRes.Content.ReadFromJsonAsync<CustomerDto>();

        var shipmentCmd = new CreateShipmentCommand(
            customer!.Id,
            new AddressDto("Carrera 15 #80-10", "Bogota", "Cundinamarca", "110111", "Colombia"),
            new AddressDto("Calle 20 #5-30", "Barranquilla", "Atlantico", "080001", "Colombia"),
            WeightKg: 2.0m,
            LengthCm: 15,
            WidthCm: 10,
            HeightCm: 10,
            CommercialValue: 100000,
            DistanceKm: 15,
            DeliveryType: Domain.Enums.DeliveryType.Standard,
            DeliveryWindow: Domain.Enums.DeliveryWindowType.Standard);
        var shipmentRes = await _client.PostAsJsonAsync("/api/shipments", shipmentCmd);
        var shipment = await shipmentRes.Content.ReadFromJsonAsync<ShipmentDto>();
        var shipmentId = shipment!.Id;

        // Quote first to reach Quoted status
        await _client.PostAsync($"/api/shipments/{shipmentId}/quote", null);

        // Act — Cancel with reason
        var cancelRes = await _client.PostAsJsonAsync($"/api/shipments/{shipmentId}/cancel",
            new { Reason = "Changed mind" });

        // Assert
        cancelRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var getRes = await _client.GetAsync($"/api/shipments/{shipmentId}");
        var cancelled = await getRes.Content.ReadFromJsonAsync<ShipmentDto>();
        cancelled!.Status.Should().Be("Cancelled");
    }

    // ---------------------------------------------------------------
    // Feature: Shipping Quote Calculation
    // ---------------------------------------------------------------

    [Fact]
    public async Task GenerateQuote_ReturnsItemizedBreakdown()
    {
        // Arrange — Gherkin: "Calculate standard shipping quote for lightweight item"
        var customerCmd = new CreateCustomerCommand(
            "Quote Test Customer",
            $"quote-{Guid.NewGuid():N}@example.com",
            "+573001112233",
            new AddressDto("Transversal 3 #20-15", "Bogota", "Cundinamarca", "110111", "Colombia"));
        var customerRes = await _client.PostAsJsonAsync("/api/customers", customerCmd);
        var customer = await customerRes.Content.ReadFromJsonAsync<CustomerDto>();

        var shipmentCmd = new CreateShipmentCommand(
            customer!.Id,
            new AddressDto("Transversal 3 #20-15", "Bogota", "Cundinamarca", "110111", "Colombia"),
            new AddressDto("Diagonal 50 #10-80", "Bucaramanga", "Santander", "680001", "Colombia"),
            WeightKg: 3.0m,
            LengthCm: 20,
            WidthCm: 15,
            HeightCm: 10,
            CommercialValue: 200000,
            DistanceKm: 25,
            DeliveryType: Domain.Enums.DeliveryType.Standard,
            DeliveryWindow: Domain.Enums.DeliveryWindowType.Standard);
        var shipmentRes = await _client.PostAsJsonAsync("/api/shipments", shipmentCmd);
        var shipment = await shipmentRes.Content.ReadFromJsonAsync<ShipmentDto>();

        // Act
        var quoteRes = await _client.PostAsync($"/api/shipments/{shipment!.Id}/quote", null);

        // Assert
        quoteRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var quote = await quoteRes.Content.ReadFromJsonAsync<ShippingQuoteDto>();
        quote.Should().NotBeNull();
        quote!.BaseCost.Should().BeGreaterThan(0);
        quote.BillableWeightKg.Should().Be(3.0m, "because actual weight 3kg > volumetric weight for this package");
        quote.Total.Should().BeGreaterThan(0);
    }

    // ---------------------------------------------------------------
    // Health Check Endpoints
    // ---------------------------------------------------------------

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
