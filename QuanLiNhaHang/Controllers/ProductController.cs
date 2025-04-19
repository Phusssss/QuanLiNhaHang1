using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLiNhaHang.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace QuanLiNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; // Để lấy thư mục lưu ảnh

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Lấy danh sách sản phẩm
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            return await _context.Products
                .Include(p => p.TypeProduct)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    NameTypeProduct = p.TypeProduct != null ? p.TypeProduct.Name : "Không có loại",
                    ImageUrl = p.ImageUrl,
                  
                    
                })
                .ToListAsync();
        }

        // Lấy sản phẩm theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.TypeProduct)
                .Where(p => p.Id == id)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    NameTypeProduct = p.TypeProduct != null ? p.TypeProduct.Name : "Không có loại",
                    ImageUrl = p.ImageUrl
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // Thêm sản phẩm mới
        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct([FromForm] ProductCreateDTO productDTO)
        {
            // Kiểm tra xem ảnh có tồn tại không
            if (productDTO.Image == null || productDTO.Image.Length == 0)
            {
                return BadRequest("Lỗi: Không có ảnh được tải lên.");
            }

            // Lấy thư mục hiện tại
            var currentDirectory = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(currentDirectory))
            {
                return BadRequest("Lỗi: Không thể xác định thư mục hiện tại.");
            }

            // Tạo thư mục lưu ảnh
            var uploadsFolder = Path.Combine(currentDirectory, "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Lưu ảnh với tên duy nhất
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productDTO.Image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await productDTO.Image.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/{fileName}";

            // Lưu vào database
            var product = new Product
            {
                Name = productDTO.Name,
                Price = productDTO.Price,
                idTypeProduct = productDTO.idTypeProduct,
                ImageUrl = imageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }


        // Cập nhật sản phẩm
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductCreateDTO productDTO)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = productDTO.Name;
            product.Price = productDTO.Price;
            product.idTypeProduct = productDTO.idTypeProduct;

            if (productDTO.Image != null)
            {
                string uploadFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string uniqueFileName = $"{Guid.NewGuid()}_{productDTO.Image.FileName}";
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await productDTO.Image.CopyToAsync(fileStream);
                }

                product.ImageUrl = $"/uploads/{uniqueFileName}";
            }

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Xóa sản phẩm
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                string filePath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
