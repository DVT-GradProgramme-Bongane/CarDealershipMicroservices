public interface IStaffService
{
    Task<List<StaffEntitiy>> GetAllAsync(CancellationToken token);
    Task<StaffEntitiy?> GetByIdAsync(Guid id, CancellationToken token);
    Task<StaffEntitiy> CreateAsync(CreateStaffBody request, CancellationToken token);
    Task DeleteAsync(Guid id, CancellationToken token);

    Task<StaffEntitiy?> UpdateAsync(Guid id, UpdateStaffBody request, CancellationToken token);
}