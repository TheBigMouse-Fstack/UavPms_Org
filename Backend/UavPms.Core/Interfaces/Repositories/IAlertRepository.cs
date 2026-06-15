using UavPms.Core.Entities;
using UavPms.Core.Enums;

namespace UavPms.Core.Interfaces.Repositories;

public interface IAlertRepository : IGenericRepository<DetectionAlert>
{
    // Hàm truy vấn lịch sử cảnh báo phức tạp dùng cho Web Dashboard
    Task<IReadOnlyList<DetectionAlert>> GetAlertHistoryAsync(
        AlertClassType? type,
        double minConfidence,
        DateTime from,
        DateTime to);
}