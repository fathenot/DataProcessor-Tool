namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series
    {
        /// <summary>
        /// Add an item to the series.
        /// If index = null it's index is the position of the item
        /// </summary>
        /// <param name="item"> item need to add </param>
        /// <param name="index"> custom index to the added item </param>
        public void Add(object? item, object? index = null)
        {
            values.Add(item);
            Support.InferDataType(values);
            // handle index
            if (index != null)
            {
                foreach (var idx in indexMap.Keys)
                {
                    indexMap.TryGetValue(idx, out List<int> pos);
                    if (pos.Contains(values.Count - 1))
                    {
                        pos.Remove(values.Count);
                        return;
                    }
                }
                if (indexMap.TryGetValue(index, out List<int> positions))
                {
                    positions.Add(values.Count);
                }
                else
                {
                    indexMap[index] = new List<int> { values.Count };
                    this.index.Add(index);
                }
            }
            else
            {
                foreach (var idx in indexMap.Keys)
                {
                    if (indexMap[idx].Contains(values.Count - 1))
                    {
                        return;
                    }
                }
                indexMap[values.Count - 1] = new List<int> { values.Count-1};
                this.index.Add((int)values.Count-1);
            }
            if (item != null)
            {
                dataType = Support.InferDataType(values);
            }
        }

        public bool RemoveFirstOccurence(object? item, bool deleteIndexIfEmpty = true)
        {
            var itemIndex = this.values.IndexOf(item);
            // remove item then remap the index
            return itemIndex >= 0;
        }
                              
        /// <summary>
        /// Removes all occurrences of the specified item from the series. 
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="deleteIndexIfEmpty">
        /// The index of the removed item, if no items are left with that index.
        /// </param>
        /// <returns>True if the item was successfully removed; otherwise, false.</returns>
        public bool RemoveAllOccurence(object? item, bool deleteIndexIfEmpty = true) 
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
            // update lại kiểu dữ liệu
            Support.InferDataType(values);
            return true;
        }

        /// <summary>
        /// clear all the data of the series
        /// </summary>
        public void Clear()
        {
            this.values.Clear();
            this.index.Clear();
            this.indexMap.Clear();
        }
        /// <summary>
        /// Update values of the series
        /// </summary>
        /// <param name="index">index of the </param>
        /// <param name="values"> if values size is 1 it will replace all the elements of the series with the value in the list</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void UpdateValues(object index, List<object?> newValues)
        {
            // checking valid arguments
            if (!this.indexMap.TryGetValue(index, out var positions))
            {
                throw new InvalidOperationException($"index {nameof(index)} is not in series ");
            }
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values), "List of values must not be null");
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
                    throw new InvalidOperationException($"there are no elements can be replace at index{index}.");
                }
                throw new ArgumentException($"Expected the length of the value to replace {positions.Count} or 1 but the actual length of value is {values.Count}");
            }
            // main logic of the method
            if (values.Count == 1)
            {
                foreach (var posítion in positions)
                {
                    this.values[posítion] = newValues[0];
                }
                return;
            }
            for (int i = 0; i < positions.Count; i++)
            {
                this.values[i] = newValues[i];
            }
        }

        /// <summary>
        /// reasign the current with the values of other series.
        /// Index of original series still unchange
        /// </summary>
        /// <param name="other"></param>
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
            Support.InferDataType(values);
        }
    }
}
