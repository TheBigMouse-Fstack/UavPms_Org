using System.Security.Policy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UavPms.Core.Entities;

namespace UavPms.Infrastructure.Persistence.Configurations;

public class EvidenceImageConfiguration : IEntityTypeConfiguration<EvidenceImage>
{
    public void Configure(EntityTypeBuilder<EvidenceImage> builder)
    {
        // Giới hạn tối đa đường dẫn 255 ký tự
        builder.Property(e => e.ImagePath).HasMaxLength(255).IsRequired();
        
        // Cấu hình quan hệ 1-1: 1 Cảnh báo chỉ có 1 ảnh bằng chứng 
        builder.HasOne(e => e.DetectionAlert)
            .WithOne(a => a.EvidenceImage)
            .HasForeignKey<EvidenceImage>(e => e.DetectionAlertId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}