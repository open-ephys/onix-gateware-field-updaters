
namespace CSHubUpdater
{
    public interface IHubConnection : IDisposable
    {
        ushort FwVersion { get; }
        ushort HubId { get; }
        ushort HwRevision { get; }
        double Voltage { get; }
        bool SafeFirmware { get; }
        ushort SafeFwVersion { get; }
        Task RestartHeadstage();
        Task UpdateFirmware(IHubBitFile file, IProgress<int> progress);
    }
}