using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UavPms.Core.Entities;

namespace UavPms.Infrastructure.Persistence.Configurations;

public class TelemetryConfiguration : IEntityTypeConfiguration<DroneTelemetry>
{
    public void Configure(EntityTypeBuilder<DroneTelemetry> builder)
    {
        // Cấu hình khóa ngoại DroneId
        // Tọa độ này có 1 drone, drone lại có nhiều tọa độ
        // Bảng DroneTelemetry này có FK là DroneId  
        builder.HasOne(t => t.Drone)
            .WithMany(d => d.Telemetries)
            .HasForeignKey(t => t.DroneId)
            .OnDelete(DeleteBehavior.Cascade); // Nếu drone bị xóa, thì toàn bộ tọa độ của nó cũng sẽ bị xóa
    }
}