using System;
using System.Collections.Generic;
using System.Linq;

namespace FruitMod.Extensions
{
    public static class MassRemove
    {
        public static void RemoveNext<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, int x)
        {
            if (x <= 0 || dictionary.Count < x) throw new ArgumentOutOfRangeException();

            for (var d = 0; d < x; x++) dictionary.Remove(dictionary.First().Key);
        }
    }
}