using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Index
{
    public class ObjectIndex : IIndex
    {
        private readonly List<object> objects;
        private readonly Dictionary<object, List<int>> indexMap;
        public ObjectIndex(List<object> objects) : base(objects)
        {
            this.objects = objects;
            indexMap = new Dictionary<object, List<int>>();
            for (int i = 0; i < objects.Count; i++)
            {
                if (!indexMap.ContainsKey(objects[i]))
                {
                    indexMap[objects[i]] = new List<int>();
                }
                indexMap[objects[i]].Add(i);
            }
        }
        public override int Count => objects.Count;
        public override IReadOnlyList<object> IndexList => objects.AsReadOnly();
        
        public override List<int> GetIndexPosition(object obj)
        {
            if (indexMap.ContainsKey(obj))
            {
                return indexMap[obj];
            }
            throw new KeyNotFoundException($"Object {obj} not found in index.");
        }
        public override bool Contains(object key)
        {
            return indexMap.ContainsKey(key);
        }
        public override object GetIndex(int idx)
        {
            return objects[idx];
        }

        public override IEnumerable<object> DistinctIndices()
        {
            return objects.Distinct();
        }

        public override int FirstPositionOf(object key)
        {
            if(indexMap.ContainsKey(key))
            {
                return indexMap[key].First();
            }
            throw new KeyNotFoundException($"Key {key} not found in index.");
        }

        public override IIndex Slice(int start, int end, int step = 1)
        {
            // Validate the parameters
            if (step == 0)
            {
                throw new ArgumentException("Step cannot be zero.");
            }
            if (start < 0 || start >= objects.Count || end < 0 || end >= objects.Count)     
            {
                throw new ArgumentOutOfRangeException("Start or end index is out of range.");
            }

            List<object> slicedObjects = new List<object>();
            if (step > 0)
            {
                for (int i = start; i <= end; i += step)
                {
                    if (i >= objects.Count)
                    {
                        slicedObjects.Add(objects[i]);
                    }
                }
            }

            if(step < 0)
            {
                for (int i = start; i >= end; i += step)
                {
                    if (i < objects.Count)
                    {
                        slicedObjects.Add(objects[i]);
                    }
                }
            }

            return new ObjectIndex(slicedObjects);
        }

        public override IEnumerator<object> GetEnumerator()
        {
          for (int i = 0; i < objects.Count; i++)
            {
                yield return objects[i];
            }
        }
    }
}
