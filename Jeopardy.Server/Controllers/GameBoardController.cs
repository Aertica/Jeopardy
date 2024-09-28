using Jeopardy.Discord;
using Jeopardy.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace Jeopardy.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GameBoardController : ControllerBase
    {
        // GET: gameboard/create/5
        [HttpGet("{id}")]
        public GameBoard Create(Guid id)
        {
            return new GameBoard(id);
        }
    }
}
