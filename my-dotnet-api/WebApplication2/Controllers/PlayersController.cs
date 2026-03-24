using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;
using Microsoft.AspNetCore.Http;
using System;

namespace WebApplication2.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class PlayersController : ControllerBase
{
    private readonly AppDbContext _db;
    public PlayersController(AppDbContext db) => _db = db;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Player>))]
    public async Task<IActionResult> GetPlayers()
    {
        var players = await _db.Players.AsNoTracking().ToListAsync();
        return Ok(players);
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Player))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostPlayer([FromBody] PlayerDto dto)
    {
        // Simular fallo cuando el segundo actual es impar
        if (DateTime.Now.Second % 2 != 0)
        {
            var text = $"Simulated failure at {DateTime.Now:HH:mm:ss}";
            Console.Error.WriteLine($"Usando Console.Error.WriteLine: {text}");
            Console.WriteLine($"Usando Console.WriteLine: {text}");
            

            return StatusCode(500, text);
        }

        var exists = await _db.Players.AnyAsync(p => p.Id == dto.Id);
        if (exists) return Conflict($"Player with id {dto.Id} already exists.");
        var player = new Player { Id = dto.Id, Name = dto.Name };
        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPlayers), new { id = player.Id }, player);
    }
}
