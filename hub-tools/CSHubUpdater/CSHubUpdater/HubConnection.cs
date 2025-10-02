using oni;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;


namespace CSHubUpdater
{
    internal abstract class HubConnection(string driver, int index, int portIndex) : IDisposable, IHubConnection, INotifyPropertyChanged
    {
        readonly oni.Context context = new(driver, index);
        readonly uint hubDeviceAddr = (((uint)portIndex + 1) << 8) + 254;
        readonly uint controlDeviceAddr = (uint)portIndex + 1;
        public ushort HubId { get; private set; }
        public ushort HwRevision { get; private set; }
        public double Voltage { get; private set; }
        public bool SafeFirmware { get; private set; }
        public ushort SafeFwVersion { get; private set; }
        public ushort FwVersion { get; private set; }

        const int MaxWriteWords = 16;
        const int TimeoutMs = 2000;

        public event PropertyChangedEventHandler? PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string? propertyName =null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        Task Init()
        {
            return Task.Run(async () =>
            {
                ResetHardware();
                context.Refresh();

                double? voltage = await PowerHub();
                if (!voltage.HasValue)
                {
                    SetVoltage(0);
                    throw new IOException("Unable to open hub");
                }
                Voltage = voltage.Value;

                //just to be sure
                context.Refresh();
                FillInfo();

            });
        }

        void FillInfo()
        {
            var hwid = context.ReadRegister(hubDeviceAddr, (uint)HubRegisterAddress.HWID);
            HubId = (ushort)hwid;
            OnPropertyChanged(nameof(HubId));
            HwRevision = (ushort)context.ReadRegister(hubDeviceAddr, (uint)HubRegisterAddress.HWREV);
            OnPropertyChanged(nameof(HwRevision));
            SafeFirmware = (hwid & 0x10000) != 0;
            OnPropertyChanged(nameof(SafeFirmware));
            SafeFwVersion = (ushort)context.ReadRegister(hubDeviceAddr, (uint)HubRegisterAddress.SAFEVER);
            OnPropertyChanged(nameof(SafeFwVersion));
            FwVersion = (ushort)context.ReadRegister(hubDeviceAddr, (uint)HubRegisterAddress.FWVER);
            OnPropertyChanged(nameof(FwVersion));
        }

        public async Task RestartHeadstage()
        {
               SetVoltage(3.3);
               await Task.Delay(100);
               SetVoltage(0);
               await Task.Delay(1000);
               SetVoltage(Voltage);
               await Task.Delay(500);
               FillInfo();
        }


        protected abstract Task<double?> PowerHub();

        protected bool CheckLinkState()
        {
            var linkstate = (LinkState)context.ReadRegister(controlDeviceAddr, (uint)ControlRegisterAddress.LINKSTATE);
            return (linkstate & LinkState.Lock) == LinkState.Lock;
        }

        protected void SetVoltage(double voltage)
        {
            if (voltage < 0 || voltage > 10)
            {
                throw new ArgumentException("Hub voltage must be between 0 and 10");
            }
            uint param = (uint)(voltage * 10);
            context.WriteRegister(controlDeviceAddr, (uint)ControlRegisterAddress.PWR, param); 
        }

        public void Dispose()
        {
            ResetHardware();
            context.Dispose();
        }

        // NB: In v2.0 of onix, this happens automatically on reset. 
        // But we add it here as a safety so the software can be used in earlier versions
        void ResetHardware()
        {
            SetVoltage(0);
            context.SetCustomOption((int)ONIXOption.PORTFUNC, 0);
        }

