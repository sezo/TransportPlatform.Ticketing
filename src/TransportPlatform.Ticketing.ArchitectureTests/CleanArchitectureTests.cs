using NetArchTest.Rules;
using NUnit.Framework;
using Shouldly;

namespace TransportPlatform.Ticketing.ArchitectureTests;

[TestFixture]
public class CleanArchitectureTests
{
    [Test]
    public void DomainLayer_ShouldNotDependOnInfrastructure()
    {
        // ── ARRANGE & ACT ──────────────────────────────────────────────────
        var domainTypes = Types.InNamespace("Ticketing.Domain");

        var result = domainTypes
            .Should()
            .NotHaveDependencyOn("TransportPlatform.Ticketing.Infrastructure")
            .GetResult();

        // ── ASSERT ─────────────────────────────────────────────────────────
        result.IsSuccessful.ShouldBeTrue(
            "Domain layer must not depend on Infrastructure. " +
            $"Violating types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Test]
    public void DomainLayer_ShouldNotDependOnApplication()
    {
        // ── ARRANGE & ACT ──────────────────────────────────────────────────
        var domainTypes = Types.InNamespace("Ticketing.Domain");

        var result = domainTypes
            .Should()
            .NotHaveDependencyOn("Ticketing.Application")
            .GetResult();

        // ── ASSERT ─────────────────────────────────────────────────────────
        result.IsSuccessful.ShouldBeTrue(
            "Domain layer must not depend on Application. " +
            $"Violating types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Test]
    public void ApplicationLayer_ShouldDependOnDomain()
    {
        // ── ARRANGE & ACT ──────────────────────────────────────────────────
        var applicationTypes = Types.InNamespace("Ticketing.Application*");

        var result = applicationTypes
            .Should()
            .HaveDependencyOn("Ticketing.Domain")
            .GetResult();

        // ── ASSERT ─────────────────────────────────────────────────────────
        result.IsSuccessful.ShouldBeTrue(
            "Application layer should depend on Domain. " +
            $"Violating types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }

    [Test]
    public void InfrastructureLayer_ShouldDependOnDomain()
    {
        // ── ARRANGE & ACT ──────────────────────────────────────────────────
        var infrastructureTypes = Types.InNamespace("TransportPlatform.Ticketing.Infrastructure*");

        var result = infrastructureTypes
            .Should()
            .HaveDependencyOn("Ticketing.Domain")
            .GetResult();

        // ── ASSERT ─────────────────────────────────────────────────────────
        result.IsSuccessful.ShouldBeTrue(
            "Infrastructure layer should depend on Domain interfaces. " +
            $"Violating types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}");
    }
}
