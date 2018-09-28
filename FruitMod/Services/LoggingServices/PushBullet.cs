using Discord;
using FruitMod.Attributes;
using FruitMod.Services;
using PushbulletSharp;
using PushbulletSharp.Models.Requests;
using System;
using System.Linq;

namespace FruitMod
{
    [SetService]
    public class PushBullet
    {
        private readonly PushbulletClient _client;
        private readonly LoggingService _log;

        public PushBullet(PushbulletClient client, LoggingService log)
        {
            _client = client;
            _log = log;
        }

        public void SendNotification(string msg)
        {
            try
            {
                var device = _client.CurrentUsersDevices().Devices.Where(x => x.Manufacturer.Equals("Samsung", StringComparison.OrdinalIgnoreCase)).First();
                if (device is null)
                {
                    var exception = new LogMessage(LogSeverity.Warning, "PushBullet", "PB Device not found!");
                    _log.Log(exception);
                }

                PushNoteRequest request = new PushNoteRequest
                {
                    DeviceIden = device.Iden,
                    Title = "FruitMod Alert!",
                    Body = msg
                };

                _client.PushNote(request);
            }
            catch (Exception e)
            {
                var exception = new LogMessage(LogSeverity.Error, "PushBullet", e.Message);
                _log.Log(exception);
            }
        }
    }
}
