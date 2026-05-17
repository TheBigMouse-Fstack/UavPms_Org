using System;

namespace UavPms.Core.Common;

// Dùng abstract vì class này chỉ để cho các class khác kế thừa
// Không bao giờ cho phép khởi tạo trực tiếp (new BaseEntity()).
public abstract class BaseEntity
{
    // Tự động sinh ra chuỗi Id ngẫu nhiên khi một object được tạo
    public Guid Id { get; set; } = Guid.NewGuid();
    // Lưu thời điểm (mặc định theo UTC) tạo
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }    
    
    public bool IsDeleted { get; set; } = false;
}