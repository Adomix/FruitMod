using System.Collections.Generic;

namespace FruitMod.Extensions
{
    public static class MassDequeue
    {
        public static IEnumerable<T> Dequeue<T>(this Queue<T> queue, int x)
        {
            for(int d = 0; d < x && queue.Count > 0; x++)
            {
                yield return queue.Dequeue();
            }
        }
    }
}