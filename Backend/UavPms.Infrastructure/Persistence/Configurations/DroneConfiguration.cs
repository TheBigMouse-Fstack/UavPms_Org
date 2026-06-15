using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UavPms.Core.Entities;

namespace UavPms.Infrastructure.Persistence.Configurations;

public class DroneConfiguration : IEntityTypeConfiguration<Drone>
{
    public void Configure(EntityTypeBuilder<Drone> builder)
    {
        // Cấu hình Index: Đảm bảo mã thiết bị (DeviceCode) không bao giờ bị khai báo trùng lặp
        builder.HasIndex(d => d.DeviceCode).IsUnique();
        
        // Thêm các ràng buộc độ dài cột trong DB để tối ưu dung lượng  
        builder.Property(d => d.DeviceCode).HasMaxLength(50).IsRequired();
        builder.Property(d => d.Name).HasMaxLength(50).IsRequired();
    }
}