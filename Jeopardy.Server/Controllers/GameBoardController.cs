using Jeopardy.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jeopardy.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GameBoardController : ControllerBase
    {
        // GET: gameboard/create
        [HttpGet]
        public Dictionary<string, IEnumerable<QuestionCard>> Create()
        {
            var model = new GameBoard();
            return model.Game;
        }
    }
}
