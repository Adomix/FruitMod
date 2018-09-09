using System;
using FruitMod.Attributes;
using System.Collections.Generic;

namespace FruitMod.Services
{
    [SetService]
    public class RatelimitService
    {
        public Dictionary<ulong, bool> rlb = new Dictionary<ulong, bool>();
        public int time { get; set; } = 0;
        public Dictionary<(ulong, ulong), DateTime> msgdict = new Dictionary<(ulong, ulong), DateTime>();
    }
}