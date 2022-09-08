using System.Runtime.InteropServices;

namespace Dictionaries.IO.Tests
{
    public class StreamDictionaryTests
    {
        [Fact]
        public void StreamDictionary_Stream_Gets_Initialized()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var hashStream = new StreamDictionary<string>(stream, bucketCount);

            Assert.Equal(bucketCount * Marshal.SizeOf<DictionaryRecord>() + sizeof(int) * 2, stream.Length);
        }

        [Fact]
        public void StreamDictionary_Add()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var hashStream = new StreamDictionary<string>(stream, bucketCount);

            var key = "key";
            var value = "value";

            hashStream.Add(key, value);

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
            Assert.Equal(1, hashStream.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
        }

        [Fact]
        public void StreamDictionary_Index()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var hashStream = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";

            var key2 = "key2";
            var value2 = "value2";

            var key3 = "key3";
            var value3 = "value3";

            hashStream.Add(key1, value1);
            hashStream.Add(key2, value2);
            hashStream.Add(key3, value3);

            var returnedValue = hashStream[key1];
            Assert.Equal(value1, returnedValue);

            returnedValue = hashStream[key2];
            Assert.Equal(value2, returnedValue);

            returnedValue = hashStream[key3];
            Assert.Equal(value3, returnedValue);
        }

        [Fact]
        public void StreamDictionary_Index_Throws_KeyNotFound()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var hashStream = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";

            var key2 = "key2";
            var value2 = "value2";

            var key3 = "key3";
            var value3 = "value3";

            var key4 = "not found";

            hashStream.Add(key1, value1);
            hashStream.Add(key2, value2);
            hashStream.Add(key3, value3);

            var exception = Assert.Throws<KeyNotFoundException>(() => { var value = hashStream[key4]; });
            Assert.Contains(key4, exception.Message);
        }

        [Fact]
        public void StreamDictionary_Index_Throws_ArgumentNullException()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var hashStream = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";

            var key2 = "key2";
            var value2 = "value2";

            var key3 = "key3";
            var value3 = "value3";

            hashStream.Add(key1, value1);
            hashStream.Add(key2, value2);
            hashStream.Add(key3, value3);

            var exception = Assert.Throws<ArgumentNullException>(() => { var value = hashStream[String.Empty]; });
            Assert.Contains("key", exception.Message);

            string? nullkey = null;
#pragma warning disable CS8604 // Possible null reference argument.
            exception = Assert.Throws<ArgumentNullException>(() => { var value = hashStream[nullkey]; });
#pragma warning restore CS8604 // Possible null reference argument.
            Assert.Contains("key", exception.Message);
        }

        [Fact]
        public void StreamDictionary_Keys()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var hashStream = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";

            var key2 = "key2";
            var value2 = "value2";

            var key3 = "key3";
            var value3 = "value3";

            hashStream.Add(key1, value1);
            hashStream.Add(key2, value2);
            hashStream.Add(key3, value3);

            var keys = hashStream.Keys;
            Assert.Equal(3, keys.Count);
            Assert.Contains(key1, keys);
            Assert.Contains(key2, keys);
            Assert.Contains(key3, keys);
        }
    }
}
