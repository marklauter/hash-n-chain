using Newtonsoft.Json;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Dictionaries.IO
{

    public partial class StreamDictionary<TValue>
        : IStreamDictionary<TValue>
    {
        public TValue this[string key]
        {
            get => this.ReadValue(key);
            set => throw new NotImplementedException();
        }

        // todo: consider not supporting keys & values because the list could be /big/
        // todo: if not supported then suggest using ReadKeys and ReadValues with a continuation token 
        public ICollection<string> Keys => this.ReadKeys().ToList();
        public ICollection<TValue> Values => throw new NotImplementedException();

        public int Count { get; private set; }
        public bool IsReadOnly { get; }

        public void Add(string key, TValue value)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), $"'{nameof(key)}' cannot be null or empty.");
            }

            if (this.IsReadOnly)
            {
                throw new NotSupportedException("dictionary is readonly");
            }

            var (hash, bucket) = StableHash.GetHashBucket(key, PrehashLength, this.bucketCount);
            var bucketOffset = this.CalculateBucketOffset(bucket);

            if (this.ReadRecord(bucketOffset) != DictionaryRecord.Empty
                && this.FindKey(key) != DictionaryRecord.NullOffset)
            {
                throw new ArgumentException($"An item with the same key has already been added. {nameof(key)}: {key}", nameof(key));
            }

            var (recordOffset, lastRecord) = this.GetLastRecordInBucket(bucket);
            var nextOffset = recordOffset;

            if (lastRecord != DictionaryRecord.Empty)
            {
                nextOffset = this.stream.Length;
                lastRecord = new DictionaryRecord(
                    nextOffset,
                    lastRecord.Hash,
                    lastRecord.KeyOffset,
                    lastRecord.KeyLength,
                    lastRecord.DataOffset,
                    lastRecord.DataLength);
                this.WriteRecord(lastRecord, recordOffset);
            }

            var keyOffset = Math.Max(nextOffset, this.stream.Length)
                + this.recordSize;
            var dataOffset = keyOffset + key.Length;
            var data = JsonConvert.SerializeObject(value);

            var newRecord = new DictionaryRecord(
                DictionaryRecord.NullOffset,
                hash,
                keyOffset,
                key.Length,
                dataOffset,
                data.Length);

            this.WriteRecord(newRecord, nextOffset);
            this.WriteString(key, keyOffset);
            this.WriteString(data, dataOffset);

            this.Count++;
            this.WriteCount();
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return String.IsNullOrEmpty(key)
                ? throw new ArgumentNullException(nameof(key), $"'{nameof(key)}' cannot be null or empty.")
                : this.FindKey(key) != DictionaryRecord.NullOffset;
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            return this.Remove(item.Key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
