using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLiNhaHang.Models;

[Route("api/[controller]")]
[ApiController]
public class KhuVucController : ControllerBase
{
    private readonly AppDbContext _context;

    public KhuVucController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<KhuVucDTO>>> GetKhuVucs()
    {
        return await _context.KhuVucs
            .Select(k => new KhuVucDTO { Id = k.Id, Name = k.Name, Description = k.Description })
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<KhuVuc>> PostKhuVuc([FromBody] KhuVucDTO dto)
    {
        if (dto == null) return BadRequest();

        var khuVuc = new KhuVuc { Name = dto.Name, Description = dto.Description };
        _context.KhuVucs.Add(khuVuc);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetKhuVucs), new { id = khuVuc.Id }, khuVuc);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutKhuVuc(int id, [FromBody] KhuVucDTO dto)
    {
        var khuVuc = await _context.KhuVucs.FindAsync(id);
        if (khuVuc == null) return NotFound();

        khuVuc.Name = dto.Name;
        khuVuc.Description = dto.Description;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteKhuVuc(int id)
    {
        var khuVuc = await _context.KhuVucs.FindAsync(id);
        if (khuVuc == null) return NotFound();

        _context.KhuVucs.Remove(khuVuc);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
