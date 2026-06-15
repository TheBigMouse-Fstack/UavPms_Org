using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UavPms.Core.Entities;

namespace UavPms.Infrastructure.Persistence.Configurations;

public class DetectionAlertConfiguration : IEntityTypeConfiguration<DetectionAlert>
{
    public void Configure(EntityTypeBuilder<DetectionAlert> builder)
    {
        // Cấu hình khóa ngoại để trỏ về Drone
        builder.HasOne(a => a.Drone)
            .WithMany(d => d.DetectionAlerts)
            .HasForeignKey(a => a.DroneId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Dùng 'Ma thuật' OwnsOne cho BoundingBox
        // Giúp giàn phẳng 4 thuộc tính của ValueObject thành 4 cột chung trong bảng DetectionAlert
        builder.OwnsOne(a => a.Bbox, bbox =>
        {
            bbox.Property(b => b.X).HasColumnName("Bbox_X");
            bbox.Property(b => b.Y).HasColumnName("Bbox_Y");
            bbox.Property(b => b.Width).HasColumnName("Bbox_Width");
            bbox.Property(b => b.Height).HasColumnName("Bbox_Height");
        });
    }
}