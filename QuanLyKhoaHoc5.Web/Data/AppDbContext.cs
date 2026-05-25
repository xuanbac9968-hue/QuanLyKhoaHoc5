using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Models.Entities;

namespace QuanLyKhoaHoc5.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<HocVien> HocViens => Set<HocVien>();
    public DbSet<GiangVien> GiangViens => Set<GiangVien>();
    public DbSet<KhoaHoc> KhoaHocs => Set<KhoaHoc>();
    public DbSet<LopHoc> LopHocs => Set<LopHoc>();
    public DbSet<LichHoc> LichHocs => Set<LichHoc>();
    public DbSet<DangKyKhoaHoc> DangKyKhoaHocs => Set<DangKyKhoaHoc>();
    public DbSet<Diem> Diems => Set<Diem>();
    public DbSet<GoiYKhoaHoc> GoiYKhoaHocs => Set<GoiYKhoaHoc>();
    public DbSet<ThongBao> ThongBaos => Set<ThongBao>();
    public DbSet<PhanCongGiangDay> PhanCongGiangDays => Set<PhanCongGiangDay>();
    public DbSet<ChatHistory> ChatHistories => Set<ChatHistory>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
    public DbSet<KyHoc> KyHocs => Set<KyHoc>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NguoiDung>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.VaiTro).HasConversion<string>().HasDefaultValue("HocVien");
        });

        modelBuilder.Entity<HocVien>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.NguoiDung).WithOne(x => x.HocVien)
                .HasForeignKey<HocVien>(x => x.Id).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.MaHocVien).IsUnique();
        });

        modelBuilder.Entity<GiangVien>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.NguoiDung).WithOne(x => x.GiangVien)
                .HasForeignKey<GiangVien>(x => x.Id).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.MaGiangVien).IsUnique();
        });

        modelBuilder.Entity<LopHoc>(e =>
        {
            e.HasOne(x => x.KhoaHoc).WithMany(x => x.LopHocs)
                .HasForeignKey(x => x.KhoaHocId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.GiangVien).WithMany(x => x.LopHocs)
                .HasForeignKey(x => x.GiangVienId).OnDelete(DeleteBehavior.SetNull);
        });

        // LichHoc belongs to LopHoc (per-session records)
        modelBuilder.Entity<LichHoc>(e =>
        {
            e.HasOne(x => x.LopHoc).WithMany(x => x.LichHocs)
                .HasForeignKey(x => x.LopHocId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DangKyKhoaHoc>(e =>
        {
            e.HasIndex(x => new { x.HocVienId, x.LopHocId }).IsUnique();
            e.HasOne(x => x.HocVien).WithMany(x => x.DangKys)
                .HasForeignKey(x => x.HocVienId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.LopHoc).WithMany(x => x.DangKys)
                .HasForeignKey(x => x.LopHocId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.NguoiDuyet).WithMany()
                .HasForeignKey(x => x.NguoiDuyetId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Diem>(e =>
        {
            e.HasOne(x => x.DangKy).WithOne(x => x.Diem)
                .HasForeignKey<Diem>(x => x.DangKyId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GoiYKhoaHoc>(e =>
        {
            e.HasOne(x => x.HocVien).WithMany(x => x.GoiYs)
                .HasForeignKey(x => x.HocVienId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.KhoaHocGoiY).WithMany(x => x.GoiYs)
                .HasForeignKey(x => x.KhoaHocGoiYId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ThongBao>(e =>
        {
            e.HasOne(x => x.NguoiNhan).WithMany(x => x.ThongBaos)
                .HasForeignKey(x => x.NguoiNhanId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PhanCongGiangDay>(e =>
        {
            e.HasOne(x => x.GiangVien).WithMany(x => x.PhanCongs)
                .HasForeignKey(x => x.GiangVienId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.KhoaHoc).WithMany(x => x.PhanCongs)
                .HasForeignKey(x => x.KhoaHocId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Diem>(e =>
        {
            e.HasOne(x => x.KyHoc).WithMany(x => x.Diems)
                .HasForeignKey(x => x.KyHocId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ChatHistory>(e =>
        {
            e.HasOne(x => x.NguoiDung).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.UserId, x.CreatedAt });
        });

        modelBuilder.Entity<ThanhToan>(e =>
        {
            e.HasOne(x => x.HocVien).WithMany()
                .HasForeignKey(x => x.HocVienId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.KhoaHoc).WithMany()
                .HasForeignKey(x => x.KhoaHocId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.NguoiDuyet).WithMany()
                .HasForeignKey(x => x.NguoiDuyetId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
