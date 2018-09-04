using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace FruitMod.Commands.AudioCommands
{
    [RequireContext(ContextType.Guild)]
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly AudioService _service;

        public AudioModule(AudioService service)
        {
            _service = service;
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("plays a song")]
        public async Task SongPlay([Remainder] string song)
        {
            await _service.JoinAsync(Context.User as SocketGuildUser, ((SocketGuildUser) Context.User).Guild.Id, song,
                Context.Channel as SocketTextChannel);
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Summary("pauses a song")]
        public async Task SongPause()
        {
            await _service.PauseAsync();
        }

        [Command("resume", RunMode = RunMode.Async)]
        [Summary("resumes a paused song")]
        public async Task SongResume()
        {
            await _service.ResumeAsync();
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Summary("skips the current song")]
        public async Task SongSkip()
        {
            await _service.SkipAsync(Context.Guild.Id);
        }

        [Command("disconnect", RunMode = RunMode.Async)]
        [Summary("disconnects the bot")]
        public async Task SongDisconnect()
        {
            await _service.DisconnectAsync();
        }

        [Command("volume", RunMode = RunMode.Async)]
        [Summary("Sets the bot's volume. 1-100")]
        public async Task Volume(uint volume)
        {
            await _service.SetVolumeAsync(volume);
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Summary("Shows the queue")]
        public async Task SeeQueue()
        {
            await _service.SeeQueue(Context.Guild.Id, Context.Channel);
        }
    }
}