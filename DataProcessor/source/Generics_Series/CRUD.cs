using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Generics_Series
{
    public partial class Series<DataType>
    {
        public void UpdateValues(object index, List<DataType> newValues)
        {
            if (!this.index.Contains(index))
            {
                throw new ArgumentException("Index not found", nameof(index));
            }

            var indices = this.indexMap[index]; // Lấy danh sách index ánh xạ

            // Đảm bảo newValues hợp lệ: hoặc có 1 phần tử hoặc có đúng số lượng phần tử của index
            if (newValues.Count != 1 && newValues.Count != indices.Count)
            {
                throw new ArgumentException("Value list must either have one element or match the indexed element count.");
            }

            for (int j = 0; j < indices.Count; j++)
            {
                this.values[indices[j]] = (newValues.Count == 1) ? newValues[0] : newValues[j];
            }
        }

        public void Add(DataType item, object? index = null)
        {
            if (index == null)
            {
                if (!this.defaultIndex)
                {
                    throw new ArgumentException("Cannot add null index when index is not default");
                }
                this.index.Add(this.Count);
                this.indexMap[this.Count] = new List<int> { this.Count };
                return;
            }
            if (index != null)
            {
                this.index.Add(index);
                if (!indexMap.TryGetValue(index, out var list))
                {
                    list = new List<int>();
                    indexMap[index] = list;
                }
                list.Add(values.Count);
                this.defaultIndex = false;
            }
            this.values.Add(item);
        }

        public bool Remove(DataType item, bool deleteIndexIfEmpty = true)
        {
            bool removed = false;
            var keysToDelete = new List<object>(); // Lưu index cần xóa
            foreach (var key in indexMap.Keys.ToList()) // ToList() để tránh Collection Modified
            {
                if (indexMap.TryGetValue(key, out var positions))
                {
                    var toRemove = positions.Where(i => Equals(values[i], item)).ToList();
                    if (toRemove.Count > 0)
                    {
                        removed = true;
                        positions.RemoveAll(i => toRemove.Contains(i));

                        if (positions.Count == 0 && deleteIndexIfEmpty)
                        {
                            keysToDelete.Add(key);
                        }
                    }
                }
            }

            if (!removed) return false;

            // Cleanup values và cập nhật lại indexMap
            var newValues = new List<DataType>();
            var newIndexMap = new Dictionary<object, List<int>>();

            int[] indexMapping = new int[values.Count]; // Ánh xạ index cũ -> index mới
            int newIdx = 0;

            for (int i = 0; i < values.Count; i++)
            {
                if (!indexMap.Values.Any(lst => lst.Contains(i))) continue; // Bỏ qua giá trị không còn được tham chiếu

                newValues.Add(values[i]);
                indexMapping[i] = newIdx++;

                foreach (var key in indexMap.Keys)
                {
                    if (indexMap[key].Contains(i))
                    {
                        if (!newIndexMap.ContainsKey(key))
                        {
                            newIndexMap[key] = new List<int>();
                        }
                        newIndexMap[key].Add(indexMapping[i]);
                    }
                }
            }

            values = newValues;
            indexMap = newIndexMap;

            // Xóa index sau khi xử lý xong để tránh Collection Modified
            foreach (var key in keysToDelete)
            {
                indexMap.Remove(key);
            }

            return true;
        }

        public void Clear()
        {
            values.Clear();
            this.index.Clear();
            this.indexMap.Clear();
        }
    }
}
