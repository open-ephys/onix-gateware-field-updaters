
namespace CSHubUpdater
{
    public interface IHubBitFile
    {
        ReadOnlyMemory<byte> Data { get; }
        ushort FwVer { get; }
        ushort HubId { get; }
        ushort HwRevision { get; }
    }
}