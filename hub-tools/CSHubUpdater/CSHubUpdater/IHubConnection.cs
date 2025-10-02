
namespace CSHubUpdater
{
    public interface IHubConnection
    {
        ushort FwVersion { get; }
        ushort HubId { get; }
        ushort HwRevision { get; }
        double Voltage { get; }
        bool SafeFirmware { get; }
        ushort SafeFwVersion { get; }
        void Dispose();
        Task RestartHeadstage();
        Task UpdateFirmware(IHubBitFile file, IProgress<int> progress);
    }
}