using System.ComponentModel.DataAnnotations;

namespace QuanLiNhaHang.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Salary {  get; set; }
        [Required]
        public string TypeSalary { get; set; }
        public List<User>? Users { get; set; }

    }
}
