using UavPms.Core.Common;
using UavPms.Core.Enums;
using UavPms.Core.ValueObjects;

namespace UavPms.Core.Entities;

public class DetectionAlert : BaseEntity
{
    public Guid DroneId { get; set; }
    
    public AlertClassType AlertClass { get; set; }
    public double Confidence { get; set; }
    public DateTime Timestamp { get; set; } =  DateTime.UtcNow;
    
    // Tích hợp Value Object BoundingBox vào đây
    // Dấu ? có nghĩa là lúc bay bình thường không phát hiện gì có thể báo là Null
    public BoundingBox? Bbox { get; set; }
    
    public bool IsValidated { get; set; }
    
    // Trỏ về máy bay phát hiện ra lỗi đang gặp 
    public virtual Drone? Drone { get; set; }
    
    // Navigation Property: MỘT cảnh báo chỉ đi kèm mới MỘT hình ảnh bằng chứng (Quan hệ 1-1)
    public virtual EvidenceImage? EvidenceImage { get; set; }
}