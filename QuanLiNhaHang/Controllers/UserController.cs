using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLiNhaHang.Models;
using QuanLiNhaHang.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System;

namespace QuanLiNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // Đăng ký người dùng
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra email đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Email == registerDTO.Email))
            {
                return BadRequest(new { Message = "Email đã được sử dụng." });
            }

            // Kiểm tra idJob có tồn tại không
            var jobExists = await _context.Jobs.AnyAsync(j => j.Id == registerDTO.IdJob);
            if (!jobExists)
            {
                return BadRequest(new { Message = "Công việc không tồn tại." });
            }

            // Tạo salt và hash mật khẩu
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string hashedPassword = HashPassword(registerDTO.Password, salt);

            // Tạo user mới
            var user = new User
            {
                Name = registerDTO.Name,
                Email = registerDTO.Email,
                Password = hashedPassword,
                PhoneNumber = registerDTO.PhoneNumber,
                IdJob = registerDTO.IdJob 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đăng ký thành công!", UserId = user.Id });
        }

        // Đăng nhập người dùng
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Tìm user theo email
            var user = await _context.Users
                .Include(u => u.Job) // Include thông tin Job nếu cần
                .FirstOrDefaultAsync(u => u.Email == loginDTO.Email);

            if (user == null || !VerifyPassword(loginDTO.Password, user.Password))
            {
                return Unauthorized(new { Message = "Email hoặc mật khẩu không đúng." });
            }

            // Trả về thông tin đăng nhập thành công
            return Ok(new
            {
                Message = "Đăng nhập thành công!",
                UserId = user.Id,
                Name = user.Name,
                Job = user.Job.Name // Trả thêm thông tin Job nếu cần
            });
        }

        // Hàm hash mật khẩu
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

        // Hàm xác minh mật khẩu
        private bool VerifyPassword(string password, string storedPassword)
        {
            var parts = storedPassword.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            string hashedPassword = HashPassword(password, salt);
            return hashedPassword == storedPassword;
        }
    }
}