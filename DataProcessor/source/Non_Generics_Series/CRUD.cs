using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Non_Generics_Series
{
    public partial class Series
    {
        public void Add(object? item, object? index = null)
        {
            if (!IsValidType(item))
            {
                try // trying cast item to proper data type to add
                {
                    if (dType == typeof(int) && int.TryParse(item?.ToString(), out int intValue))
                    {
                        this.values.Add(intValue);
                        return;
                    }
                    if (dType == typeof(double) && double.TryParse(item?.ToString(), out double DoubleValue))
                    {
                        this.values.Add(DoubleValue);
                        return;
                    }
                    if (dType == typeof(DateTime) && DateTime.TryParse(item?.ToString(), out DateTime DateTimeValue))
                    {
                        this.values.Add(DateTimeValue);
                        return;
                    }
                    var convertedItem = Convert.ChangeType(item, dType);
                    this.values.Add(convertedItem);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Expected type {dType}, but got {item?.GetType()}. You must change the đata type to {this.dtype} first", ex);
                }
            }
            if (index == null)
            {
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
            }
            this.values.Add(item);
        }
        public bool Remove(object? item, bool deleteIndexIfEmpty = true) // remove all occurent of item
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
            var newValues = new List<object?>();
            var newIndexMap = new Dictionary<object, List<int>>();

            int[] indexMapping = new int[values.Count]; // Ánh xạ index cũ -> index mới
            int newIdx = 0;

            for (int i = 0; i < values.Count; i++)
            {
                if (!indexMap.Values.Any(lst => lst.Contains(i))) continue; // Bỏ qua giá trị không còn được tham chiếu

                newValues.Add(values[i]);
                indexMapping[i] = newIdx++; // ánh xạ từ vị trí ban đầu của giá trị sang vị trí mới

                // index ban dầu của index của value phải ánh xạ tới vị trí mỡi của giá trị này 
                foreach (var key in indexMap.Keys)
                {
                    // nêu IndexMap[key] chứa vị trí ban đầu của value không bị xoá thì 
                    // newIndex[key] ánh xạ tới vị trí mới của value
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
            this.values.Clear();
            this.index.Clear();
            this.indexMap.Clear();
        }
        public void UpdateValues(object index, List<object?> values)
        {
            // checking valid arguments
            if (!this.indexMap.TryGetValue(index, out var positions))
            {
                throw new InvalidOperationException($"index {nameof(index)} is not in series ");
            }
            if (values == null)
            {
                throw new ArgumentNullException("List of values must not be null", nameof(values));
            }
            if (values.Count == 0)
            {
                throw new InvalidOperationException("this action can't be done if values count is 0");
            }
            // check type validity
            var invalidValues = values.Where(v => !this.IsValidType(v)).ToList();
            if (invalidValues.Count > 0)
            {
                throw new ArgumentException(
                    $"The list contains {invalidValues.Count} invalid value(s): " +
                    $"{string.Join(", ", invalidValues.Select(v => v?.ToString() ?? "null"))}."
                );
            }
            if (values.Count > 1 && values.Count != positions.Count)
            {
                if (positions.Count == 0)
                {
                    throw new InvalidOperationException($"there are no elements can be replace at index{index.ToString()}.");
                }
                throw new ArgumentException($"Expected the length of the value to replace {positions.Count} or 1 but the actual length of value is {values.Count}");
            }
            // main logic of the method
            if (values.Count == 1)
            {
                foreach (var posítion in positions)
                {
                    this.values[posítion] = values[0];
                }
                return;
            }
            for (int i = 0; i < positions.Count; i++)
            {
                this.values[i] = values[i];
            }
        }
        public void UpdateValues(Series other)
        {
            this.indexMap.Clear();
            this.values.Clear();
            this.index.Clear();
            for (int i = 0; i < other.values.Count; i++)
            {
                values[i] = other.values[i];
            }
            this.index = new List<object>(other.index);
            this.indexMap = new Dictionary<object, List<int>>();
            foreach (var key in other.indexMap.Keys)
            {
                this.indexMap[key] = new List<int>(other.indexMap[key]);
            }
        }
    }
}
