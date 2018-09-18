using System;
using System.Collections.Generic;
using System.Linq;

namespace FruitMod.Extensions
{
    public static class MassRemove
    {
        public static void RemoveNext<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, int x)
        {
            if (x <= 0) throw new ArgumentOutOfRangeException();

            for (var d = 0; d < x && dictionary.Count > 0; x++) dictionary.Remove(dictionary.First().Key);
        }
    }
}