using System;
using System.Collections.Generic;

namespace FruitMod.Extensions
{
    public static class MassDequeue
    {
        public static IEnumerable<T> Dequeue<T>(this Queue<T> queue, int x)
        {
            if(x > int.MaxValue || x > queue.Count) { throw new ArgumentOutOfRangeException(); }
            for(int d = 0; d < x && queue.Count > 0; d++)
            {
                yield return queue.Dequeue();
            }
        }
    }
}