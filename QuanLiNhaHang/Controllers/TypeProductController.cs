using Microsoft.AspNetCore.Mvc;
using QuanLiNhaHang.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLiNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TypeProductController(AppDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách loại sản phẩm
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeProductDTO>>> GetTypeProducts()
        {
            return await _context.TypeProducts
                .Select(tp => new TypeProductDTO { Id = tp.Id, Name = tp.Name })
                .ToListAsync();
        }

        // Lấy loại sản phẩm theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeProductDTO>> GetTypeProduct(int id)
        {
            var typeProduct = await _context.TypeProducts.FindAsync(id);
            if (typeProduct == null)
            {
                return NotFound();
            }

            return new TypeProductDTO { Id = typeProduct.Id, Name = typeProduct.Name };
        }

        // Thêm loại sản phẩm mới
        [HttpPost]
        public async Task<ActionResult<TypeProduct>> AddTypeProduct(TypeProductDTO typeProductDTO)
        {
            var typeProduct = new TypeProduct
            {
                Name = typeProductDTO.Name
            };

            _context.TypeProducts.Add(typeProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTypeProduct), new { id = typeProduct.Id }, typeProduct);
        }

        // Cập nhật loại sản phẩm
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTypeProduct(int id, TypeProductDTO typeProductDTO)
        {
            var typeProduct = await _context.TypeProducts.FindAsync(id);
            if (typeProduct == null)
            {
                return NotFound();
            }

            typeProduct.Name = typeProductDTO.Name;
            _context.Entry(typeProduct).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Xóa loại sản phẩm
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTypeProduct(int id)
        {
            var typeProduct = await _context.TypeProducts.FindAsync(id);
            if (typeProduct == null)
            {
                return NotFound();
            }

            _context.TypeProducts.Remove(typeProduct);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
