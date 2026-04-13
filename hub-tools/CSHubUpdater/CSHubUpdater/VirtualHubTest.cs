using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CSHubUpdater
{
    internal class VirtualHubTest : IHubConnection
    {
        public ushort FwVersion => 3;

        public ushort HubId => 2;

        public ushort HwRevision => 259;

        public double Voltage => 4.2;

        public bool SafeFirmware => false;

        public ushort SafeFwVersion => 0;

        public void Dispose()
        {
            Debug.WriteLine("hub dispose");
        }

        public async Task RestartHeadstage()
        {
            await Task.Delay(1000);
            Debug.WriteLine("hub restart");
        }

        public Task UpdateFirmware(IHubBitFile file, IProgress<int> progress)
        {
            return Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < file.Data.Length / 4; i += 100)
                {
                    Thread.Sleep(100);
                    progress.Report(i);
                //    if (i > file.Data.Length / 8) throw new IOException("BBB");
                }
              //  throw new IOException("AAA");
            });
        }

        public static async Task<VirtualHubTest> CreateFromHubInfoAsync(string driver, int index, int portIndex, uint hubId)
        {
            Console.WriteLine($"{driver}: {index} - {portIndex} - {hubId}");
            var obj = new VirtualHubTest();
            await obj.Init();
            return obj;
        }

        Task Init()
        {
            return Task.Run(async () =>
            {
                Debug.WriteLine("Starting");
                await Task.Delay(2000);
            });
        }
    }
}
