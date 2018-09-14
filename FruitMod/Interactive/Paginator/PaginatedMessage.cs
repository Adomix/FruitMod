using System.Collections.Generic;
using Discord;
using Colour = Discord.Color;

namespace FruitMod.Interactive.Paginator
{
    public class PaginatedMessage
    {
        public IEnumerable<object> Pages { get; set; }

        public string Content { get; set; } = "";

        public EmbedAuthorBuilder Author { get; set; } = null;
        public Colour Color { get; set; } = Colour.Default;
        public string Title { get; set; } = "";

        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;
    }
}