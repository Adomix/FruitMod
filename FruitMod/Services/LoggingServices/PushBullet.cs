using FruitMod.Attributes;
using FruitMod.Services;
using PushBulletNet;
using PushBulletNet.PushBullet.Models;
using System.Threading.Tasks;

namespace FruitMod
{
    [SetService]
    public class PushBullet
    {
        private readonly PushBulletClient _client;
        private readonly PushBulletDevice _device;
        private readonly LoggingService _log;

        public PushBullet(PushBulletClient client, PushBulletDevice device, LoggingService log)
        {
            _client = client;
            _device = device;
            _log = log;
        }

        public async Task SendNotificationAsync(string msg)
        {
            await _client.PushAsync("FruitMod Alert!", msg, _device.Iden);
        }
    }
}