using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class ChatHistory
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(20)]
    public string Role { get; set; } = "user"; // user | assistant

    [Required]
    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public NguoiDung NguoiDung { get; set; } = null!;
}
