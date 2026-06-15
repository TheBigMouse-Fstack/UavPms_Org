using UavPms.Core.Entities;

namespace UavPms.Core.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<AppUser>
{
    // Tạm thời để trống, sau này có thể thêm hàm GetByEmailAsync
}