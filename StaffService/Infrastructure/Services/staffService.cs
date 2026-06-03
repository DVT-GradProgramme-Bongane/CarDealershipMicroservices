using Microsoft.EntityFrameworkCore;

public class StaffServiceController : IStaffService
{
    private readonly StaffDBContext _context;

    public StaffServiceController(StaffDBContext context)
    {
        _context = context;
    }

    public Task<List<StaffEntitiy>> GetAllAsync(CancellationToken token) => _context.Staff.AsNoTracking().ToListAsync(token);

    public Task<StaffEntitiy?> GetByIdAsync(Guid id, CancellationToken token) => _context.Staff.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, token);

    public async Task<StaffEntitiy> CreateAsync(CreateStaffBody request, CancellationToken token)
    {
        var staff = new StaffEntitiy(request.FirstName, request.LastName, request.Role, request.Email, request.Phone);
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync(token);
        return staff;
    }

    public async Task DeleteAsync(Guid id, CancellationToken token)
    {
        var staff = await _context.Staff.FirstOrDefaultAsync(staff => staff.Id == id, token);
        if (staff is null) return;

        _context.Staff.Remove(staff);
        await _context.SaveChangesAsync(token);
    }

    public async Task<StaffEntitiy?> UpdateAsync(Guid id, UpdateStaffBody request, CancellationToken token)
    {
        var staff = await _context.Staff.FirstOrDefaultAsync(staff => staff.Id == id, token);

        if(staff is null) return null;

        if(!string.IsNullOrWhiteSpace(request.FirstName)) staff.FirstName = request.FirstName;
        if(!string.IsNullOrWhiteSpace(request.LastName)) staff.LastName = request.LastName;
         staff.StaffRole = request.Role;

         await _context.SaveChangesAsync(token);
         return staff;
    }
}