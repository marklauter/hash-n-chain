using System.Diagnostics;

namespace HashChains
{
    [DebuggerDisplay("NO:{NextOffset}, H:{Hash}, KO:{KeyOffset}, KL:{KeyLength}, DO:{DataOffset}, DL:{DataLength}")]
    // todo: set the struct to fixed and set the relative addresses for each field
    internal readonly struct HashRecord : IEquatable<HashRecord>
    {
        internal const long NullOffset = 0L;
        internal static HashRecord Empty => new(
            HashRecord.NullOffset,
            0,
            HashRecord.NullOffset,
            0,
            HashRecord.NullOffset,
            0);

        internal HashRecord(
            long nextOffset,
            uint hash,
            long keyOffset,
            int keyLength,
            long dataOffset,
            int dataLength)
        {
            this.NextOffset = nextOffset;
            this.Hash = hash;
            this.KeyOffset = keyOffset;
            this.KeyLength = keyLength;
            this.DataOffset = dataOffset;
            this.DataLength = dataLength;
        }

        public readonly long NextOffset; // next record
        public readonly uint Hash; // hash value
        public readonly long KeyOffset;
        public readonly int KeyLength;
        public readonly long DataOffset; // points to offset of data in some other file
        public readonly int DataLength;

        public override bool Equals(object? obj)
        {
            return obj is HashRecord record && this.Equals(record);
        }

        public bool Equals(HashRecord other)
        {
            return this.NextOffset == other.NextOffset &&
                   this.Hash == other.Hash &&
                   this.DataOffset == other.DataOffset &&
                   this.DataLength == other.DataLength;
        }

        public static bool operator ==(HashRecord left, HashRecord right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HashRecord left, HashRecord right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.NextOffset, this.Hash, this.DataOffset, this.DataLength);
        }
    }
}
