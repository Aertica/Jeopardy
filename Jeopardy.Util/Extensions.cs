using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Jeopardy.Util
{
    public static class Extensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list, int passes = 1)
        {
            var arr = list as T[] ?? list.ToArray();
            for (int i = 0; i < passes; i++)
                RandomNumberGenerator.Shuffle<T>(arr);
            return arr;
        }
    }
}
