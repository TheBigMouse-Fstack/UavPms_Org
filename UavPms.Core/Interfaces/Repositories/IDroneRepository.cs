using UavPms.Core.Entities;

namespace UavPms.Core.Interfaces.Repositories;

public interface IDroneRepository : IGenericRepository<Drone>
{
    // Ngoài các hàm CRUD cơ bản, Drone cần thêm hàm lấy theo mã thiết bị
    Task<Drone> GetByDeviceCodeAsync(string code);
}