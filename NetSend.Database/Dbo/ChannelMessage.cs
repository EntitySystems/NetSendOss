using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetSend.Database.Types;
using NetSend.Shared.Static;

namespace NetSend.Database.Dbo;

public class ChannelMessage
{
    public Guid Id { get; set; }
    public required Guid? UserId { get; set; }
    public bool IsSystem { get; set; }
    public required DateTime DateTime { get; set; }

    public required string? PlainText { get; set; }
}

public class ChannelMessageConfiguration : IEntityTypeConfiguration<ChannelMessage>
{
    public void Configure(EntityTypeBuilder<ChannelMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasValueGenerator<Guid7ValueGenerator>();

        builder.HasIndex(x => x.DateTime).IsDescending(true);

        builder.Property(x => x.IsSystem).HasDefaultValue(false);

        builder.Property(x => x.PlainText).HasMaxLength(AppConstants.MaxMessageLength);
    }
}