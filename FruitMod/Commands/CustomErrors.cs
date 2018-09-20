namespace FruitMod.Commands
{
    public class FruitErrors
    {
      public enum Reasons
      {
        NoGuild = "You must be in a guild to use this command!",
        WrongChannel = "You must use this command in the proper channel!",
        Blocked = "You have been blocked from the bot!",
        Banned = "You have been banned from the bot!",
        Zero = "Value must be > 0!"
      }
    }
}
