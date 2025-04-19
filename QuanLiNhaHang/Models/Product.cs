using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLiNhaHang.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        [ForeignKey("TypeProduct")]
        public int idTypeProduct { get; set; }
        public TypeProduct? TypeProduct { get; set; }

        public string? ImageUrl { get; set; } // Thêm trường hình ảnh
    }
}
