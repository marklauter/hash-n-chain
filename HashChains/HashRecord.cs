using System.Diagnostics;

namespace HashChains
{

    [DebuggerDisplay("{Offset}, {Value}")]
    public readonly struct HashRecord
    {
        internal HashRecord(
            long offset,
            uint value)
        {
            this.Offset = offset;
            this.Value = value;
        }

        public readonly long Offset;
        public readonly uint Value;
    }
}