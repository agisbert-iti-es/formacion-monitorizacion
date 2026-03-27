
using MyDotNetApi.Models;

namespace MyDotNetApi.Repositories;

public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetPlayers();
    Task<bool> PlayerExists(int id);
    Task<Player> AddPlayer(Player player);
}