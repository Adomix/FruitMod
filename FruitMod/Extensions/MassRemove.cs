using System;
using System.Linq;
using System.Collections.Generic;

namespace FruitMod.Extensions
{
    public static class MassRemove
    {
        public static void RemoveNext<TKey, TValue> (this SortedDictionary<TKey, TValue> dictionary, uint x)
        {
            if(x > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            for (int d = 0; d < x && dictionary.Count > 0; x++)
            {
                dictionary.Remove(dictionary.FirstOrDefault().Key);
            }
        }
    }
}