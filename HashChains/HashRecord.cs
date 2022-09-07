using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace HashChains
{
    internal class HashStream<TValue>
        : IHashStream<TValue>
    {
        private readonly int bucketCount;

        public HashStream(int bucketCount)
            : this(bucketCount, false)
        {
        }

        public HashStream(
            int bucketCount,
            bool isReadOnly)
        {
            this.bucketCount = bucketCount;
            this.IsReadOnly = isReadOnly;
        }

        public TValue this[uint key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ICollection<uint>? Keys { get; }
        public ICollection<TValue>? Values { get; }
        public int Count { get; }
        public bool IsReadOnly { get; }

        public void Add(uint key, TValue value)
        {
            // 1. get prehash of value
            // 2. get bucket 
            // 3. calc stream offset - it's bucket * sizeof(HashRecord)
            // 4. read location
            // 4.a if it's null create and write a hash record
            // 4.b if it's not null then traverse the linked list by reading the offset attributes from each record. update the final item in the list with the new item's offset (length of stream) and then write the new item to end of file.

            // todo: experiment with BinaryFormatter, implement ISerializable, or just use BinaryWriter like the sample code from graph project
            //var x = new BinaryFormatter()
        }


        public void Add(KeyValuePair<uint, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<uint, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(uint key)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<uint, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<uint, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(uint key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<uint, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(uint key, [MaybeNullWhen(false)] out TValue value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }


    public interface IHashStream<TValue>
        : IDictionary<uint, TValue>
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
