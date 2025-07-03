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
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIndex"/> class, creating an index for the specified list
        /// of objects.
        /// </summary>
        /// <remarks>The constructor builds an internal index mapping each unique object in the list to
        /// the list of its indices. This allows for efficient lookups of object positions within the list.</remarks>
        /// <param name="objects">The list of objects to index. Cannot be null.</param>
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
                    slicedObjects.Add(objects[i]);
                }
            }

            if(step < 0)
            {
                for (int i = start; i >= end; i += step)
                {
                    slicedObjects.Add(objects[i]);
                }
            }

            return new ObjectIndex(slicedObjects);
        }

        public override IIndex Slice(List<object> indexList)
        {
            List<object> slicedObjects = new List<object>();
            foreach (var item in indexList)
            {
                if(item == null)
                {
                    throw new ArgumentException(nameof(item), "Item cannot be null.");
                }
                if (indexMap.ContainsKey(item))
                {
                    foreach (var position in indexMap[item])
                    {
                        slicedObjects.Add(objects[position]);
                    }
                }
                else
                {
                    throw new ArgumentException($"Object {item} not found in the current index.");
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

        public override object this[int index] {
           protected set
            {
                if (index < 0 || index >= objects.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                object oldValue = objects[index];
                objects[index] = value;

                // Cập nhật indexMap
                if (indexMap.ContainsKey(oldValue))
                {
                    indexMap[oldValue].Remove(index);
                    if (indexMap[oldValue].Count == 0)
                    {
                        indexMap.Remove(oldValue);
                    }
                }

                if (!indexMap.ContainsKey(value))
                {
                    indexMap[value] = new List<int>();
                }
                indexMap[value].Add(index);
            }
    }
    }
}
