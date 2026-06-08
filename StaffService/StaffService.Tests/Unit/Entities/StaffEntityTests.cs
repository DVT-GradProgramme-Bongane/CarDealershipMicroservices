using FluentAssertions;

namespace CarDealership.StaffService.Tests.Unit.Entities;

public class StaffEntityTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        var entity = new StaffEntitiy("Zanele", "Maseko", Role.salesperson, "zanele@dealer.test", "+27 11 555 0199");

        entity.FirstName.Should().Be("Zanele");
        entity.LastName.Should().Be("Maseko");
        entity.StaffRole.Should().Be(Role.salesperson);
        entity.Email.Should().Be("zanele@dealer.test");
        entity.Phone.Should().Be("+27 11 555 0199");
    }

    [Fact]
    public void Constructor_GeneratesNonEmptyId()
    {
        var entity = new StaffEntitiy("Zanele", "Maseko", Role.salesperson, "zanele@dealer.test", "+27 11 555 0199");

        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_SetsCreatedAt_ToUtcNow()
    {
        var before = DateTime.UtcNow;
        var entity = new StaffEntitiy("Zanele", "Maseko", Role.salesperson, "zanele@dealer.test", "+27 11 555 0199");
        var after = DateTime.UtcNow;

        entity.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Constructor_EachInstance_HasUniqueId()
    {
        var a = new StaffEntitiy("Zanele", "Maseko", Role.salesperson, "a@dealer.test", "+27 11 555 0001");
        var b = new StaffEntitiy("Johan", "Botha", Role.manager, "b@dealer.test", "+27 11 555 0002");

        a.Id.Should().NotBe(b.Id);
    }
}
