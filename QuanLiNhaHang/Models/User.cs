using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLiNhaHang.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [ForeignKey("Job")]
        public int IdJob { get; set; }  // Sửa idJob thành IdJob theo convention
        public Job? Job { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public List<Order>? Orders { get; set; }
        public List<Timekeeping>? Timekeepings { get; set; }  // Thêm relationship với Timekeeping
    }
}