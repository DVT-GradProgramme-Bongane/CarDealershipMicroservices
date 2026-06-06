using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.StaffService.Tests.Unit.Services;

public class StaffServiceTests
{
    private readonly StaffDBContext _context;
    private readonly IStaffService _service;

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
    public async Task GetAllAsync_WhenSeeded_ReturnsAllEntities()
    {
        _context.Staff.Add(new StaffEntitiy("Zanele", "Maseko", Role.salesperson, "zanele@dealer.test", "+27 11 555 0199"));
        _context.Staff.Add(new StaffEntitiy("Johan", "Botha", Role.manager, "johan@dealer.test", "+27 11 555 0200"));
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsCorrectEntity()
    {
        var entity = new StaffEntitiy("Thabo", "Nkosi", Role.salesperson, "thabo@dealer.test", "+27 82 555 0100");
        _context.Staff.Add(entity);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(entity.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.FirstName.Should().Be("Thabo");
        result.LastName.Should().Be("Nkosi");
        result.StaffRole.Should().Be(Role.salesperson);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_PersistsAndReturnsEntity()
    {
        var request = new CreateStaffBody("Lerato", "Mokoena", Role.finance_manager, "lerato@dealer.test", "+27 11 555 0201");

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.FirstName.Should().Be("Lerato");
        result.LastName.Should().Be("Mokoena");
        result.StaffRole.Should().Be(Role.finance_manager);
        _context.Staff.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesEntity()
    {
        var entity = new StaffEntitiy("Naledi", "Dlamini", Role.mechanic, "naledi@dealer.test", "+27 11 555 0202");
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
    public async Task UpdateAsync_WhenExists_UpdatesAndReturnsEntity()
    {
        var entity = new StaffEntitiy("Pieter", "vanWyk", Role.salesperson, "pieter@dealer.test", "+27 11 555 0203");
        _context.Staff.Add(entity);
        await _context.SaveChangesAsync();

        var request = new UpdateStaffBody("Pieter", "van Wyk", Role.finance_manager, "pieter@dealer.test", "+27 11 555 0203");
        var result = await _service.UpdateAsync(entity.Id, request, CancellationToken.None);

        result.Should().NotBeNull();
        result!.LastName.Should().Be("van Wyk");
        result.StaffRole.Should().Be(Role.finance_manager);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        var request = new UpdateStaffBody("Test", "User", Role.mechanic, "test@dealer.test", "+27 11 555 0204");

        var result = await _service.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        result.Should().BeNull();
    }
}
