using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;
using System;
using System.Threading.Tasks;

namespace WebApplication2.Repositories;

public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetPlayers();
    Task<bool> PlayerExists(int id);
    Task<Player> AddPlayer(Player player);
}