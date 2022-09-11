using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dictionaries.IO
{
    public partial class StreamDictionary<TValue>
    {
        private void WriteString(string s, long offset)
        {
            this.stream.Position = offset;
            this.writer?.Write(Encoding.UTF8.GetBytes(s));
        }

        private IEnumerable<KeyValuePair<string, TValue>> ReadKeyValuePairs()
        {
            for (var bucket = 0; bucket < this.BucketCount; ++bucket)
            {
                var offset = this.CalculateBucketOffset((int)(uint)bucket);
                do
                {
                    var keyMetaData = this.ReadKeyMetaData(offset);
                    var dataMetaData = this.ReadDataMetaData(offset);
                    if (keyMetaData.offset != DictionaryRecord.NullOffset && keyMetaData.length > 0 &&
                        dataMetaData.offset != DictionaryRecord.NullOffset && dataMetaData.length > 0)
                    {
                        var key = this.ReadString(keyMetaData.offset, keyMetaData.length);
                        var json = this.ReadString(dataMetaData.offset, dataMetaData.length);
                        var value = JsonConvert.DeserializeObject<TValue>(json);
                        yield return value is null
                            ? throw new InvalidDataException(json)
                            : new KeyValuePair<string, TValue>(key, value);
                    }

                    offset = this.ReadNextRecordOffsetField(offset);
                } while (offset != DictionaryRecord.NullOffset);
            }
        }

        private IEnumerable<TValue> ReadValues()
        {
            for (var bucket = 0; bucket < this.BucketCount; ++bucket)
            {
                var offset = this.CalculateBucketOffset((int)(uint)bucket);
                do
                {
                    var metaData = this.ReadDataMetaData(offset);
                    if (metaData.offset != DictionaryRecord.NullOffset && metaData.length > 0)
                    {
                        var json = this.ReadString(metaData.offset, metaData.length);
                        var value = JsonConvert.DeserializeObject<TValue>(json);
                        yield return value == null ? throw new InvalidDataException(json) : value;
                    }

                    offset = this.ReadNextRecordOffsetField(offset);
                } while (offset != DictionaryRecord.NullOffset);
            }
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

        private void SetValue(string key, TValue value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (this.IsReadOnly)
            {
                throw new NotSupportedException("dictionary is readonly");
            }

            var offset = this.FindKey(key);
            if (offset == DictionaryRecord.NullOffset)
            {
                this.Add(key, value);
            }
            else
            {
                var json = JsonConvert.SerializeObject(value);
                var dataOffset = this.stream.Length;
                var dataLength = json.Length;

                var currentRecord = this.ReadRecord(offset);

                var newRecord = new DictionaryRecord(
                    currentRecord.NextRecordOffset,
                    currentRecord.Hash,
                    currentRecord.KeyOffset,
                    currentRecord.KeyLength,
                    dataOffset,
                    dataLength);

                this.WriteString(json, dataOffset);
                this.WriteRecord(newRecord, offset);
            }
        }

        private bool RemoveInternal(string key)
        {
            var (keyhash, bucket) = StableHash.GetHashBucket(key, this.prehashLength, this.BucketCount);
            var offset = this.CalculateBucketOffset(bucket);
            var bucketOffset = offset;
            var previousOffset = offset;
            do
            {
                var recordhash = this.ReadHashField(offset);
                if (keyhash == recordhash)
                {
                    var keyMetaData = this.ReadKeyMetaData(offset);
                    var recordKey = this.ReadString(keyMetaData.offset, keyMetaData.length);
                    if (key.Equals(recordKey, StringComparison.Ordinal))
                    {
                        var record = this.ReadRecord(offset);
                        if (bucketOffset == offset) // record is first in bucket
                        {
                            // overwrite first bucket record with next record or empty record
                            var nextRecord = record.NextRecordOffset == DictionaryRecord.NullOffset
                                ? DictionaryRecord.Empty
                                : this.ReadRecord(record.NextRecordOffset);

                            this.WriteRecord(nextRecord, offset);
                        }
                        else
                        {
                            // if it's any other item then the previous item
                            // has it's nextoffset adjusted to the matching item nextoffset
                            var previousRecord = this.ReadRecord(previousOffset);
                            var newRecord = new DictionaryRecord(
                                record.NextRecordOffset,
                                previousRecord.Hash,
                                previousRecord.KeyOffset,
                                previousRecord.KeyLength,
                                previousRecord.DataOffset,
                                previousRecord.DataLength);

                            this.WriteRecord(newRecord, previousOffset);
                        }

                        return true;
                    }
                }

                previousOffset = offset;
                offset = this.ReadNextRecordOffsetField(offset);

            } while (offset != DictionaryRecord.NullOffset);

            return false;
        }


        // returns offset of record matching key or DictionaryRecord.NullOffset if not found
        private long FindKey(string key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var (keyhash, bucket) = StableHash.GetHashBucket(key, this.prehashLength, this.BucketCount);
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

        private IEnumerable<string> ReadKeys()
        {
            for (var bucket = 0; bucket < this.BucketCount; ++bucket)
            {
                var offset = this.CalculateBucketOffset((int)(uint)bucket);
                do
                {
                    var metaData = this.ReadKeyMetaData(offset);
                    if (metaData.offset != DictionaryRecord.NullOffset && metaData.length > 0)
                    {
                        yield return this.ReadString(metaData.offset, metaData.length);
                    }

                    offset = this.ReadNextRecordOffsetField(offset);
                } while (offset != DictionaryRecord.NullOffset);
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

        private int ReadBucketCount()
        {
            this.stream.Position = BucketCountOffset;
            return this.reader.ReadInt32();
        }

        private int ReadPrehashLength()
        {
            this.stream.Position = PrehashLengthOffset;
            return this.reader.ReadInt32();
        }

        private void WriteCount()
        {
            this.stream.Position = CountOffset;
            this.writer?.Write(this.Count);
        }

        private void WriteBucketCount()
        {
            this.stream.Position = BucketCountOffset;
            this.writer?.Write(this.BucketCount);
        }

        private void WritePrehashLength()
        {
            this.stream.Position = PrehashLengthOffset;
            this.writer?.Write(this.prehashLength);
        }

        private void WriteHeader()
        {
            this.WriteCount();
            this.WriteBucketCount();
            this.WritePrehashLength();
        }

        private void InitializeStream()
        {
            this.Count = 0;
            this.WriteHeader();
            this.AllocateBuckets();
            this.stream.SetLength(this.BucketCount * this.recordSize + FirstBucketOffset);
        }

        private void AllocateBuckets()
        {
            this.stream.Position = FirstBucketOffset;
            this.writer?.Write(new byte[this.BucketCount * this.recordSize]);
        }

        private long CalculateBucketOffset(int bucket)
        {
            return bucket * this.recordSize + FirstBucketOffset;
        }

        private void WriteRecord(DictionaryRecord record, long offset)
        {
            var buffer = new byte[this.recordSize];
            Unsafe.As<byte, DictionaryRecord>(ref buffer[0]) = record;
            this.stream.Position = offset;
            this.writer?.Write(buffer);
        }

        private DictionaryRecord ReadRecord(long offset)
        {
            this.stream.Position = offset;
            var buffer = this.reader.ReadBytes(this.recordSize);
            return Unsafe.As<byte, DictionaryRecord>(ref buffer[0]);
        }

        private (long offset, DictionaryRecord record) GetLastRecordInBucket(int bucket)
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
