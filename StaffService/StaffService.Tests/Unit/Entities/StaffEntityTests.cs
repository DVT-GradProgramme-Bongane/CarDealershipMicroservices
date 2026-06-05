using FluentAssertions;

namespace CarDealership.StaffTests.Unit.Entities;

public class StaffEntityTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        var entity = new StaffEntitiy("Alice", "Smith", Role.salesperson, "alice@dealer.com", "0821234567");

        entity.FirstName.Should().Be("Alice");
        entity.LastName.Should().Be("Smith");
        entity.StaffRole.Should().Be(Role.salesperson);
        entity.Email.Should().Be("alice@dealer.com");
        entity.Phone.Should().Be("0821234567");
    }

    [Fact]
    public void Constructor_GeneratesNonEmptyId()
    {
        var entity = new StaffEntitiy("Bob", "Jones", Role.mechanic, "bob@dealer.com", "0837654321");

        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_GeneratesUniqueIdPerInstance()
    {
        var a = new StaffEntitiy("A", "B", Role.manager, "a@b.com", "0800000001");
        var b = new StaffEntitiy("C", "D", Role.manager, "c@d.com", "0800000002");

        a.Id.Should().NotBe(b.Id);
    }

    [Fact]
    public void Constructor_SetsCreatedAt_ToUtcNow()
    {
        var before = DateTime.UtcNow;
        var entity = new StaffEntitiy("Carol", "White", Role.finance_manager, "carol@dealer.com", "0841111111");
        var after = DateTime.UtcNow;

        entity.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }
}
