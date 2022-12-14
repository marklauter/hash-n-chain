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
            set => this.SetValue(key, value);
        }

        public ICollection<string> Keys => this.ReadKeys().ToArray();

        public ICollection<TValue> Values => this.ReadValues().ToArray();

        private int count;

        public int Count
        {
            get => this.count;
            private set
            {
                if (this.count != value)
                {
                    this.count = value;
                    this.WriteCount();
                }
            }
        }

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

            var (hash, bucket) = StableHash.GetHashBucket(key, this.prehashLength, this.BucketCount);
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
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            if (this.IsReadOnly)
            {
                throw new NotSupportedException("dictionary is readonly");
            }

            // zero writes the bucket heads
            this.InitializeStream();
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            var offset = this.FindKey(item.Key);
            if (offset == DictionaryRecord.NullOffset)
            {
                return false;
            }
            else
            {
                var (dataOffset, dataLength) = this.ReadDataMetaData(offset);
                var valueJson = JsonConvert.SerializeObject(item.Value);
                var storedjson = this.ReadString(dataOffset, dataLength);
                return storedjson.Equals(valueJson, StringComparison.Ordinal);
            }
        }

        public bool ContainsKey(string key)
        {
            return String.IsNullOrEmpty(key)
                ? throw new ArgumentNullException(nameof(key), $"'{nameof(key)}' cannot be null or empty.")
                : this.FindKey(key) != DictionaryRecord.NullOffset;
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Length - arrayIndex < this.Count)
            {
                throw new ArgumentException("Array too short.", nameof(array));
            }

            foreach (var kvp in this.ReadKeyValuePairs())
            {
                array[arrayIndex++] = kvp;
            }
        }

        public bool Remove(string key)
        {
            return String.IsNullOrEmpty(key)
                ? throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key))
                : this.IsReadOnly
                    ? throw new NotSupportedException("dictionary is readonly")
                    : this.RemoveInternal(key);
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            return this.Remove(item.Key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
        {
            try
            {
                value = this.ReadValue(key);
                return true;
            }
            catch (KeyNotFoundException)
            {
                value = default;
                return false;
            }
            catch
            {
                throw;
            }
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return this.ReadKeyValuePairs().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
