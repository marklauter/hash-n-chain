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
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            Assert.Equal(bucketCount * Marshal.SizeOf<DictionaryRecord>() + sizeof(int) * 2, stream.Length);
        }

        [Fact]
        public void StreamDictionary_Add()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            var key = "key";
            var value = "value";

            dictionary.Add(key, value);

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
            Assert.Equal(1, dictionary.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
        }

        [Fact]
        public void StreamDictionary_Add_Throws_On_DuplicateKey()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            var key = "key";
            var value = "value";

            dictionary.Add(key, value);
            var exception = Assert.Throws<ArgumentException>(() => dictionary.Add(key, value));
            Assert.Contains(key, exception.Message);
            Assert.Contains(nameof(key), exception.Message);
        }

        [Fact]
        public void StreamDictionary_Index()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";

            var key2 = "key2";
            var value2 = "value2";

            var key3 = "key3";
            var value3 = "value3";

            dictionary.Add(key1, value1);
            dictionary.Add(key2, value2);
            dictionary.Add(key3, value3);

            var returnedValue = dictionary[key1];
            Assert.Equal(value1, returnedValue);

            returnedValue = dictionary[key2];
            Assert.Equal(value2, returnedValue);

            returnedValue = dictionary[key3];
            Assert.Equal(value3, returnedValue);
        }

        [Fact]
        public void StreamDictionary_Index_Throws_KeyNotFound()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";

            var key2 = "key2";
            var value2 = "value2";

            var key3 = "key3";
            var value3 = "value3";

            var key4 = "not found";

            dictionary.Add(key1, value1);
            dictionary.Add(key2, value2);
            dictionary.Add(key3, value3);

            var exception = Assert.Throws<KeyNotFoundException>(() => _ = dictionary[key4]);
            Assert.Contains(key4, exception.Message);
        }

        [Fact]
        public void StreamDictionary_Index_Throws_ArgumentNullException()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";

            var key2 = "key2";
            var value2 = "value2";

            var key3 = "key3";
            var value3 = "value3";

            dictionary.Add(key1, value1);
            dictionary.Add(key2, value2);
            dictionary.Add(key3, value3);

            var exception = Assert.Throws<ArgumentNullException>(() => { var value = dictionary[String.Empty]; });
            Assert.Contains("key", exception.Message);

            string? nullkey = null;
#pragma warning disable CS8604 // Possible null reference argument.
            exception = Assert.Throws<ArgumentNullException>(() => { var value = dictionary[nullkey]; });
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
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";

            var key2 = "key2";
            var value2 = "value2";

            var key3 = "key3";
            var value3 = "value3";

            dictionary.Add(key1, value1);
            dictionary.Add(key2, value2);
            dictionary.Add(key3, value3);

            var keys = dictionary.Keys;
            Assert.Equal(3, keys.Count);
            Assert.Contains(key1, keys);
            Assert.Contains(key2, keys);
            Assert.Contains(key3, keys);
        }

        [Fact]
        public void StreamDictionary_ContainsKey()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";

            var key2 = "key2";
            var value2 = "value2";

            var key3 = "key3";
            var value3 = "value3";

            dictionary.Add(key1, value1);
            dictionary.Add(key2, value2);
            dictionary.Add(key3, value3);

            Assert.True(dictionary.ContainsKey(key1));
            Assert.True(dictionary.ContainsKey(key2));
            Assert.True(dictionary.ContainsKey(key3));
        }

        [Fact]
        public void StreamDictionary_ContainsKey_Throws_ArgumentNullException()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";

            var key2 = "key2";
            var value2 = "value2";

            var key3 = "key3";
            var value3 = "value3";

            dictionary.Add(key1, value1);
            dictionary.Add(key2, value2);
            dictionary.Add(key3, value3);

            var exception = Assert.Throws<ArgumentNullException>(() => { var value = dictionary.ContainsKey(String.Empty); });
            Assert.Contains("key", exception.Message);

            string? nullkey = null;
#pragma warning disable CS8604 // Possible null reference argument.
            exception = Assert.Throws<ArgumentNullException>(() => { var value = dictionary.ContainsKey(nullkey); });
#pragma warning restore CS8604 // Possible null reference argument.
            Assert.Contains("key", exception.Message);
        }

        [Fact]
        public void StreamDictionary_Clear()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";
            var key2 = "key2";
            var value2 = "value2";
            var key3 = "key3";
            var value3 = "value3";
            var key4 = "key4";
            var value4 = "value4";

            dictionary.Add(key1, value1);

            dictionary.Clear();
            Assert.True(dictionary.Count == 0);

            dictionary.Add(key2, value2);
            dictionary.Add(key3, value3);
            dictionary.Add(key4, value4);

            var exception = Assert.Throws<KeyNotFoundException>(() => _ = dictionary[key1]);
            Assert.Contains(key1, exception.Message);

            var returnedValue = dictionary[key2];
            Assert.Equal(value2, returnedValue);

            returnedValue = dictionary[key3];
            Assert.Equal(value3, returnedValue);

            returnedValue = dictionary[key4];
            Assert.Equal(value4, returnedValue);
        }

        [Fact]
        public void StreamDictionary_Contains()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";
            var kvp1 = new KeyValuePair<string, string>(key1, value1);

            var key2 = "key2";
            var value2 = "value2";
            var kvp2 = new KeyValuePair<string, string>(key2, value2);

            var key3 = "key3";
            var value3 = "value3";
            var kvp3 = new KeyValuePair<string, string>(key3, value3);

            dictionary.Add(key1, value1);
            dictionary.Add(key2, value2);
            dictionary.Add(key3, value3);

#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
            Assert.True(dictionary.Contains(kvp1));
            Assert.True(dictionary.Contains(kvp2));
            Assert.True(dictionary.Contains(kvp3));
#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
        }

        [Fact]
        public void StreamDictionary_Values()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10u;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount);

            var key1 = "key1";
            var value1 = "value1";

            var key2 = "key2";
            var value2 = "value2";

            var key3 = "key3";
            var value3 = "value3";

            dictionary.Add(key1, value1);
            dictionary.Add(key2, value2);
            dictionary.Add(key3, value3);

            var values = dictionary.Values;
            Assert.Equal(3, values.Count);
            Assert.Contains(value1, values);
            Assert.Contains(value2, values);
            Assert.Contains(value3, values);
        }
    }
}
