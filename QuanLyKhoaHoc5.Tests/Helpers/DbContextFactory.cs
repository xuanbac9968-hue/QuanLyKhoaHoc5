using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;

namespace QuanLyKhoaHoc5.Tests.Helpers;

/// <summary>
/// Tạo AppDbContext dùng InMemory provider — mỗi test dùng một database riêng
/// để tránh state leaking giữa các test case.
/// </summary>
public static class DbContextFactory
{
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            // Tắt warning về transaction không hỗ trợ trong InMemory
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
