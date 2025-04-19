using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLiNhaHang.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLiNhaHang.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JobController(AppDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách công việc
        [HttpGet]
        public async Task<IActionResult> GetJobs()
        {
            try
            {
                var jobs = await _context.Jobs
                    .Select(j => new
                    {
                        id = j.Id,
                        name = j.Name,
                        description = j.Description,
                        salary = j.Salary,
                        typeSalary = j.TypeSalary
                    })
                    .ToListAsync();
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi lấy danh sách công việc", error = ex.Message });
            }
        }

        // Thêm công việc mới
        [HttpPost]
        public async Task<IActionResult> AddJob([FromBody] JobRequest request)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Tên công việc không được để trống!" });
                }
                if (request.Salary < 0)
                {
                    return BadRequest(new { message = "Lương phải là số không âm!" });
                }

                var job = new Job
                {
                    Name = request.Name,
                    Description = request.Description,
                    Salary = request.Salary,
                    TypeSalary = request.TypeSalary
                };

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm công việc thành công!", jobId = job.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi thêm công việc", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] JobRequest request)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                {
                    return NotFound(new { message = "Không tìm thấy công việc!" });
                }

                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Tên công việc không được để trống!" });
                }
                if (request.Salary < 0)
                {
                    return BadRequest(new { message = "Lương phải là số không âm!" });
                }

                job.Name = request.Name;
                job.Description = request.Description;
                job.Salary = request.Salary;
                job.TypeSalary = request.TypeSalary;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật công việc thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi cập nhật công việc", error = ex.Message });
            }
        }

        // Xóa công việc
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                {
                    return NotFound(new { message = "Không tìm thấy công việc!" });
                }

                // Kiểm tra xem công việc có đang được sử dụng bởi nhân viên không
                var isInUse = await _context.Users.AnyAsync(u => u.IdJob == id);
                if (isInUse)
                {
                    return BadRequest(new { message = "Không thể xóa công việc vì đang được sử dụng bởi nhân viên!" });
                }

                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Xóa công việc thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi xóa công việc", error = ex.Message });
            }
        }

        // DTO cho request
        public class JobRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Salary { get; set; }
            public string TypeSalary { get; set; }
        }
    }
}