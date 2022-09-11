namespace Dictionaries.IO.Tests
{
    public class StableHashTests
    {
        [Fact]
        public void HashExtensions_Prehash_GeneratesExpected_Hash()
        {
            var value = "hello";

            var hash = StableHash.Prehash(value, 3);

            Assert.Equal(193493694u, hash);
        }

        [Fact]
        public void HashExtensions_Prehash_Doesnt_Overrun_String_Length()
        {
            var value = "hello";

            var hash = StableHash.Prehash(value, value.Length + 1);

            Assert.Equal(261238937u, hash);

            hash = StableHash.Prehash(value, value.Length + 10);

            Assert.Equal(261238937u, hash);
        }

        [Fact]
        public void HashExtensions_Generates_Different_Values_For_Different_Strings()
        {
            var value1 = "hello";
            var value2 = "jimmy";

            var hash1 = StableHash.Prehash(value1, 3);
            var hash2 = StableHash.Prehash(value2, 3);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void HashExtensions_Generates_Same_Values_For_Different_Strings()
        {
            var value1 = "hello";
            var value2 = "help";

            var hash1 = StableHash.Prehash(value1, 3);
            var hash2 = StableHash.Prehash(value2, 3);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void HashExtensions_GetHashBucket_Returns_Expected_Bucket()
        {
            var value1 = "hello";
            var value2 = "jimmy";
            var buckets = 10;

            (_, var bucket) = StableHash.GetHashBucket(value1, 3, buckets);
            Assert.True(bucket <= buckets && bucket >= 0);
            Assert.Equal(4, bucket);

            (_, bucket) = StableHash.GetHashBucket(value2, 3, buckets);
            Assert.True(bucket <= buckets && bucket >= 0);
            Assert.Equal(5, bucket);
        }
    }
}
