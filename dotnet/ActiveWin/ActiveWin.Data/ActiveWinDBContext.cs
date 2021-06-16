using System;
using System.Runtime.InteropServices;
using ActiveWin.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace ActiveWin.Data
{
    public partial class ActiveWinDBContext : DbContext
    {
        public ActiveWinDBContext()
        {
        }

        public ActiveWinDBContext(DbContextOptions<ActiveWinDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Platform> Platforms { get; set; }
        public virtual DbSet<TimeEntry> TimeEntries { get; set; }
        public virtual DbSet<TimeEntryHistory> TimeEntryHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlite(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"DataSource={AppConstants.BasePath}ActiveWin.data\\ActiveWinDB.db" : $"DataSource={AppConstants.BasePath}ActiveWin.data/ActiveWinDB.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Platform>(entity =>
            {
                entity.ToTable("Platform");

                entity.HasIndex(e => e.Id, "IX_Platform_Id")
                    .IsUnique();

                entity.Property(e => e.CreatedAt).IsRequired();

                entity.Property(e => e.DisplayName).IsRequired();

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.ToTable("TimeEntry");

                entity.HasIndex(e => e.Id, "IX_TimeEntry_Id")
                    .IsUnique();

                entity.Property(e => e.Application).IsRequired();

                entity.Property(e => e.CreatedAt).IsRequired();

                entity.Property(e => e.IconPath).IsRequired();

                entity.Property(e => e.Platform)
                    .IsRequired()
                    .HasColumnType("NUMERIC");
            });

            modelBuilder.Entity<TimeEntryHistory>(entity =>
            {
                entity.ToTable("TimeEntryHistory");

                entity.HasIndex(e => e.Id, "IX_TimeEntryHistory_Id")
                    .IsUnique();

                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(d => d.TimeEntry)
                    .WithMany(p => p.TimeEntryHistories)
                    .HasForeignKey(d => d.TimeEntryId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
