using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Utilities
{
    /// <summary>
    /// List utilities
    /// </summary>
    internal static class ListUtilities
    {
       /// <summary>
       /// Sorts list randomly
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="list"></param>
        public static void SortRandom<T>(this List<T> list)
        {
            if (list.Count < 2) return;

            var random = new Random();
            var index = list.Count;
            while (index > 1)
            {
                index--;
                var k = random.Next(index + 1);
                var value = list[k];
                list[k] = list[index];
                list[index] = value;
            }
        }
    }
}
