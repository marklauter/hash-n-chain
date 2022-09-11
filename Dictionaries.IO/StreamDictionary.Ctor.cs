using System.Runtime.InteropServices;
using System.Text;

namespace Dictionaries.IO
{

    public partial class StreamDictionary<TValue>
    {
        private readonly Stream stream;
        private readonly BinaryWriter writer;
        private readonly BinaryReader reader;
        private readonly int recordSize;

        public uint BucketCount { get; }

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
            this.BucketCount = this.ReadBucketCount();
            var minFileSize = this.CalculateBucketOffset(this.BucketCount);
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

            this.BucketCount = bucketCount;
            this.IsReadOnly = isReadOnly;
            this.writer = new BinaryWriter(stream, Encoding.UTF8, true);
            this.reader = new BinaryReader(stream, Encoding.UTF8, true);
            this.recordSize = Marshal.SizeOf<DictionaryRecord>();
            this.InitializeStream();
        }
    }
}
