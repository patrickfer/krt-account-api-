using KRT.Domain.Entities;
using KRT.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KRT.Infrastructure.Data.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.HolderName)
            .IsRequired()
            .HasMaxLength(150);

        builder.OwnsOne(a => a.Cpf, cpf =>
        {
            cpf.Property(c => c.Value)
                .HasColumnName("Cpf")
                .IsRequired()
                .HasMaxLength(11);

            cpf.HasIndex(c => c.Value)
                .IsUnique()
                .HasDatabaseName("IX_Accounts_Cpf");
        });

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
