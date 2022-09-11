namespace Dictionaries.IO
{
    // http://www.partow.net/programming/hashfunctions/index.html#GeneralHashFunctionLicense
    public static class StableHash
    {
        public static (uint hash, uint bucket) GetHashBucket(byte[] value, int length, uint bucketCount)
        {
            var hash = Prehash(value, length);
            var bucket = hash % bucketCount;
            return (hash, bucket);
        }

        public static (uint hash, uint bucket) GetHashBucket(string value, int length, uint bucketCount)
        {
            var hash = Prehash(value, length);
            var bucket = hash % bucketCount;
            return (hash, bucket);
        }

        public static uint Prehash(string value, int length)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));
            }

            unchecked
            {
                var hash = 5381u;
                length = length == -1 || length > value.Length
                    ? value.Length
                    : length;

                for (var i = 0; i < length; ++i)
                {
                    hash = (hash << 5) + hash + value[i];
                }

                return hash;
            }
        }

        public static uint Prehash(byte[] value, int length)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            unchecked
            {
                var hash = 5381u;
                length = length == -1 || length > value.Length
                    ? value.Length
                    : length;

                for (var i = 0; i < length; ++i)
                {
                    hash = (hash << 5) + hash + value[i];
                }

                return hash;
            }
        }
    }
}
