using System;
using System.Threading.Tasks;
using Discord;
using FruitMod.Attributes;
using Color = System.Drawing.Color;
using Console = Colorful.Console;

namespace FruitMod.Services
{
    [SetService]
    public class LoggingService
    {
        public Task Log(LogMessage message)
        {
            var sev = message.Severity;
            switch (sev)
            {
                case LogSeverity.Debug:
                    Console.ForegroundColor = Color.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = Color.Lime;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = Color.IndianRed;
                    break;
                case LogSeverity.Critical:
                    Console.ForegroundColor = Color.Red;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = Color.DodgerBlue;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = Color.OrangeRed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Console.Write($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] [{sev,-8}]");
            Console.ResetColor();
            Console.WriteLine($" {message.Message}");
            return Task.CompletedTask;
        }

        public Task LavalinkLog(LogMessage message)
        {
            Console.ResetColor();
            Console.ForegroundColor = Color.Gold;
            Console.Write($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] [{"Lava",-8}]");
            Console.ResetColor();
            Console.WriteLine($" {message.Message}");
            return Task.CompletedTask;
        }
    }
}