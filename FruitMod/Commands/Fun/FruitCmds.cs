using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FruitMod.Database;
using FruitMod.Objects;
using FruitMod.Objects.DataClasses;

namespace FruitMod.Commands.FunCommands
{
    [RequireContext(ContextType.Guild)]
    public class FruitCmds : ModuleBase<FruitModContext>
    {
        private static readonly Dictionary<(ulong, ulong), DateTime> feedback =
            new Dictionary<(ulong, ulong), DateTime>();

        private readonly DbService _db;
        private readonly Random _random;

        public FruitCmds(Random random, DbService db)
        {
            _random = random;
            _db = db;
        }

        [Command("fruits")]
        [Summary("Shows you your fruits!!")]
        public async Task ShowFruits()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserStruct.ContainsKey(Context.User.Id))
            {

                dbo.UserStruct.Add(Context.User.Id,
                    new UserStruct
                    { UserId = Context.User.Id, Warnings = new Dictionary<int, string>(), Mangos = 0 });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            await ReplyAsync($"You have:\n{dbo.UserStruct[Context.User.Id].Mangos} mangos!");
        }

        [Command("daily")]
        [Summary("Awards you your daily amount of fruit!")]
        public async Task Daily()
        {
            if (feedback.ContainsKey((Context.Guild.Id, Context.User.Id)) &&
                feedback[(Context.Guild.Id, Context.User.Id)].Date == DateTime.Now.Date)
            {
                await ReplyAsync("You have already redeemed your daily today!");
                return;
            }
            else if(feedback.ContainsKey((Context.Guild.Id, Context.User.Id)) &&
                feedback[(Context.Guild.Id, Context.User.Id)].Date != DateTime.Now.Date)
            {
                feedback.Remove((Context.Guild.Id, Context.User.Id));
            }

            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserStruct.ContainsKey(Context.User.Id))
            {

                dbo.UserStruct.Add(Context.User.Id,
                    new UserStruct
                    { UserId = Context.User.Id, Warnings = new Dictionary<int, string>(), Mangos = 0 });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            feedback.Add((Context.Guild.Id, Context.User.Id), DateTime.Now);
            var amount = _random.Next(10, 51);
            dbo.UserStruct[Context.User.Id].Mangos += amount;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have been given {amount} mangos!");
        }

        [Command("give")]
        [Summary("Gives someone x of your fruits! Usage: give <amount> <user>")]
        public async Task GiveFruits(int amount, IUser user)
        {
            if (user.Id == Context.User.Id)
            {
                await ReplyAsync("You can not give yourself your fruit!");
                return;
            }

            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);

            if (!dbo.UserStruct.ContainsKey(Context.User.Id))
            {
                await ReplyAsync("You have no fruits!");
                return;
            }

            if (dbo.UserStruct[Context.User.Id].Mangos == 0)
            {
                await ReplyAsync($"You do not have any mangos!");
                return;
            }

            if (!dbo.UserStruct.ContainsKey(user.Id))
            {
                dbo.UserStruct.Add(user.Id,
                    new UserStruct { UserId = user.Id, Warnings = new Dictionary<int, string>(), Mangos = 0 });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            var invokersFruits = dbo.UserStruct[Context.User.Id].Mangos;
            var receiversFruits = dbo.UserStruct[user.Id].Mangos;

            if (amount <= 0)
            {
                await ReplyAsync("Amount must be greater than 0!");
                return;
            }

            if (amount > invokersFruits)
            {
                await ReplyAsync($"You do not have enough mangos!");
                return;
            }

            invokersFruits -= amount;
            receiversFruits += amount;

            dbo.UserStruct[Context.User.Id].Mangos = invokersFruits;
            dbo.UserStruct[user.Id].Mangos = receiversFruits;

            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have successfully given {user} {amount} mangos!");
        }
    }
}