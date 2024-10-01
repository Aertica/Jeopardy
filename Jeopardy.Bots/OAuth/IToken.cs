using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jeopardy.Bots.OAuth
{
    public interface IToken
    {
        public string? AccessToken { get; }
        public string? RefreshToken { get; }
        public DateTime? Expiration { get; }
    }
}