        public Task UpdateFirmware(IHubBitFile file, IProgress<int> progress)
        {
            return Task.Factory.StartNew(() =>
            {
                using var cancellation = new CancellationTokenSource();
                progress.Report(0);
                var data = MemoryMarshal.Cast<byte, uint>(file.Data.Span);
                WriteProgrammer(ProgrammingStatus.DISABLE);
                if (ReadProgrammer().HasFlag(ProgrammingStatus.BUSY))
                {
                    throw new IOException("Programmer in busy state. Try again in a few seconds or reboot the device");
                }
                bool isCrossLink = CheckCrossLinkAndSetSize(file.Data.Length);
                WriteProgrammer(ProgrammingStatus.ENABLE);
                ProgrammingStatus status;
                if (WaitProgrammer(cancellation, TimeoutMs).HasFlag(ProgrammingStatus.ERROR))
                {
                    throw new IOException("Error while clearing flash.");
                }

                int written = 0;
                while (written < data.Length)
                {
                    int toWrite = Math.Min(data.Length - written, MaxWriteWords);
                    for (int i = 0; i < toWrite; i++)
                    {
                        WriteProgrammerData(data[written++]);
                    }
                    WaitProgrammer(cancellation, TimeoutMs, isCrossLink);
                    progress.Report(written);

                }

                WriteProgrammer(ProgrammingStatus.DISABLE);
                WaitBusy(cancellation, TimeoutMs);



            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        }

        ProgrammingStatus WaitProgrammer(CancellationTokenSource cancellationToken, int msTimeout, bool crossLinkCheck = false)
        {
            cancellationToken.TryReset();
            cancellationToken.CancelAfter(msTimeout);
            var token = cancellationToken.Token;
            ProgrammingStatus status;
            ProgrammingStatus test = ProgrammingStatus.WAIT_DATA;
            if (crossLinkCheck) test |= ProgrammingStatus.DONE;
            while (((status = ReadProgrammer()) & test) == 0)
            {
                token.ThrowIfCancellationRequested();
            }
            return status;
        }
        void WaitBusy(CancellationTokenSource cancellationToken, int msTimeout)
        {
            cancellationToken.TryReset();
            cancellationToken.CancelAfter(msTimeout);
            var token = cancellationToken.Token;
            ProgrammingStatus test = ProgrammingStatus.BUSY;
            while ((ReadProgrammer() & test) != 0)
            {
                token.ThrowIfCancellationRequested();
            }
        }

        void WriteProgrammer(ProgrammingStatus val)
        {
            context.WriteRegister(hubDeviceAddr, (uint)HubRegisterAddress.PROGRAM, (uint)val);
        }

        ProgrammingStatus ReadProgrammer()
        {
            return (ProgrammingStatus)context.ReadRegister(hubDeviceAddr, (uint)HubRegisterAddress.PROGRAM);
        }

        void WriteProgrammerData(uint val)
        {
            context.WriteRegister(hubDeviceAddr, (uint)HubRegisterAddress.PROGRAM_DATA, val);
        }

        bool CheckCrossLinkAndSetSize(int len)
        {
            try
            {
                context.WriteRegister(hubDeviceAddr, (uint)HubRegisterAddress.PROGRAM_SIZE, (uint)len);
            }
            catch (Exception)
            {
                //NB: If there is an error here it just means this is not a crosslink device
                return false;
            }
            return true;
        }

        public static async Task<HubConnection> CreateFromHubInfoAsync(string driver, int index, int portIndex, uint hubId)
        {
            var assembly = Assembly.GetExecutingAssembly(); ;
            var deviceType = assembly.GetTypes().FirstOrDefault(t =>
            {
                var attr = t.GetCustomAttribute<HubIDAttribute>();
                if (attr == null) return false;
                if (attr.ID != hubId) return false;
                return t.IsClass && !t.IsAbstract && typeof(HubConnection).IsAssignableFrom(t);
            }) ?? throw new ArgumentException($"Hub ID not supported {hubId}");

            var hub = Activator.CreateInstance(deviceType, [driver, index, portIndex]) as HubConnection
                ?? throw new ArgumentException($"Unknown error initializating hub id {hubId}");
            try
            {
                await hub.Init();
                if (hub.HubId != hubId)
                {

                    throw new ArgumentException($"Invalid Hub detected. Expected {hubId}, Reported {hub.HubId}");
                }
            }
            catch (Exception)
            {
                hub.Dispose();
                throw;
            }
            return hub;

        }

        public static async Task<HubConnection> CreateFromVoltageAsync(string driver, int index, int portIndex, double voltage)
        {
            HubConnection hub = new HubFixedVoltage(driver, index, portIndex, voltage);
            try
            {
                await hub.Init();
            }
            catch (Exception)
            {
                hub.Dispose();
                throw;
            }
            return hub;
        }

        public enum ControlRegisterAddress : uint
        {
            PWR = 0x03,
            LINKSTATE = 0x05
        }

        [Flags]
        public enum LinkState : uint
        {
            None = 0,
            Pass = 1,
            Lock = 2
        }

        public enum HubRegisterAddress : uint
        {
            HWID = 0x00,
            HWREV = 0x01,
            FWVER = 0x02,
            SAFEVER = 0x03,
            CLOCK = 0x04,
            REBOOT = 0x10,
            PROGRAM = 0x20,
            PROGRAM_DATA = 0x21,
            PROGRAM_SIZE = 0x22 // only for Crosslink devices
        }

        [Flags]
        public enum ProgrammingStatus : uint
        {
            DISABLE = 0x00,
            ENABLE = 0x01,
            BUSY = 0x02,
            WAIT_DATA = 0x04,
            ERROR = 0x08,
            FULL = 0x10,
            DONE = 0x10000
        }

    }
}
