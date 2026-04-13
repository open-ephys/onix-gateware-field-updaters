using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Threading;
using System.Runtime.CompilerServices;

namespace CSHubUpdater
{
    internal class HubBitFile : IHubBitFile
    {
        const string HeaderSignature = "ONIXHUBX";
        const ushort HeaderVersion = 1;
        private static readonly int HeaderSize = Unsafe.SizeOf<BitFileHeader>();

        const int MaxFileSize = 5 * 1024 * 1024; // Anything beyond this size will result in an error

        public ushort HubId => header.HardwareID;
        public ushort HwRevision => header.HardwareRev;
        public ushort FwVer => header.FwVersion;
        public ReadOnlyMemory<byte> Data { get; }

        readonly BitFileHeader header;

        private HubBitFile(BitFileHeader header, byte[] data)
        {
            this.header = header;
            Data = data;
        }

        public static async Task<IHubBitFile> CreateAsync(FileInfo info, CancellationToken cancellationToken)
        {
            if (info == null || !info.Exists)
            {
                throw new FileNotFoundException("Selected file does not exist");
            }
            if (info.Length > MaxFileSize)
            {
                throw new ArgumentException("File size is greater than supported");
            }

            await using var stream = info.OpenRead();
            byte[] headerBuffer = new byte[HeaderSize];
            await stream.ReadExactlyAsync(headerBuffer, cancellationToken);
            var header = MemoryMarshal.Read<BitFileHeader>(headerBuffer);
            ValidateHeader(header, stream.Length);

            int dataLength = (int)header.Length;
            int paddedSize = (dataLength + 3) & ~3;
            byte[] dataBuffer = new byte[paddedSize];
            Array.Fill(dataBuffer, (byte)0xFF);
            await stream.ReadExactlyAsync(dataBuffer.AsMemory(0, dataLength), cancellationToken);
            byte[] computedHash = SHA1.HashData(new ReadOnlySpan<byte>(dataBuffer, 0, dataLength));
            if (!header.Sha1HashSpan.SequenceEqual(computedHash))
            {
                throw new InvalidDataException("Hashes do not match. Possible file corruption");
            }

            cancellationToken.ThrowIfCancellationRequested();
            return new HubBitFile(header, dataBuffer);
        }

        static void ValidateHeader(BitFileHeader header, long streamLength)
        {
            if (!header.SignatureSpan.SequenceEqual(System.Text.Encoding.ASCII.GetBytes(HeaderSignature)))
            {
                throw new InvalidDataException("Invalid firmware file (Invalid header).");
            }

            if (header.Version != HeaderVersion)
            {
                throw new InvalidDataException($"Unextpected file version. Expected {HeaderVersion} but reported {header.Version}.");
            }

            if ((ulong)streamLength < (header.Length + (ulong)HeaderSize))
            {
                throw new InvalidDataException("File size is smaller than reported.");
            }
        }



        [StructLayout(LayoutKind.Explicit, Size = 64, Pack = 1)]
        unsafe struct BitFileHeader
        {
            [FieldOffset(0)]
            public fixed byte Signature[8];
            [FieldOffset(8)]
            public ushort Version;
            [FieldOffset(10)]
            public ushort HardwareID;
            [FieldOffset(12)]
            public ushort HardwareRev;
            [FieldOffset(14)]
            public ushort FwVersion;
            [FieldOffset(16)]
            public ulong Length;
            [FieldOffset(24)]
            public fixed byte Sha1Hash[20];
            [FieldOffset(44)]
            public fixed byte Reserved[20];

            public ReadOnlySpan<byte> SignatureSpan
            {
                get
                {
                    fixed (byte* ptr = Signature)
                    {
                        return new ReadOnlySpan<byte>(ptr, 8);
                    }
                }
            }
            public ReadOnlySpan<byte> Sha1HashSpan
            {
                get
                {
                    fixed (byte* ptr = Sha1Hash)
                    {
                        return new Span<byte>(ptr, 20);
                    }
                }

            }
        }


    }
}
