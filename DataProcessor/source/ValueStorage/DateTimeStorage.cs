using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataProcessor.source.ValueStorage
{
    internal class DateTimeStorage: ValueStorage
    {
        DateTime?[] dates;
        GCHandle handle;

        internal DateTimeStorage(DateTime?[] dates)
        {
            this.dates = dates;
            handle = GCHandle.Alloc(dates, GCHandleType.Pinned);
        }

        public DateTime?[] GetDates() { return dates; } 
        public GCHandle GetHandle() { return handle; }

        public override object? GetValue(int index)
        {
            return dates[index];
        }

        public override nint GetArrayAddress()
        {
            return handle.AddrOfPinnedObject();
        }

        public override int Length => dates.Length;

        public override IEnumerable<int> NullPositions
        {
            get
            {
                for (int i = 0; i < dates.Length; i++)
                {
                    if (dates[i] == null)
                    {
                        yield return i;
                    }
                }
            }
        }

        public override Type ValueType => typeof(DateTime);

        public override void SetValue(int index, object? value)
        {
            if(value == null)
            {
                dates[index] = null;
            }      
            if(value is DateTime dateTime)
            {
                dates[index] = dateTime;
            }
        }
    }
}
