using Microsoft.AspNetCore.Mvc;
using QuanLiNhaHang.Models;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class TableController : ControllerBase
{
    private readonly AppDbContext _context;

    public TableController(AppDbContext context)
    {
        _context = context;
    }

    // Lấy danh sách bàn
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TableDTO>>> GetTables()
    {
        var tables = await _context.Tables
            .Include(t => t.KhuVuc)
            .Include(t => t.LoaiBan)
            .Select(t => new TableDTO
            {
                Id = t.Id,
                Name = t.Name,
                NameKhuVuc = t.KhuVuc.Name,
                NameLoaiPhong = t.LoaiBan.Name,
                MaxPeople = t.LoaiBan.MaxPeople.ToString(),
                status =t.Status,
                OrderID =t.OrderIDCurent
            })
            .ToListAsync();

        return Ok(tables);
    }

    // Thêm bàn mới
    [HttpPost]
    public async Task<ActionResult<Table>> PostTable([FromBody] TableCreateDTO dto)
    {
        if (dto == null) return BadRequest("Invalid data");

        var table = new Table
        {
            Name = dto.Name,
            idLoaiBan = dto.IDLoaiBan,
            idKhuVuc = dto.IDKhuVuc
        };

        _context.Tables.Add(table);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTables), new { id = table.Id }, table);
    }


    // Cập nhật thông tin bàn
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTable(int id, [FromBody] TableCreateDTO dto)
    {
        if (dto == null || id != dto.Id)
        {
            return BadRequest("Invalid data");
        }

        var table = await _context.Tables.FindAsync(id);
        if (table == null)
        {
            return NotFound("Table not found");
        }

        table.Name = dto.Name;
        table.idLoaiBan = dto.IDLoaiBan;
        table.idKhuVuc = dto.IDKhuVuc;

        _context.Tables.Update(table);
        await _context.SaveChangesAsync();

        return NoContent(); // Trả về 204 No Content nếu cập nhật thành công
    }


    // Xóa bàn
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTable(int id)
    {
        var table = await _context.Tables.FindAsync(id);
        if (table == null) return NotFound();

        _context.Tables.Remove(table);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
