using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;
using System;
using System.Threading.Tasks;

namespace WebApplication2.Repositories;

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