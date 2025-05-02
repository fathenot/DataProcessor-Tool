using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.ValueStorage
{
    internal class ObjectValueStorage: ValueStorage
    {
        private readonly object?[] objects;
        GCHandle handle;

        internal ObjectValueStorage(object?[] objects)
        {
            this.objects = objects;
            handle = GCHandle.Alloc(objects, GCHandleType.Pinned);
        }

        public override nint GetArrayAddress()
        {
            return handle.AddrOfPinnedObject();
        }

        public override object? GetValue(int index)
        {
            return objects[index];
        }

        public override void SetValue(int index, object? value)
        {
            objects[index] = value;
        }

        public override int Length => objects.Length;
        public override IEnumerable<int> NullPositions
        {
            get
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i] == null)
                    {
                        yield return i;
                    }
                }
            }
        }

        public override Type ValueType => typeof(object);
    }
}
