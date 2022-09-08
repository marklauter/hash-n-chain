using Newtonsoft.Json;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace HashChains
{
    internal class HashStream<TValue>
        : IHashStream<TValue>
        , IDisposable
    {
        private const long CountOffset = 0;
        private const long BucketCountOffset = sizeof(int);
        private const long FirstBucketOffset = sizeof(int) * 2;

        private readonly Stream stream;
        private readonly BinaryWriter writer;
        private readonly BinaryReader reader;
        private readonly uint bucketCount;
        private readonly int recordSize;
        private bool disposedValue;

        public HashStream(
            Stream stream,
            uint bucketCount)
            : this(stream, bucketCount, false)
        {
        }

        public unsafe HashStream(
            Stream stream,
            uint bucketCount,
            bool isReadOnly)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (!stream.CanSeek)
            {
                throw new ArgumentException("can't seek", nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException("can't read", nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException("can't write", nameof(stream));
            }

            this.bucketCount = bucketCount;
            this.IsReadOnly = isReadOnly;
            this.writer = new BinaryWriter(stream, Encoding.UTF8, true);
            this.reader = new BinaryReader(stream, Encoding.UTF8, true);
            this.recordSize = sizeof(HashRecord);
            if (stream.Length == 0)
            {
                this.InitializeStream();
            }
        }

        public TValue this[string key]
        {
            get => this.ReadValue(key);
            set => throw new NotImplementedException();
        }

        public ICollection<string> Keys => throw new NotImplementedException();
        public ICollection<TValue> Values => throw new NotImplementedException();
        public int Count { get; private set; }
        public bool IsReadOnly { get; }

        public void Add(string key, TValue value)
        {
            var (hash, bucket) = StableHash.GetHashBucket(key, 3, this.bucketCount);
            (var recordOffset, var lastRecord) = this.GetLastRecordInChain(bucket);

            var nextOffset = recordOffset;
            var keyOffset = nextOffset + this.recordSize;
            var dataOffset = keyOffset + key.Length;
            var data = JsonConvert.SerializeObject(value);

            var newRecord = new HashRecord(
                HashRecord.NullOffset,
                hash,
                keyOffset,
                key.Length,
                dataOffset,
                data.Length);

            if (lastRecord != HashRecord.Empty)
            {
                nextOffset = this.stream.Length;
                lastRecord = new HashRecord(
                    nextOffset,
                    lastRecord.Hash,
                    lastRecord.KeyOffset,
                    lastRecord.KeyLength,
                    lastRecord.DataOffset,
                    lastRecord.DataLength);
                this.WriteRecord(lastRecord, recordOffset);
            }

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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.reader.Dispose();
                    this.writer.Dispose();
#pragma warning disable IDISP007 // Don't dispose injected
                    this.stream.Dispose();
#pragma warning restore IDISP007 // Don't dispose injected
                }

                this.disposedValue = true;
            }

            if (disposing)
            {
                this.reader?.Dispose();
            }

            if (disposing)
            {
                this.writer?.Dispose();
            }
        }

        ~HashStream()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: false);
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void WriteString(string s, long offset)
        {
            this.stream.Position = offset;
            this.writer.Write(Encoding.UTF8.GetBytes(s));
        }

        private TValue ReadValue(string key)
        {
            var record = this.FindRecord(key);
            this.stream.Position = record.DataOffset;
            var buffer = this.reader.ReadBytes(record.DataLength);
            var json = Encoding.UTF8.GetString(buffer);
            var value = JsonConvert.DeserializeObject<TValue>(json);
            return value == null ? throw new InvalidDataException(json) : value;
        }

        private HashRecord FindRecord(string key)
        {
            var (_, bucket) = StableHash.GetHashBucket(key, 3, this.bucketCount);
            var offset = this.CalculateBucketOffset(bucket);

            var record = this.ReadRecord(offset);
            var recordKey = this.ReadKey(record);
            while (!key.Equals(recordKey, StringComparison.Ordinal))
            {
                if (record.NextRecordOffset != HashRecord.NullOffset)
                {
                    throw new KeyNotFoundException(key);
                }

                record = this.ReadRecord(record.NextRecordOffset);
                recordKey = this.ReadKey(record);
            }

            return record;
        }

        private string ReadKey(HashRecord record)
        {
            return this.ReadString(record.KeyOffset, record.KeyLength);
        }

        private string ReadString(long offset, int length)
        {
            this.stream.Position = offset;
            var buffer = this.reader.ReadBytes(length);
            return Encoding.UTF8.GetString(buffer);
        }

        private void WriteCount()
        {
            this.stream.Position = CountOffset;
            this.writer.Write(this.Count);
        }

        private void WriteBucketCount()
        {
            this.stream.Position = BucketCountOffset;
            this.writer.Write(this.bucketCount);
        }

        private void WriteHeader()
        {
            this.WriteCount();
            this.WriteBucketCount();
        }

        private void InitializeStream()
        {
            this.WriteHeader();
            this.AllocateBuckets();
        }

        private void AllocateBuckets()
        {
            this.stream.Position = FirstBucketOffset;
            this.writer.Write(new byte[this.bucketCount * this.recordSize]);
        }

        private long CalculateBucketOffset(uint bucket)
        {
            return bucket * this.recordSize + FirstBucketOffset;
        }

        private void WriteRecord(HashRecord record, long offset)
        {
            var buffer = new byte[this.recordSize];
            Unsafe.As<byte, HashRecord>(ref buffer[0]) = record;
            this.stream.Position = offset;
            this.writer.Write(buffer);
        }

        private HashRecord ReadRecord(long offset)
        {
            this.stream.Position = offset;
            var buffer = this.reader.ReadBytes(this.recordSize);
            return Unsafe.As<byte, HashRecord>(ref buffer[0]);
        }

        private (long offset, HashRecord record) GetLastRecordInChain(uint bucket)
        {
            var offset = this.CalculateBucketOffset(bucket);
            var nextRecordOffset = this.ReadNextRecordOffsetField(offset);

            while (nextRecordOffset != HashRecord.NullOffset)
            {
                offset = nextRecordOffset;
                nextRecordOffset = this.ReadNextRecordOffsetField(offset);
            }

            var record = this.ReadRecord(offset);
            return (offset, record);
        }

        private (long offset, int length) ReadKeyMetaData(long offset)
        {
            return (this.ReadKeyOffsetField(offset), this.ReadKeyLengthField(offset));
        }

        private (long offset, int length) ReadDataMetaData(long offset)
        {
            return (this.ReadDataOffsetField(offset), this.ReadDataLengthField(offset));
        }

        private long ReadNextRecordOffsetField(long offset)
        {
            this.stream.Position = offset + HashRecord.NextRecordOffsetFieldOffset;
            return this.reader.ReadInt64();
        }

        private uint ReadHashField(long offset)
        {
            this.stream.Position = offset + HashRecord.HashFieldOffset;
            return this.reader.ReadUInt32();
        }

        private long ReadKeyOffsetField(long offset)
        {
            this.stream.Position = offset + HashRecord.KeyOffsetFieldOffset;
            return this.reader.ReadInt64();
        }

        private int ReadKeyLengthField(long offset)
        {
            this.stream.Position = offset + HashRecord.KeyLengthFieldOffset;
            return this.reader.ReadInt32();
        }

        private long ReadDataOffsetField(long offset)
        {
            this.stream.Position = offset + HashRecord.DataOffsetFieldOffset;
            return this.reader.ReadInt64();
        }

        private int ReadDataLengthField(long offset)
        {
            this.stream.Position = offset + HashRecord.DataLengthFieldOffset;
            return this.reader.ReadInt32();
        }
    }
}
