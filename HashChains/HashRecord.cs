using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HashChains
{
    internal class HashStream
        : IHashStream
    {
        private readonly Stream stream;
        private readonly uint bucketCount;
        private readonly int recordSize;

        public HashStream(
            Stream stream,
            uint bucketCount)
            : this(stream, bucketCount, false)
        {
        }

        public HashStream(
            Stream stream,
            uint bucketCount,
            bool isReadOnly)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.bucketCount = bucketCount;
            this.IsReadOnly = isReadOnly;
            this.recordSize = Marshal.SizeOf(typeof(HashRecord));
        }

        public string this[uint key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ICollection<uint> Keys => throw new NotImplementedException();
        public ICollection<string> Values => throw new NotImplementedException();
        public int Count => throw new NotImplementedException();
        public bool IsReadOnly { get; }

        public void Add(uint key, string value)
        {
            // 1. get prehash of value
            // 2. get bucket 
            var (_, bucket) = StableHash.GetHashBucket(value, 3, this.bucketCount);

            // 3. calc stream offset - it's bucket * sizeof(HashRecord)
            _ = bucket * this.recordSize;

            // 4. read from that offset location
            // 4.a if it's null create and write a hash record
            // 4.b if it's not null then traverse the linked list by reading the offset attributes from each record. update the final item in the list with the new item's offset (length of stream) and then write the new item to end of file.

            // todo: experiment with BinaryFormatter, implement ISerializable, or just use BinaryWriter like the sample code from graph project
            // more info: https://stackoverflow.com/questions/628843/byte-for-byte-serialization-of-a-struct-in-c-sharp
            //var x = new BinaryFormatter()
        }

        public void Add(KeyValuePair<uint, string> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<uint, string> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(uint key)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<uint, string>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<uint, string>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(uint key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<uint, string> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(uint key, [MaybeNullWhen(false)] out string value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }


    public interface IHashStream
        : IDictionary<uint, string>
    {
    }

    [DebuggerDisplay("{Offset}, {Value}")]
    internal readonly struct HashRecord
    {
        internal HashRecord(
            long previousOffset,
            long nextOffset,
            uint value)
        {
            this.PreviousOffset = previousOffset;
            this.NextOffset = nextOffset;
            this.Value = value;
        }

        public readonly long PreviousOffset;
        public readonly long NextOffset;
        public readonly uint Value;
    }
}
