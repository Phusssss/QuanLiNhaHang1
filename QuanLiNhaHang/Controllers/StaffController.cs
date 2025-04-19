using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLiNhaHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace QuanLiNhaHang.Controllers
{
    [Route("api/staff")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StaffController(AppDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách nhân viên
        [HttpGet]
        public async Task<IActionResult> GetStaff()
        {
            try
            {
                var staff = await _context.Users
                    .Include(u => u.Job)
                    .Select(u => new
                    {
                        id = u.Id,
                        name = u.Name,
                        email = u.Email,
                        phoneNumber = u.PhoneNumber,
                        jobId = u.IdJob,
                        jobName = u.Job.Name
                    })
                    .ToListAsync();
                return Ok(staff);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi lấy danh sách nhân viên", error = ex.Message });
            }
        }

        // Thêm nhân viên mới
        [HttpPost]
        public async Task<IActionResult> AddStaff([FromBody] UserRequest request)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(request.JobId);
                if (job == null)
                {
                    return BadRequest(new { message = "Công việc không tồn tại!" });
                }

                byte[] salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
                string hashedPassword = HashPassword("123456", salt);

                var staff = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = hashedPassword,
                    IdJob = request.JobId,
                    PhoneNumber = request.PhoneNumber
                };

                _context.Users.Add(staff);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi thêm nhân viên", error = ex.Message });
            }
        }

        // Cập nhật nhân viên
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] UserRequest request)
        {
            try
            {
                var staff = await _context.Users.FindAsync(id);
                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhân viên!" });
                }

                var job = await _context.Jobs.FindAsync(request.JobId);
                if (job == null)
                {
                    return BadRequest(new { message = "Công việc không tồn tại!" });
                }

                staff.Name = request.Name;
                staff.Email = request.Email;
                staff.PhoneNumber = request.PhoneNumber;
                staff.IdJob = request.JobId;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi cập nhật nhân viên", error = ex.Message });
            }
        }

        // Xóa nhân viên
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            try
            {
                var staff = await _context.Users.FindAsync(id);
                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhân viên!" });
                }

                _context.Users.Remove(staff);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Xóa nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi xóa nhân viên", error = ex.Message });
            }
        }

        // DTO cho request Timekeeping
        public class TimekeepingRequest
        {
            public int UserId { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public int Status { get; set; }
            public string TypeJob { get; set; }
        }

        // Thêm lịch làm việc (single entry)
        [HttpPost("timekeeping")]
        public async Task<IActionResult> AddTimekeeping([FromBody] TimekeepingRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhân viên!" });
                }

                if (request.Start >= request.End)
                {
                    return BadRequest(new { message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc!" });
                }

                var conflictingSchedule = await _context.Timekeepings
                    .Where(t => t.IdUser == request.UserId)
                    .Where(t => t.Start < request.End && t.End > request.Start)
                    .AnyAsync();

                if (conflictingSchedule)
                {
                    return BadRequest(new { message = "Lịch làm việc bị trùng với lịch hiện có!" });
                }

                if (!Enum.TryParse<JobType>(request.TypeJob, true, out var jobType))
                {
                    return BadRequest(new { message = "Loại công việc không hợp lệ!" });
                }

                var timekeeping = new Timekeeping
                {
                    IdUser = request.UserId,
                    Start = request.Start,
                    End = request.End,
                    Status = request.Status,
                    TypeJob = jobType
                };

                _context.Timekeepings.Add(timekeeping);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Thêm lịch làm việc thành công!",
                    timekeepingId = timekeeping.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi thêm lịch làm việc", error = ex.Message });
            }
        }

        // Thêm nhiều lịch làm việc (batch for FullTime)
        [HttpPost("timekeeping/batch")]
        public async Task<IActionResult> AddTimekeepingBatch([FromBody] List<TimekeepingRequest> requests)
        {
            try
            {
                var timekeepings = new List<Timekeeping>();

                foreach (var request in requests)
                {
                    var user = await _context.Users.FindAsync(request.UserId);
                    if (user == null)
                    {
                        return NotFound(new { message = $"Không tìm thấy nhân viên với ID {request.UserId}!" });
                    }

                    if (request.Start >= request.End)
                    {
                        return BadRequest(new { message = $"Thời gian không hợp lệ cho nhân viên {request.UserId}!" });
                    }

                    var conflictingSchedule = await _context.Timekeepings
                        .Where(t => t.IdUser == request.UserId)
                        .Where(t => t.Start < request.End && t.End > request.Start)
                        .AnyAsync();

                    if (conflictingSchedule)
                    {
                        return BadRequest(new { message = $"Lịch trùng cho nhân viên {request.UserId}!" });
                    }

                    if (!Enum.TryParse<JobType>(request.TypeJob, true, out var jobType))
                    {
                        return BadRequest(new { message = "Loại công việc không hợp lệ!" });
                    }

                    timekeepings.Add(new Timekeeping
                    {
                        IdUser = request.UserId,
                        Start = request.Start,
                        End = request.End,
                        Status = request.Status,
                        TypeJob = jobType
                    });
                }

                _context.Timekeepings.AddRange(timekeepings);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Thêm tất cả lịch làm việc thành công!",
                    timekeepingIds = timekeepings.Select(t => t.Id)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi thêm lịch làm việc", error = ex.Message });
            }
        }

        // Lấy danh sách lịch làm việc của một nhân viên
        [HttpGet("{id}/timekeeping")]
        public async Task<IActionResult> GetStaffTimekeeping(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhân viên!" });
                }

                var timekeepings = await _context.Timekeepings
                    .Where(t => t.IdUser == id)
                    .Select(t => new
                    {
                        id = t.Id,
                        start = t.Start,
                        end = t.End,
                        status = t.Status,
                        typeJob = t.TypeJob.ToString(),
                        userName = t.User.Name
                    })
                    .ToListAsync();

                return Ok(timekeepings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi lấy danh sách lịch làm việc", error = ex.Message });
            }
        }

        // Sửa lịch làm việc
        [HttpPut("timekeeping/{id}")]
        public async Task<IActionResult> UpdateTimekeeping(int id, [FromBody] TimekeepingRequest request)
        {
            try
            {
                var timekeeping = await _context.Timekeepings.FindAsync(id);
                if (timekeeping == null)
                {
                    return NotFound(new { message = "Không tìm thấy lịch làm việc!" });
                }

                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhân viên!" });
                }

                if (request.Start >= request.End)
                {
                    return BadRequest(new { message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc!" });
                }

                // Kiểm tra trùng lịch, bỏ qua chính lịch đang sửa
                var conflictingSchedule = await _context.Timekeepings
                    .Where(t => t.IdUser == request.UserId && t.Id != id)
                    .Where(t => t.Start < request.End && t.End > request.Start)
                    .AnyAsync();

                if (conflictingSchedule)
                {
                    return BadRequest(new { message = "Lịch làm việc bị trùng với lịch hiện có!" });
                }

                if (!Enum.TryParse<JobType>(request.TypeJob, true, out var jobType))
                {
                    return BadRequest(new { message = "Loại công việc không hợp lệ!" });
                }

                timekeeping.IdUser = request.UserId;
                timekeeping.Start = request.Start;
                timekeeping.End = request.End;
                timekeeping.Status = request.Status;
                timekeeping.TypeJob = jobType;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật lịch làm việc thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi cập nhật lịch làm việc", error = ex.Message });
            }
        }
        [HttpDelete("timekeeping/{id}")]
        public async Task<IActionResult> DeleteTimekeeping(int id)
        {
            try
            {
                var timekeeping = await _context.Timekeepings.FindAsync(id);
                if (timekeeping == null)
                {
                    return NotFound(new { message = "Không tìm thấy lịch làm việc!" });
                }

                _context.Timekeepings.Remove(timekeeping);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa lịch làm việc thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi xóa lịch làm việc", error = ex.Message });
            }
        }
        private string HashPassword(string password, byte[] salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }
        public class UserRequest
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public int JobId { get; set; }
        }
    }
}