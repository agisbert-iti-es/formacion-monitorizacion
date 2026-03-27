using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using WebApplication2.Services;
using System.Threading.Tasks;

namespace WebApplication2.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;
    
    public PlayersController(IPlayerService playerService) => _playerService = playerService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Player>))]
    public async Task<IActionResult> GetPlayers()
    {
        var players = await _playerService.GetPlayers();
        return Ok(players);
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Player))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostPlayer([FromBody] PlayerDto dto)
    {
        var result = await _playerService.PostPlayer(dto);
        return result;
    }
}
