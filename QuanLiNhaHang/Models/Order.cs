using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLiNhaHang.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserID { get; set; }
        public User User { get; set; }

        [Required]
        public DateTime TimeCreate { get; set; }

        [Required]
        public DateTime? TimePay { get; set; } // Cho phép null

        [Required]
        public int Status { get; set; } = 0; // 0: Chưa thanh toán, 1: Đã thanh toán

        [Required]
        [ForeignKey("Table")]
        public int idTable { get; set; } // Mã bàn
        public string NameTable { get; set; } // Tên bàn

        public List<OrderDetail> OrderDetails { get; set; }
    }
}
