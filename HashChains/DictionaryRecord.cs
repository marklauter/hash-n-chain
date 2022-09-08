using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dictionaries.IO
{
    [DebuggerDisplay("NO:{NextRecordOffset}, H:{Hash}, KO:{KeyOffset}, KL:{KeyLength}, DO:{DataOffset}, DL:{DataLength}")]
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Size = 48, Pack = 1)]
    internal readonly struct DictionaryRecord : IEquatable<DictionaryRecord>
    {
        internal const long NullOffset = 0L;
        internal const int NextRecordOffsetFieldOffset = 0x00;
        internal const int HashFieldOffset = 0x08;
        internal const int KeyOffsetFieldOffset = 0x0C;
        internal const int KeyLengthFieldOffset = 0x14;
        internal const int DataOffsetFieldOffset = 0x18;
        internal const int DataLengthFieldOffset = 0x20;

        internal static DictionaryRecord Empty { get; } = new(
            DictionaryRecord.NullOffset,
            0,
            DictionaryRecord.NullOffset,
            0,
            DictionaryRecord.NullOffset,
            0);

        internal DictionaryRecord(
            long nextRecordOffset,
            uint hash,
            long keyOffset,
            int keyLength,
            long dataOffset,
            int dataLength)
        {
            this.NextRecordOffset = nextRecordOffset;
            this.Hash = hash;
            this.KeyOffset = keyOffset;
            this.KeyLength = keyLength;
            this.DataOffset = dataOffset;
            this.DataLength = dataLength;
        }

        [FieldOffset(0x00)] // 0
        public readonly long NextRecordOffset;

        [FieldOffset(0x08)] // 0 + long
        public readonly uint Hash;

        [FieldOffset(0x0C)] // 0 + long + int
        public readonly long KeyOffset;

        [FieldOffset(0x14)] // 0 + long + int + long
        public readonly int KeyLength;

        [FieldOffset(0x18)] // 0 + long + int + long + int
        public readonly long DataOffset;

        [FieldOffset(0x20)] // 0 + long + int + long + int + long
        public readonly int DataLength;

        public override bool Equals(object? obj)
        {
            return obj is DictionaryRecord record && this.Equals(record);
        }

        public bool Equals(DictionaryRecord other)
        {
            return this.NextRecordOffset == other.NextRecordOffset &&
                   this.Hash == other.Hash &&
                   this.DataOffset == other.DataOffset &&
                   this.DataLength == other.DataLength;
        }

        public static bool operator ==(DictionaryRecord left, DictionaryRecord right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DictionaryRecord left, DictionaryRecord right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.NextRecordOffset, this.Hash, this.DataOffset, this.DataLength);
        }
    }
}
