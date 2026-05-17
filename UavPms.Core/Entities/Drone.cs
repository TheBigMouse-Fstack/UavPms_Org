using System.Collections.Generic;
using UavPms.Core.Common;
using UavPms.Core.Enums;

namespace UavPms.Core.Entities;

public class Drone : BaseEntity
{
    public string DeviceCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DroneStatus Status { get; set; } = DroneStatus.Idle;
    public double BatteryPercentage { get; set; }
    
    // Navigation Property: 1 Drone sẽ có một danh sách (ICollection) nhiều mốc tọa độ (quan hệ 1-Nhiều)
    // Từ khóa 'virtual' giúp EF Core hỗ trợ tính năng Lazy Loading sau này (nếu cần)
    public virtual ICollection<DroneTelemetry> Telemetries { get; set; } = new List<DroneTelemetry>();
    
    // Tương tự, 1 Drone cũng có thể bắn ra nhiều Alert
    public virtual ICollection<DetectionAlert> DetectionAlerts { get; set; } = new List<DetectionAlert>();
}