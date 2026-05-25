using QuanLyKhoaHoc5.Web.Models.Entities;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Unit test cho các static helper method của entity Diem:
/// - TinhTongKet (GK * 30% + CK * 70%)
/// - TinhXepLoai (Xuất sắc / Giỏi / Khá / Trung bình / Yếu)
/// Đây là pure functions — không cần DB, không cần mock.
/// </summary>
public class DiemHelperTests
{
    // ─── TinhTongKet ────────────────────────────────────────────────────────────

    [Fact]
    public void TinhTongKet_BothNull_ReturnsNull()
    {
        var result = Diem.TinhTongKet(null, null);
        Assert.Null(result);
    }

    [Fact]
    public void TinhTongKet_NullGiuaKy_ReturnsNull()
    {
        var result = Diem.TinhTongKet(null, 8.0);
        Assert.Null(result);
    }

    [Fact]
    public void TinhTongKet_NullCuoiKy_ReturnsNull()
    {
        var result = Diem.TinhTongKet(7.0, null);
        Assert.Null(result);
    }

    [Fact]
    public void TinhTongKet_ValidInput_CalculatesFormula()
    {
        // 8.0 * 0.3 + 9.0 * 0.7 = 2.4 + 6.3 = 8.70
        var result = Diem.TinhTongKet(8.0, 9.0);
        Assert.NotNull(result);
        Assert.Equal(8.70, result!.Value, precision: 2);
    }

    [Fact]
    public void TinhTongKet_EqualScores_ReturnsEqualScore()
    {
        // 7.0 * 0.3 + 7.0 * 0.7 = 7.0
        var result = Diem.TinhTongKet(7.0, 7.0);
        Assert.Equal(7.00, result!.Value, precision: 2);
    }

    [Fact]
    public void TinhTongKet_ZeroScores_ReturnsZero()
    {
        var result = Diem.TinhTongKet(0.0, 0.0);
        Assert.Equal(0.00, result!.Value, precision: 2);
    }

    [Fact]
    public void TinhTongKet_PerfectScores_ReturnsTen()
    {
        // 10 * 0.3 + 10 * 0.7 = 10
        var result = Diem.TinhTongKet(10.0, 10.0);
        Assert.Equal(10.00, result!.Value, precision: 2);
    }

    [Fact]
    public void TinhTongKet_HighCKWeight_ReflectsFormula()
    {
        // 5.0 * 0.3 + 10.0 * 0.7 = 1.5 + 7.0 = 8.5
        var result = Diem.TinhTongKet(5.0, 10.0);
        Assert.Equal(8.50, result!.Value, precision: 2);
    }

    // ─── TinhXepLoai ─────────────────────────────────────────────────────────────

    [Fact]
    public void TinhXepLoai_Null_ReturnsNull()
    {
        var result = Diem.TinhXepLoai(null);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(8.5, "Xuất sắc")]
    [InlineData(9.0, "Xuất sắc")]
    [InlineData(10.0, "Xuất sắc")]
    public void TinhXepLoai_XuatSac_Threshold(double diem, string expected)
    {
        Assert.Equal(expected, Diem.TinhXepLoai(diem));
    }

    [Theory]
    [InlineData(7.0, "Giỏi")]
    [InlineData(7.5, "Giỏi")]
    [InlineData(8.4, "Giỏi")]
    public void TinhXepLoai_Gioi_Threshold(double diem, string expected)
    {
        Assert.Equal(expected, Diem.TinhXepLoai(diem));
    }

    [Theory]
    [InlineData(5.5, "Khá")]
    [InlineData(6.0, "Khá")]
    [InlineData(6.9, "Khá")]
    public void TinhXepLoai_Kha_Threshold(double diem, string expected)
    {
        Assert.Equal(expected, Diem.TinhXepLoai(diem));
    }

    [Theory]
    [InlineData(4.0, "Trung bình")]
    [InlineData(4.5, "Trung bình")]
    [InlineData(5.4, "Trung bình")]
    public void TinhXepLoai_TrungBinh_Threshold(double diem, string expected)
    {
        Assert.Equal(expected, Diem.TinhXepLoai(diem));
    }

    [Theory]
    [InlineData(3.9, "Yếu")]
    [InlineData(2.0, "Yếu")]
    [InlineData(0.0, "Yếu")]
    public void TinhXepLoai_Yeu_Threshold(double diem, string expected)
    {
        Assert.Equal(expected, Diem.TinhXepLoai(diem));
    }

    [Fact]
    public void TinhXepLoai_ExactBoundary_85_IsXuatSac()
    {
        Assert.Equal("Xuất sắc", Diem.TinhXepLoai(8.5));
    }

    [Fact]
    public void TinhXepLoai_ExactBoundary_70_IsGioi()
    {
        Assert.Equal("Giỏi", Diem.TinhXepLoai(7.0));
    }

    [Fact]
    public void TinhXepLoai_ExactBoundary_40_IsTrungBinh()
    {
        Assert.Equal("Trung bình", Diem.TinhXepLoai(4.0));
    }
}
