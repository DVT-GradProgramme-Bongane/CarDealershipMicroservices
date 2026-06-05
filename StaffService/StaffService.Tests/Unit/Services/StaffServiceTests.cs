using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.StaffTests.Unit.Services;

public class StaffServiceTests
{
    private readonly StaffDBContext _context;
    private readonly StaffServiceController _service;

    public StaffServiceTests()
    {
        var options = new DbContextOptionsBuilder<StaffDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new StaffDBContext(options);
        _service = new StaffServiceController(_context);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
    {
        var result = await _service.GetAllAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenSeeded_ReturnsAllStaff()
    {
        _context.Staff.Add(new StaffEntitiy("Alice", "Smith", Role.salesperson, "alice@dealer.com", "0821234567"));
        _context.Staff.Add(new StaffEntitiy("Bob", "Jones", Role.mechanic, "bob@dealer.com", "0837654321"));
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsCorrectStaff()
    {
        var entity = new StaffEntitiy("Carol", "White", Role.manager, "carol@dealer.com", "0841111111");
        _context.Staff.Add(entity);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(entity.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.FirstName.Should().Be("Carol");
        result.LastName.Should().Be("White");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_PersistsAndReturnsStaff()
    {
        var request = new CreateStaffBody("Dave", "Brown", Role.finance_manager, "dave@dealer.com", "0852222222");

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.FirstName.Should().Be("Dave");
        result.StaffRole.Should().Be(Role.finance_manager);
        _context.Staff.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesStaff()
    {
        var entity = new StaffEntitiy("Eve", "Davis", Role.salesperson, "eve@dealer.com", "0863333333");
        _context.Staff.Add(entity);
        await _context.SaveChangesAsync();

        await _service.DeleteAsync(entity.Id, CancellationToken.None);

        _context.Staff.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_DoesNotThrow()
    {
        var act = async () => await _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesFirstAndLastName()
    {
        var entity = new StaffEntitiy("Frank", "Green", Role.mechanic, "frank@dealer.com", "0874444444");
        _context.Staff.Add(entity);
        await _context.SaveChangesAsync();
        var request = new UpdateStaffBody("Franklin", "Greene", Role.manager, "frank@dealer.com", "0874444444");

        var result = await _service.UpdateAsync(entity.Id, request, CancellationToken.None);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Franklin");
        result.LastName.Should().Be("Greene");
        result.StaffRole.Should().Be(Role.manager);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        var request = new UpdateStaffBody("X", "Y", Role.salesperson, "x@y.com", "0800000000");

        var result = await _service.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        result.Should().BeNull();
    }

    // ── Appended by scaffold agent ────────────────────────────────
    // Covers: UpdateAsync whitespace-guard behaviour; CreateAsync contact field persistence
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WhenExists_WithEmptyFirstName_KeepsOriginalFirstName()
    {
        var entity = new StaffEntitiy("Grace", "Hall", Role.salesperson, "grace@dealer.com", "0880000001");
        _context.Staff.Add(entity);
        await _context.SaveChangesAsync();

        var request = new UpdateStaffBody("", "Hall", Role.salesperson, "grace@dealer.com", "0880000001");
        var result = await _service.UpdateAsync(entity.Id, request, CancellationToken.None);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Grace");
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_WithEmptyLastName_KeepsOriginalLastName()
    {
        var entity = new StaffEntitiy("Henry", "Irving", Role.mechanic, "henry@dealer.com", "0880000002");
        _context.Staff.Add(entity);
        await _context.SaveChangesAsync();

        var request = new UpdateStaffBody("Henry", "", Role.mechanic, "henry@dealer.com", "0880000002");
        var result = await _service.UpdateAsync(entity.Id, request, CancellationToken.None);

        result.Should().NotBeNull();
        result!.LastName.Should().Be("Irving");
    }

    [Fact]
    public async Task CreateAsync_WithValidData_PersistsEmailAndPhone()
    {
        var request = new CreateStaffBody("Iris", "James", Role.finance_manager, "iris@dealer.com", "0890000003");

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Email.Should().Be("iris@dealer.com");
        result.Phone.Should().Be("0890000003");
    }
}
