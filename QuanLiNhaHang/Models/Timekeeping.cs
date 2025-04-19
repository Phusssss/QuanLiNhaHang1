using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLiNhaHang.Models
{
    public enum JobType
    {
        FullTime,
        PartTime
    }

    public class Timekeeping
    {
        public int Id { get; set; }
        public int IdUser { get; set; }  // Sửa idUser thành IdUser theo convention
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Status { get; set; }
        public JobType TypeJob { get; set; }  // Sử dụng enum thay vì string

        // Navigation property
        [ForeignKey("IdUser")]
        public User? User { get; set; }
    }
}