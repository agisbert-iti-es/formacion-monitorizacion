using Microsoft.EntityFrameworkCore;
using MyDotNetApi.Data;
using MyDotNetApi.Models;

namespace MyDotNetApi.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly AppDbContext _db;

    public PlayerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Player>> GetPlayers()
    {
        return await _db.Players.AsNoTracking().ToListAsync();
    }

    public async Task<bool> PlayerExists(int id)
    {
        return await _db.Players.AnyAsync(p => p.Id == id);
    }

    public async Task<Player> AddPlayer(Player player)
    {
        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        return player;
    }
}