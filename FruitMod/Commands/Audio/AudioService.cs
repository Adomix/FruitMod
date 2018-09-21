using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Database;
using FruitMod.Objects;
using SharpLink;

namespace FruitMod.Commands.AudioCommands
{
    [SetService]
    public class AudioService
    {
        private readonly DbService _db;
        private readonly LavalinkManager _manager;
        private LavalinkPlayer _player;
        private SocketTextChannel _userchannel;

        public AudioService(LavalinkManager manager, DbService db)
        {
            _manager = manager;
            _db = db;
        }

        ~AudioService()
        {
            Console.WriteLine("Player destructed!");
        }

        public async Task JoinAsync(SocketGuildUser user, ulong guildId, string title, SocketTextChannel channel)
        {
            var dbo = _db.GetById<GuildObjects>(guildId);
            _userchannel = channel;

            if (user == null)
                await channel.SendMessageAsync("You must be in a voice channel!");
            if (_player == null)
                _player = await _manager.JoinAsync((user as IVoiceState).VoiceChannel);
            else if (_player.Playing == false) await _manager.JoinAsync((user as IVoiceState).VoiceChannel);

            string id = "ytsearch:";

            if(title.Contains("sc") && title.IndexOf("sc") < 5)
            {
                id = "scsearch:";
            }

                var tracks = await _manager.GetTracksAsync($"{id} {title}");
            var track = tracks.Tracks.ElementAt(0);
            if (track == null)
                await channel.SendMessageAsync("Track could not be found!");
            else
                await channel.SendMessageAsync(
                    $"Track {track.Title} by {track.Author} has been found. Length: {track.Length}");
            dbo.MusicQueue.Enqueue(track.Title);
            _db.StoreObject(dbo, guildId);
            await PlayAsync(guildId, channel, _player);
        }

        public async Task PlayAsync(ulong guildId, SocketTextChannel channel, LavalinkPlayer player)
        {
            var dbo = _db.GetById<GuildObjects>(guildId);
            if (!_player.Playing)
            {
                if (dbo.MusicQueue.TryDequeue(out var track))
                {
                    var newTracks = await _manager.GetTracksAsync($"ytsearch: {track}");
                    var newTrack = newTracks.Tracks.ElementAt(0);
                    await channel.SendMessageAsync($"Track: {newTrack.Title} {newTrack.Author}");
                    await player.PlayAsync(newTrack);
                    var songEmbed = new EmbedBuilder()
                        .WithTitle("Now playing")
                        .AddField("Title: ", newTrack.Title)
                        .AddField("Author: ", newTrack.Author)
                        .AddField("Duration: ", newTrack.Length)
                        .WithUrl(newTrack.Url)
                        .WithThumbnailUrl(@"https://mangobestfruit.s-ul.eu/Gb81hYnM")
                        .WithColor(Color.Red)
                        .Build();
                    await _userchannel.SendMessageAsync(string.Empty, false, songEmbed);
                    _db.StoreObject(dbo, guildId);
                }
                else
                {
                    await channel.SendMessageAsync("Track failed! Is the queue empty?");
                }
            }
        }

        public async Task CheckQueue(ulong guildId)
        {
            var dbo = _db.GetById<GuildObjects>(guildId);
            if (dbo.MusicQueue.TryPeek(out var track))
            {
                await PlayAsync(guildId, _userchannel, _player);
            }
            else
            {
                await _userchannel.SendMessageAsync("Queue is empty! Disconnecting....");
                await DisconnectAsync();
            }
        }

        public async Task Dequeued(LavalinkPlayer player, LavalinkTrack track, string dequeuedtitle)
        {
            await CheckQueue(_player.VoiceChannel.Guild.Id);
        }

        // Now for the extra tasks

        public async Task PauseAsync()
        {
            await _player.PauseAsync();
            await _userchannel.SendMessageAsync("Song paused!");
        }

        public async Task ResumeAsync()
        {
            await _player.ResumeAsync();
            await _userchannel.SendMessageAsync("Song resumed!");
        }

        public async Task SkipAsync(ulong guildId)
        {
            await _player.StopAsync();
            await _userchannel.SendMessageAsync("Song skipped!");
            await CheckQueue(guildId);
        }

        public async Task DisconnectAsync()
        {
            await _player.StopAsync();
            await _player.DisconnectAsync();
        }

        public async Task SetVolumeAsync(uint volume)
        {
            await _player.SetVolumeAsync(volume);
        }

        public async Task SeeQueue(ulong guildId, ISocketMessageChannel channel)
        {
            var dbo = _db.GetById<GuildObjects>(guildId);
            var myqueue = dbo.MusicQueue;
            var tracks = new List<string>();
            foreach (var id in myqueue)
            {
                var track = await _manager.GetTracksAsync($"ytsearch: {id}");
                tracks.Add(track.Tracks.ElementAt(0).Title);
            }

            await channel.SendMessageAsync($"Current Queue:\n```\n{string.Join("\n", tracks)}```");
        }
    }
}