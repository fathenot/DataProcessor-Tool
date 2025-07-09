using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.GenericsSeries;
using DataProcessor.source.Index;

namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series
    {
        /// <summary>
        /// Groups the elements of the current object by their distinct indices.
        /// </summary>
        /// <remarks>This method creates a mapping of distinct indices to their corresponding positions
        /// within the current object's index structure. The resulting grouping is returned as a <see cref="GroupView"/>
        /// object, which provides access to the grouped data.</remarks>
        /// <returns>A <see cref="GroupView"/> object containing the grouped elements, where each distinct index is mapped to an
        /// array of positions.</returns>
        public GroupView GroupByIndex()
        {
            Dictionary<object, int[]> groups = new Dictionary<object, int[]>();
            foreach (var Index in this.index.DistinctIndices())
            {
                groups[Index] = this.index.GetIndexPosition(Index).ToArray();
            }
            return new GroupView(this, groups);
        }

        public GroupView GroupByValue()
        {
            Dictionary<object, int[]> groups = new Dictionary<object, int[]>();
            List<object?> allValues = new List<object?>();
            for (int Index = 0; Index < values.Count; Index++)
            {
                allValues.Add(this.values.GetValue(Index));
            }

            var removedDuplicate = new HashSet<object?>(allValues);
            foreach (var Element in removedDuplicate)
            {
                int[] indicies = allValues.Select((value, index) => new { value, index })
                                        .Where(x => Object.Equals(x, Element))
                                        .Select(x => x.index)
                                        .ToArray();
                if (Element == null)
                {
                    object convertedNullValue = DBNull.Value;
                    groups[convertedNullValue] = indicies;
                }
                else
                {
                    groups[Element] = indicies;
                }

            }
            return new GroupView(this, groups);

        }

        public ISeries Clone()
        {
            return new Series(this);
        }

        public void CopyTo(object?[] array, int arrayIndex)
        {
            if (values == null)
            {
                return;
            }
            values.ToList().CopyTo(array, arrayIndex);
        }

        public Series<DataType> ConvertToGenerics<DataType>() where DataType : notnull
        {
            var newValues = new List<DataType>(values.Count);
            foreach (var v in values)
            {
                if (v == null || v == DBNull.Value)
                {
                    newValues.Add(default!); // Giá trị mặc định của T
                    continue;
                }

                try
                {
                    // Nếu v đã là DataType, thêm vào luôn
                    if (v is DataType castedValue)
                    {
                        newValues.Add(castedValue);
                    }
                    // Xử lý chuyển đổi kiểu dữ liệu
                    else
                    {
                        object convertedValue = Convert.ChangeType(v, typeof(DataType));
                        newValues.Add((DataType)convertedValue);
                    }
                }
                catch
                {
                    newValues.Add(default!);
                }
            }
            return new Series<DataType>(newValues, this.Name, this.index.ToList());
        }


        public ISeries AsType(Type newType, bool forceCast = false)
        {
            ArgumentNullException.ThrowIfNull(newType);

            var newValues = new List<object?>(values.Count);
            // add new value to newValues to create new Series
            foreach (var v in values)
            {
                if (v == null || v == DBNull.Value)
                {
                    newValues.Add(DBNull.Value);
                    continue;
                }

                try
                {
                    if (newType.IsEnum)
                    {
                        if (v is string strEnum && Enum.TryParse(newType, strEnum, true, out object? enumValue))
                        {
                            newValues.Add(enumValue);
                        }
                        else if (v is int intEnum && Enum.IsDefined(newType, intEnum))
                        {
                            newValues.Add(Enum.ToObject(newType, intEnum));
                        }
                        else
                        {
                            if (forceCast) throw new InvalidCastException($"Cannot convert {v} to {newType}");
                            newValues.Add(DBNull.Value);
                        }
                    }

                    else if (newType == typeof(DateTime) && v is string str)
                    {
                        newValues.Add(DateTime.TryParse(str, out DateTime dt) ? dt :
                                      (forceCast ? throw new InvalidCastException($"Cannot convert {str} to DateTime") : DBNull.Value));
                    }
                    else
                    {
                        newValues.Add(Convert.ChangeType(v, newType));
                    }
                }
                catch
                {
                    if (forceCast)
                        throw new InvalidCastException($"Cannot convert {v} to {newType}");
                    else
                        newValues.Add(DBNull.Value);
                }
            }

            var result = new Series(newValues, Index, newType, name: this.seriesName)
            {
                dataType = newType // bảo toàn kiểu dữ liệu gốc tránh bị hệ thống suy luận kiểu làm sai kiểu dữ liệu
            };
            return result;
        }

        /// <summary>
        /// Sorts the values in the series using the specified comparer and returns a new series with the sorted values.
        /// </summary>
        /// <remarks>The sorting operation does not modify the current series. Instead, it creates and
        /// returns a new series with the sorted values. The original index is retained in the returned series, ensuring
        /// that the relationship between values and their indices remains consistent.</remarks>
        /// <param name="comparer">An optional comparer used to determine the order of the values. If <see langword="null"/>, the default
        /// comparer for the value type is used.</param>
        /// <returns>A new <see cref="Series"/> instance containing the values sorted according to the specified comparer, with
        /// the original index preserved.</returns>
        public Series SortValues(Comparer<object?>? comparer = null)
        {
            if (comparer == null)
            {
                comparer = Comparer<object?>.Default;
            }
            // Sort the values and index together based on the values
            var sortedIndices = values.Select((value, index) => new { value, index })
                                      .OrderBy(x => x.value, comparer)
                                      .Select(x => x.index)
                                      .ToList();
            // Create new sorted lists
            var sortedValues = new List<object?>(values.Count);
            foreach (var idx in sortedIndices)
            {
                sortedValues.Add(values[idx]);
            }

            return new Series(sortedValues,
                index: index.ToList(),
                dtype: this.dataType,
                name: this.seriesName);

        }

        /// <summary>
        /// Sorts the index of the series using the specified comparer and reorders the values accordingly.
        /// </summary>
        /// <remarks>This method creates a new series with the index sorted based on the specified
        /// comparer. The values are reordered to maintain their association with the original index elements. If no
        /// comparer is provided, the default comparer for the index element type is used.</remarks>
        /// <param name="comparer">An optional comparer used to determine the order of the index elements. If <see langword="null"/>, the
        /// default comparer for the index element type is used.</param>
        /// <returns>A new <see cref="Series"/> instance with the index sorted and the values reordered to match the new index
        /// order.</returns>
        public Series SortIndex(Comparer<object?>? comparer = null)
        {
            if (comparer == null)
            {
                comparer = Comparer<object?>.Default;
            }
            // Sort the index and values together based on the index
            var sortedIndices = index.Select((idx, i) => new { idx, i })
                                      .OrderBy(x => x.idx, comparer)
                                      .Select(x => x.i)
                                      .ToList();
            // Create new sorted lists
            var sortedValues = new List<object?>(values.Count);
            foreach (var idx in sortedIndices)
            {
                sortedValues.Add(values[idx]);
            }
            return new Series(sortedValues,
                index: index.ToList(),
                dtype: this.dataType,
                name: this.seriesName);
        }

        /// <summary>
        /// Creates a new <see cref="Series"/> instance with the values and index reversed.
        /// </summary>
        /// <remarks>The returned <see cref="Series"/> will have its values and index in reverse order
        /// compared to the current instance. The data type and series name are preserved in the reversed
        /// series.</remarks>
        /// <returns>A new <see cref="Series"/> instance with reversed values and index.</returns>
        public Series Reverse()
        {
            // Reverse the values and index
            var reversedValues = new List<object?>(values);
            reversedValues.Reverse();
            var reversedIndex = new List<object>(index);
            reversedIndex.Reverse();
            return new Series(reversedValues, reversedIndex, this.dataType, this.seriesName);
        }
        
        /// <summary>
        /// Combines the current series with another series, creating a new series that contains the values and indices
        /// from both.
        /// </summary>
        /// <remarks>The method appends the values and indices of the specified <paramref name="other"/>
        /// series to the current series. If both series have range-based indices, the resulting series will not include
        /// an explicit index. Otherwise, the indices from both series are combined into the resulting series.</remarks>
        /// <param name="other">The series to combine with the current series. Must not be null.</param>
        /// <returns>A new <see cref="Series"/> instance containing the combined values and indices from the current series and
        /// the specified <paramref name="other"/> series. If both series have range-based indices, the resulting series
        /// will have a null index.</returns>
        public Series Extend(Series other)
        {
            List<object?> extendedValues = new List<object?>();
            List<object> extendedindex = new List<object>();
            for(int i = 0; i < this.Count; i++)
            {
                extendedindex.Add(index[i]);
                extendedValues.Add(values[i]);
            }

            for(int i = 0; i < other.Count; i++)
            {
                extendedindex.Add(other.index[i]);
                extendedValues.Add(other.values[i]);
            }

            if(this.index.GetType() == typeof(RangeIndex) && other.index.GetType() == typeof(RangeIndex))
            {
                return new Series(extendedValues, null, name: this.seriesName);
            }
            return new Series(extendedValues, extendedindex, dtype: null, this.seriesName);
        }
    }
}
