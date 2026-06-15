using System;
using UavPms.Core.Common;

namespace UavPms.Core.Entities;

public class DroneTelemetry : BaseEntity
{
    // Khóa ngoại để biết tọa độ của máy bay nào
    public Guid DroneId { get; set; }
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation Property: Sợi dây trỏ ngược về Object Drone mẹ (Quan hệ nhiều-1)
    public virtual Drone? Drone { get; set; }
}