using System.ComponentModel.DataAnnotations;

namespace QuanLiNhaHang.Models
{
    public class LoaiBan
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int MaxPeople { get; set; }
        public List<Table>? Tables { get; set; }
    }
}
