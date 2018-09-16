using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace FruitMod.Commands
{
    public class FruitModContext : ICommandContext
    {
        public FruitModContext(DiscordSocketClient client, IUserMessage message)
        {
            Client = client;
            Guild = (message.Channel as SocketGuildChannel)?.Guild;
            GuildChannel = message.Channel as SocketGuildChannel;
            Channel = message.Channel as SocketTextChannel;
            DmChannel = message.Channel as SocketChannel;
            User = message.Author as SocketUser;
            GuildUser = message.Author as SocketGuildUser;
            SMessage = message as SocketUserMessage;
            Message = message;
        }

        public DiscordSocketClient Client { get; }
        public SocketGuild Guild { get; }
        public SocketGuildChannel GuildChannel { get; }
        public SocketTextChannel Channel { get; }
        public SocketChannel DmChannel { get; }
        public SocketUser User { get; }
        public SocketGuildUser GuildUser { get; }
        public SocketUserMessage SMessage { get; }
        public IUserMessage Message { get; }

        IDiscordClient ICommandContext.Client => Client;
        IGuild ICommandContext.Guild => Guild;
        IMessageChannel ICommandContext.Channel => Channel;
        IUser ICommandContext.User => User;
        IUserMessage ICommandContext.Message => Message;
    }
}