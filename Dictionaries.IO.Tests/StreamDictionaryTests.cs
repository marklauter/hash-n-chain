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
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

            Assert.Equal(bucketCount * Marshal.SizeOf<DictionaryRecord>() + sizeof(int) * 3, stream.Length);
        }

        [Fact]
        public void StreamDictionary_Stream_IsReadOnly()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

            var key = "key";
            var value = "value";

            dictionary.Add(key, value);

#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var roStream = new MemoryStream(stream.ToArray(), false);
#pragma warning restore IDISP001 // Dispose created
            using var roDictionary = new StreamDictionary<string>(roStream);
            Assert.True(roDictionary.IsReadOnly);
        }

        [Fact]
        public void StreamDictionary_Add()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

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
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

            var key = "key";
            var value = "value";

            dictionary.Add(key, value);
            var exception = Assert.Throws<ArgumentException>(() => dictionary.Add(key, value));
            Assert.Contains(key, exception.Message);
            Assert.Contains(nameof(key), exception.Message);
        }

        [Fact]
        public void StreamDictionary_Get_Indexer()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i}";

                dictionary.Add(key, value);
            }

            for (var i = 0; i < 3; i++)
            {
                var value = dictionary[$"key{i}"];
                Assert.Equal($"value{i}", value);
            }
        }

        [Fact]
        public void StreamDictionary_Set_Indexer()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i}";

                dictionary[key] = value;
            }

            for (var i = 0; i < 3; i++)
            {
                var value = dictionary[$"key{i}"];
                Assert.Equal($"value{i}", value);
            }

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i * 10}";

                dictionary[key] = value;
            }

            for (var i = 0; i < 3; i++)
            {
                var value = dictionary[$"key{i}"];
                Assert.Equal($"value{i * 10}", value);
            }
        }

        [Fact]
        public void StreamDictionary_TryGetValue()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i}";

                dictionary.Add(key, value);
            }

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                if (dictionary.TryGetValue(key, out var v))
                {
                    Assert.Equal($"value{i}", v);
                }
            }

            Assert.False(dictionary.TryGetValue("no-key", out var x));
        }

        [Fact]
        public void StreamDictionary_Index_Throws_KeyNotFound()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

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
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

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
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

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
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

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
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

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
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

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
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

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
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i}";

                dictionary.Add(key, value);
            }

            var values = dictionary.Values;
            Assert.Equal(3, values.Count);
            for (var i = 0; i < 3; i++)
            {
                Assert.Contains($"value{i}", values);
            }
        }

        [Fact]
        public void StreamDictionary_Stream_With_Data_Sets_Count_Buckets_N_PreHashLength()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i}";

                dictionary.Add(key, value);
            }

            using var stream2 = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(stream2);

            using var dictionary2 = new StreamDictionary<string>(stream2);
            Assert.Equal(dictionary.Count, dictionary2.Count);
            Assert.Equal(dictionary.BucketCount, dictionary2.BucketCount);
            Assert.Equal(dictionary.prehashLength, dictionary2.prehashLength);
        }

        [Fact]
        public void StreamDictionary_Stream_CopyTo()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i}";

                dictionary.Add(key, value);
            }

            var array = new KeyValuePair<string, string>[dictionary.Count];
            dictionary.CopyTo(array, 0);

            for (var i = 0; i < 3; i++)
            {
                var kvp = array[i];
                Assert.Equal($"key{i}", kvp.Key);
                Assert.Equal($"value{i}", kvp.Value);
            }
        }

        [Fact]
        public void StreamDictionary_Stream_Remove_Returns_False_When_Key_Not_Found()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i}";

                dictionary.Add(key, value);
            }

            Assert.False(dictionary.Remove("no key"));
        }

        [Fact]
        public void StreamDictionary_Stream_Remove_First_Bucket_Item_Returns_True_When_Key_Found()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 3);

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i}";

                dictionary.Add(key, value);
            }

            Assert.True(dictionary.Remove("key0"));
            Assert.False(dictionary.ContainsKey("key0"));
            Assert.True(dictionary.ContainsKey("key1"));
            Assert.True(dictionary.ContainsKey("key2"));
        }

        [Fact]
        public void StreamDictionary_Stream_Remove_Second_Bucket_Item_Returns_True_When_Key_Found()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 3);

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i}";

                dictionary.Add(key, value);
            }

            Assert.True(dictionary.Remove("key1"));
            Assert.True(dictionary.ContainsKey("key0"));
            Assert.False(dictionary.ContainsKey("key1"));
            Assert.True(dictionary.ContainsKey("key2"));
        }

        [Fact]
        public void StreamDictionary_Stream_Remove_Last_Bucket_Item_Returns_True_When_Key_Found()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 3);

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i}";

                dictionary.Add(key, value);
            }

            Assert.True(dictionary.Remove("key2"));
            Assert.True(dictionary.ContainsKey("key0"));
            Assert.True(dictionary.ContainsKey("key1"));
            Assert.False(dictionary.ContainsKey("key2"));
        }

        [Fact]
        public void StreamDictionary_GetEnumerator()
        {
#pragma warning disable IDISP001 // Dispose created - hash stream disposes
            var stream = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created
            var bucketCount = 10;
            using var dictionary = new StreamDictionary<string>(stream, bucketCount, 4);

            for (var i = 0; i < 3; i++)
            {
                var key = $"key{i}";
                var value = $"value{i}";

                dictionary.Add(key, value);
            }

            foreach (var kvp in dictionary)
            {
                Assert.True(dictionary.ContainsKey(kvp.Key));
            }
        }
    }
}
