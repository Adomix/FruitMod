using FruitMod.Attributes;
using FruitMod.Services;
using PushBulletNet;
using PushBulletNet.POST;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FruitMod
{
    [SetService]
    public class PushBullet
    {
        private readonly PBClient _client;
        private readonly LoggingService _log;

        public PushBullet(PBClient client, LoggingService log)
        {
            _client = client;
            _log = log;
        }

        public async Task SendNotificationAsync(string msg)
        {
            PushRequest push = new PushRequest
            {
                TargetDeviceIdentity = _client.UserDevices.Devices.FirstOrDefault(x => x.Manufacturer.Equals("Samsung", StringComparison.OrdinalIgnoreCase)).Iden,
                Title = "FruitMod Alert!",
                Content = msg
            };

            await _client.PushRequestAsync(push);
        }
    }
}