using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jeopardy.Bots.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public ulong? ID { get; }

        public UnauthorizedException() { }

        public UnauthorizedException(ulong id)
        {
            ID = id;
        }

        public UnauthorizedException(ulong id, string message)
            : base(message)
        {
            ID = id;
        }

        public UnauthorizedException(ulong id, string message, Exception inner)
            : base(message, inner)
        {
            ID = id;
        }
    }
}
