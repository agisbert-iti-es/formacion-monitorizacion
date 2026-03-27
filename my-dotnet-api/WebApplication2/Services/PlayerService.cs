using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using WebApplication2.Repositories;
using System;
using System.Threading.Tasks;

namespace WebApplication2.Services;

public interface IPlayerService
{
    Task<IEnumerable<Player>> GetPlayers();
    Task<IActionResult> PostPlayer(PlayerDto dto);
}

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task<IEnumerable<Player>> GetPlayers()
    {
        return await _playerRepository.GetPlayers();
    }

    public async Task<IActionResult> PostPlayer(PlayerDto dto)
    {
        var exists = await _playerRepository.PlayerExists(dto.Id);
        if (exists) return new ConflictObjectResult($"Player with id {dto.Id} already exists.");
        
        // Simular fallo 
        if (DateTime.Now.Minute % 3 == 0)
        {
            var text = $"Simulated failure at {DateTime.Now:HH:mm:ss}";
            Console.Error.WriteLine($"Usando Console.Error.WriteLine: {text}");
            
            int httpStatusCode = DateTime.Now.Second % 2 != 0 ? 500 : 400;

            if (httpStatusCode == 500)
            {
                throw new Exception(text); // Esto se capturará como una excepción en OpenTelemetry
            }
            else
            {
                return new BadRequestObjectResult(text); // Esto se registrará como un error HTTP en OpenTelemetry
            }            
        }

        var player = new Player { Id = dto.Id, Name = dto.Name };
        var addedPlayer = await _playerRepository.AddPlayer(player);

        return new CreatedAtActionResult("GetPlayers", "Players", new { id = addedPlayer.Id }, addedPlayer);
    }
}