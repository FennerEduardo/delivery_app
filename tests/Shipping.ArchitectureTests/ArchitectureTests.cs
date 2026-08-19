using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Shipping.ArchitectureTests;

public class ArchitectureTests
{
    private const string DomainNamespace = "Shipping.Domain";
    private const string ApplicationNamespace = "Shipping.Application";
    private const string InfrastructureNamespace = "Shipping.Infrastructure";
    private const string ApiNamespace = "Shipping.Api";

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOnOtherProjects()
    {
        var result = Types.InAssembly(typeof(Domain.Entities.Shipment).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Domain layer must be completely isolated without external project dependencies.");
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencyOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(typeof(Application.Commands.CreateShipmentCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Application layer must not depend on Infrastructure or Web API.");
    }

    [Fact]
    public void Controllers_ShouldInheritFromControllerBase()
    {
        var result = Types.InAssembly(typeof(Api.Controllers.ShipmentsController).Assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
            .GetResult();

        result.IsSuccessful.Should().BeTrue("All REST API controllers must inherit from ControllerBase.");
    }
}
