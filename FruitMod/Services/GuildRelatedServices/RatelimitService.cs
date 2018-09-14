using System;
using System.Collections.Generic;
using FruitMod.Attributes;

namespace FruitMod.Services
{
    [SetService]
    public class RatelimitService
    {
        public Dictionary<(ulong, ulong), DateTime> msgdict = new Dictionary<(ulong, ulong), DateTime>();
        public Dictionary<ulong, bool> rlb = new Dictionary<ulong, bool>();
        public int time { get; set; } = 0;
    }
}