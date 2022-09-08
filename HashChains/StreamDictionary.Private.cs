﻿using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dictionaries.IO
{
    public partial class StreamDictionary<TValue>
    {
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
            var offset = this.FindKey(key);
            return offset == DictionaryRecord.NullOffset
                ? throw new KeyNotFoundException(key)
                : this.ReadRecord(offset);
        }

        // returns offset of record matching key or DictionaryRecord.NullOffset if not found
        private long FindKey(string key)
        {
            var (keyhash, bucket) = StableHash.GetHashBucket(key, PrehashLength, this.bucketCount);
            var offset = this.CalculateBucketOffset(bucket);
            do
            {
                var recordhash = this.ReadHashField(offset);
                if (keyhash == recordhash)
                {
                    var keyMetaData = this.ReadKeyMetaData(offset);
                    var recordKey = this.ReadString(keyMetaData.offset, keyMetaData.length);
                    if (key.Equals(recordKey, StringComparison.Ordinal))
                    {
                        return offset;
                    }
                }

                offset = this.ReadNextRecordOffsetField(offset);

            } while (offset != DictionaryRecord.NullOffset);

            return DictionaryRecord.NullOffset;
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