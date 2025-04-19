using System.ComponentModel.DataAnnotations;

namespace QuanLiNhaHang.Models
{
    public class KhuVuc
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public List<Table>? Tables { get; set; }
    }
}
