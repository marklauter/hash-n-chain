namespace Dictionaries.IO
{
    public partial class StreamDictionary<TValue>
    {
        private const long CountOffset = 0;
        private const long BucketCountOffset = sizeof(int);
        private const long FirstBucketOffset = sizeof(int) * 2;
        private const int PrehashLength = 3;
    }
}
