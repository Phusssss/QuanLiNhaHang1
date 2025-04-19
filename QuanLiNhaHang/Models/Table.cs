using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLiNhaHang.Models
{
    public class Table
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        // Khóa ngoại liên kết với KhuVuc
        [Required]
        [ForeignKey("KhuVuc")]
        public int idKhuVuc { get; set; }
        public KhuVuc? KhuVuc { get; set; }

        // Khóa ngoại liên kết với LoaiBan
        [Required]
        [ForeignKey("LoaiBan")]
        public int idLoaiBan { get; set; }
        public LoaiBan? LoaiBan { get; set; }
        public int Status { get; set; }
        [Required]
        public int OrderIDCurent { get; set; }
        
    
    }
}
