namespace HashChains.Tests
{
    public class HashStreamTests
    {
        [Fact]
        public unsafe void HashStream_Stream_Gets_Initialized()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var hashStream = new HashStream<string>(stream, bucketCount);

            Assert.Equal(bucketCount * sizeof(HashRecord) + sizeof(int) * 2, stream.Length);
        }

        [Fact]
        public void HashStream_Add()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var hashStream = new HashStream<string>(stream, bucketCount);

            var key = "key";
            var value = "value";

            hashStream.Add(key, value);

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
            Assert.Equal(1, hashStream.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
        }

        [Fact]
        public void HashStream_Index()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var hashStream = new HashStream<string>(stream, bucketCount);

            var key = "key";
            var value = "value";

            hashStream.Add(key, value);

            var returnedValue = hashStream["key"];
            Assert.Equal(value, returnedValue);
        }
    }
}
