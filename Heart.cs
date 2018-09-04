using System.Drawing;
using System.Threading.Tasks;
using Colorful;
using FruitMod.Services;

namespace FruitMod
{
    public class Heart
    {
        private ConfigService _config;

        private static void Main()
        {
            new Heart().MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            Console.ForegroundColor = Color.Teal;
            Console.WriteLine(@"  ______          _ _   __  __           _ 
 |  ____|        (_) | |  \/  |         | |
 | |__ _ __ _   _ _| |_| \  / | ___   __| |
 |  __| '__| | | | | __| |\/| |/ _ \ / _` |
 | |  | |  | |_| | | |_| |  | | (_) | (_| |
 |_|  |_|   \__,_|_|\__|_|  |_|\___/ \__,_|
                                           
                                           ");

            _config = new ConfigService();
            await _config.LaunchAsync();
        }
    }
}