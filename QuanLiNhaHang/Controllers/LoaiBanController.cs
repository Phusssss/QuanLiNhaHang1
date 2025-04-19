using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLiNhaHang.Models;

[Route("api/[controller]")]
[ApiController]
public class LoaiBanController : ControllerBase
{
    private readonly AppDbContext _context;

    public LoaiBanController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoaiBanDTO>>> GetLoaiBans()
    {
        return await _context.LoaiBans
            .Select(l => new LoaiBanDTO { Id = l.Id, Name = l.Name, MaxPeople = l.MaxPeople })
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<LoaiBan>> PostLoaiBan([FromBody] LoaiBanDTO dto)
    {
        if (dto == null) return BadRequest();

        var loaiBan = new LoaiBan { Name = dto.Name, MaxPeople = dto.MaxPeople };
        _context.LoaiBans.Add(loaiBan);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLoaiBans), new { id = loaiBan.Id }, loaiBan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutLoaiBan(int id, [FromBody] LoaiBanDTO dto)
    {
        var loaiBan = await _context.LoaiBans.FindAsync(id);
        if (loaiBan == null) return NotFound();

        loaiBan.Name = dto.Name;
        loaiBan.MaxPeople = dto.MaxPeople;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLoaiBan(int id)
    {
        var loaiBan = await _context.LoaiBans.FindAsync(id);
        if (loaiBan == null) return NotFound();

        _context.LoaiBans.Remove(loaiBan);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
