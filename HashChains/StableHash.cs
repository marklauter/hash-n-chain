namespace HashChains
{
    // http://www.partow.net/programming/hashfunctions/index.html#GeneralHashFunctionLicense
    public static class StableHash
    {
        public static uint GetHashBucket(string value, uint length, uint buckets)
        {
            var hash = Prehash(value, length);
            return hash % buckets;
        }

        public static uint Prehash(string value, uint length)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));
            }

            unchecked
            {
                var hash = 5381u;
                for (var i = 0; i < length && i < value.Length; ++i)
                {
                    hash = (hash << 5) + hash + value[i];
                }

                return hash;
            }
        }
    }
}
