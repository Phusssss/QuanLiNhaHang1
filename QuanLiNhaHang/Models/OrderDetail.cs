// Trong OrderDetail.cs
using QuanLiNhaHang.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class OrderDetail
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Order")]
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    [Required]
    [ForeignKey("Product")]
    public int idProduct { get; set; }
    public Product? Product { get; set; }

    [Required]
    public int Quality { get; set; }

    [Required]
    public decimal price { get; set; }

    // Thêm trạng thái cho món ăn
    public int Status { get; set; } = 0; // 0: Chưa chế biến, 1: Đang chế biến, 2: Đã hoàn thành
}