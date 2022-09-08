﻿using Newtonsoft.Json;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Dictionaries.IO
{
    internal class StreamDictionary<TValue>
        : IStreamDictionary<TValue>
        , IDisposable
    {
        private const long CountOffset = 0;
        private const long BucketCountOffset = sizeof(int);
        private const long FirstBucketOffset = sizeof(int) * 2;
        private const int PrehashLength = 3;

        private readonly Stream stream;
        private readonly BinaryWriter writer;
        private readonly BinaryReader reader;
        private readonly uint bucketCount;
        private readonly int recordSize;
        private bool disposedValue;

        public StreamDictionary(Stream stream)
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

            this.IsReadOnly = !stream.CanWrite;

            this.writer = new BinaryWriter(stream, Encoding.UTF8, true);
            this.reader = new BinaryReader(stream, Encoding.UTF8, true);
            this.recordSize = Marshal.SizeOf<DictionaryRecord>();

            this.Count = this.ReadCount();
            this.bucketCount = this.ReadBucketCount();
            var minFileSize = this.CalculateBucketOffset(this.bucketCount);
            if (stream.Length < minFileSize)
            {
                throw new ArgumentException($"invalid stream size. expected: {minFileSize}, actual: {stream.Length}", nameof(stream));
            }
        }

        public StreamDictionary(
            Stream stream,
            uint bucketCount)
            : this(stream, bucketCount, false)
        {
        }

        public StreamDictionary(
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

            if (isReadOnly && !stream.CanWrite)
            {
                throw new ArgumentException("can't write to stream", nameof(isReadOnly));
            }

            if (stream.Length != 0)
            {
                throw new ArgumentException("expected stream to be zero length", nameof(stream));
            }

            this.bucketCount = bucketCount;
            this.IsReadOnly = isReadOnly;
            this.writer = new BinaryWriter(stream, Encoding.UTF8, true);
            this.reader = new BinaryReader(stream, Encoding.UTF8, true);
            this.recordSize = Marshal.SizeOf<DictionaryRecord>();
            this.InitializeStream();
        }

        public TValue this[string key]
        {
            get => this.ReadValue(key);
            set => throw new NotImplementedException();
        }

        public ICollection<string> Keys => this.ReadKeys().ToList();
        public ICollection<TValue> Values => throw new NotImplementedException();
        public int Count { get; private set; }
        public bool IsReadOnly { get; }

        public void Add(string key, TValue value)
        {
            var (hash, bucket) = StableHash.GetHashBucket(key, PrehashLength, this.bucketCount);
            (var recordOffset, var lastRecord) = this.GetLastRecordInBucket(bucket);

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

        ~StreamDictionary()
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
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var record = this.FindRecord(key);
            this.stream.Position = record.DataOffset;
            var buffer = this.reader.ReadBytes(record.DataLength);
            var json = Encoding.UTF8.GetString(buffer);
            var value = JsonConvert.DeserializeObject<TValue>(json);
            return value == null ? throw new InvalidDataException(json) : value;
        }

        private DictionaryRecord FindRecord(string key)
        {
            var (keyhash, bucket) = StableHash.GetHashBucket(key, PrehashLength, this.bucketCount);
            var offset = this.CalculateBucketOffset(bucket);
            do
            {
                var recordhash = this.ReadHashField(offset);
                if (keyhash == recordhash)
                {
                    // then look closer and possibly return
                    var keyMetaData = this.ReadKeyMetaData(offset);
                    var recordKey = this.ReadString(keyMetaData.offset, keyMetaData.length);
                    if (key.Equals(recordKey, StringComparison.Ordinal))
                    {
                        return this.ReadRecord(offset);
                    }
                }
                // else move to next
                offset = this.ReadNextRecordOffsetField(offset);

            } while (offset != DictionaryRecord.NullOffset);

            throw new KeyNotFoundException(key);
        }

        private string ReadKey(DictionaryRecord record)
        {
            return this.ReadString(record.KeyOffset, record.KeyLength);
        }

        private IEnumerable<string> ReadKeys()
        {
            for (var bucket = 0; bucket < this.bucketCount; ++bucket)
            {
                var offset = this.CalculateBucketOffset((uint)bucket);
                while (offset != DictionaryRecord.NullOffset)
                {
                    var keyMetaData = this.ReadKeyMetaData(offset);
                    if (keyMetaData.offset != DictionaryRecord.NullOffset && keyMetaData.length > 0)
                    {
                        yield return this.ReadString(keyMetaData.offset, keyMetaData.length);
                    }

                    offset = this.ReadNextRecordOffsetField(offset);
                }
            }
        }

        private string ReadString(long offset, int length)
        {
            this.stream.Position = offset;
            var buffer = this.reader.ReadBytes(length);
            return Encoding.UTF8.GetString(buffer);
        }

        private int ReadCount()
        {
            this.stream.Position = CountOffset;
            return this.reader.ReadInt32();
        }

        private uint ReadBucketCount()
        {
            this.stream.Position = BucketCountOffset;
            return this.reader.ReadUInt32();
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

        private void WriteRecord(DictionaryRecord record, long offset)
        {
            var buffer = new byte[this.recordSize];
            Unsafe.As<byte, DictionaryRecord>(ref buffer[0]) = record;
            this.stream.Position = offset;
            this.writer.Write(buffer);
        }

        private DictionaryRecord ReadRecord(long offset)
        {
            this.stream.Position = offset;
            var buffer = this.reader.ReadBytes(this.recordSize);
            return Unsafe.As<byte, DictionaryRecord>(ref buffer[0]);
        }

        private (long offset, DictionaryRecord record) GetLastRecordInBucket(uint bucket)
        {
            var offset = this.CalculateBucketOffset(bucket);
            var nextRecordOffset = this.ReadNextRecordOffsetField(offset);

            while (nextRecordOffset != DictionaryRecord.NullOffset)
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
            this.stream.Position = offset + DictionaryRecord.NextRecordOffsetFieldOffset;
            return this.reader.ReadInt64();
        }

        private uint ReadHashField(long offset)
        {
            this.stream.Position = offset + DictionaryRecord.HashFieldOffset;
            return this.reader.ReadUInt32();
        }

        private long ReadKeyOffsetField(long offset)
        {
            this.stream.Position = offset + DictionaryRecord.KeyOffsetFieldOffset;
            return this.reader.ReadInt64();
        }

        private int ReadKeyLengthField(long offset)
        {
            this.stream.Position = offset + DictionaryRecord.KeyLengthFieldOffset;
            return this.reader.ReadInt32();
        }

        private long ReadDataOffsetField(long offset)
        {
            this.stream.Position = offset + DictionaryRecord.DataOffsetFieldOffset;
            return this.reader.ReadInt64();
        }

        private int ReadDataLengthField(long offset)
        {
            this.stream.Position = offset + DictionaryRecord.DataLengthFieldOffset;
            return this.reader.ReadInt32();
        }
    }
}