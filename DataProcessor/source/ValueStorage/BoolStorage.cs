using System.Collections;

namespace DataProcessor.source.ValueStorage
{
    internal class BoolStorage : AbstractValueStorage, IEnumerable<object?>
    {
        private readonly BitArray values;
        private readonly NullBitMap nullBitMap;
        internal BoolStorage(bool?[] bools)
        {
            int len = bools.Length;
            values = new BitArray(len);
            nullBitMap = new NullBitMap(len);
            for (int i = 0; i < len; i++)
            {
                if (bools[i].HasValue)
                {
                    values[i] = bools[i].Value;
                }
                else
                {
                    // null: không set value, nhưng đánh dấu null = true
                    values[i] = false; // placeholder, không quan trọng
                    nullBitMap.SetNull(i, true);
                }
            }
        }

        internal BoolStorage(bool[] bools)
        {
            this.values = new BitArray(bools);
        }

        internal override int Count => values.Count;
        internal override Type ElementType => typeof(bool);

        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < values.Length; i++)
                    if (nullBitMap.IsNull(i)) yield return i;
            }
        }

        internal bool[] Value
        {
            get
            {
                bool[] result = new bool[values.Length - NullIndices.Count()];
                int current_idx = 0;
                for (int i = 0; i < this.Count; i++)
                {
                    if (!nullBitMap.IsNull(i))
                    {
                        result[current_idx] = values[i];
                        current_idx++;
                    }
                }
                return result;
            }
        }
        internal override nint GetNativeBufferPointer()
        {
            throw new NotImplementedException();
        }

        internal override object? GetValue(int index)
        {
            return values[index];
        }

        internal override void SetValue(int index, object? value)
        {
            if (value == null)
            {
                nullBitMap.SetNull(index, true);
                return;
            }
            else if (value is bool b)
            {
                values[index] = b;
                return;
            }
            throw new ArgumentException("value must be boolean value or null");
        }

        public override IEnumerator<object?> GetEnumerator()
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (nullBitMap.IsNull(i))
                    yield return null;
                else
                    yield return values[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}
