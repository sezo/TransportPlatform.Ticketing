using NetArchTest.Rules;
using NUnit.Framework;
using Shouldly;

namespace TransportPlatform.Ticketing.ArchitectureTests;

[TestFixture]
public class MicroserviceDecouplingTests
{
    [Test]
    public void TicketingService_ShouldNotDependOnAccounting()
    {
        // ── ARRANGE & ACT ──────────────────────────────────────────────────
        var ticketingTypes = Types.InNamespace("TransportPlatform.Ticketing*");
        
        var result = ticketingTypes
            .Should()
            .NotHaveDependencyOn("TransportPlatform.Accounting*")
            .GetResult();

        // ── ASSERT ─────────────────────────────────────────────────────────
        result.IsSuccessful.ShouldBeTrue(
            $"Ticketing service should not depend on Accounting. " +
            $"Found references: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Test]
    public void TicketingService_ShouldNotDependOnVehicleService()
    {
        // ── ARRANGE & ACT ──────────────────────────────────────────────────
        var ticketingTypes = Types.InNamespace("TransportPlatform.Ticketing*");
        
        var result = ticketingTypes
            .Should()
            .NotHaveDependencyOn("TransportPlatform.Vehicles*")
            .GetResult();

        // ── ASSERT ─────────────────────────────────────────────────────────
        result.IsSuccessful.ShouldBeTrue(
            $"Ticketing service should be loosely coupled from Vehicles. " +
            $"Found direct references: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }
}
