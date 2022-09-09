namespace Dictionaries.IO
{
    public partial class StreamDictionary<TValue>
        : IDisposable
    {
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.reader.Dispose();
                    this.writer.Dispose();
                    this.stream.Dispose();
                }

                this.disposedValue = true;
            }

            if (disposing)
            {
                this.reader?.Dispose();
            }

            if (disposing)
            {
                this.writer?.Dispose();
            }
        }

        ~StreamDictionary()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: false);
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
