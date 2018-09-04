using System.Threading.Tasks;
using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Commands.AudioCommands;
using SharpLink;

namespace FruitMod.Services
{
    [SetService]
    public class SharplinkService
    {
        private readonly AudioService _audio;
        private readonly DiscordSocketClient _client;

        private readonly LavalinkManager _manager;

        public SharplinkService(LavalinkManager manager, AudioService audio, DiscordSocketClient client)
        {
            _manager = manager;
            _audio = audio;
            _client = client;
        }

        public void AudioInitialization()
        {
            _client.Ready += SharpLink;
        }

        // _client.Ready += SharpLink;
        private async Task SharpLink()
        {
            await _manager.StartAsync();
        }

        // _manager.TrackEnd += Dequeue;
        private async Task Dequeue(LavalinkPlayer player, LavalinkTrack track, string id)
        {
            await _audio.Dequeued(player, track, id);
        }
    }
}