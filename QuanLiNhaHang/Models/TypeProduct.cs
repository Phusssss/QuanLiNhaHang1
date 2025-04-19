using System.ComponentModel.DataAnnotations;

namespace QuanLiNhaHang.Models
{
    public class TypeProduct
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        List<Product>? Products { get; set; }
    }
}

