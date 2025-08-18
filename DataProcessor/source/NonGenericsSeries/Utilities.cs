using DataProcessor.source.GenericsSeries;
using DataProcessor.source.Index;
using DataProcessor.source.ValueStorage;

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
            Dictionary<object, List<int>> groups = new Dictionary<object, List<int>>();
            foreach (var Index in this.index.DistinctIndices())
            {
                groups[Index] = this.index.GetIndexPosition(Index).ToList();
            }
            return new GroupView(this, groups);
        }


        public GroupView GroupByValue()
        {
            Dictionary<object, List<int>> groups = new Dictionary<object, List<int>>();
            for (int i = 0; i < this.valueStorage.Count; i++)
            {
                if (groups.ContainsKey(this.valueStorage[i]!))
                {
                    groups[this.valueStorage[i]!].Add(i);
                }

                else if (this.valueStorage[i] != null && !groups.ContainsKey(this.valueStorage[i]!))
                {
                    groups[this.valueStorage[i]!] = new List<int>() { i };
                }

            }
            return new GroupView(this, groups);
        }

        public ISeries Clone()
        {
            return new Series(this, true);
        }

        public void CopyTo(object?[] array, int arrayIndex)
        {
            if (this.valueStorage == null)
            {
                return;
            }
            this.valueStorage.ToList().CopyTo(array, arrayIndex);
        }

        public Series<DataType> ConvertToGenerics<DataType>() where DataType : notnull
        {
            var newValues = new List<DataType>(this.valueStorage.Count);
            foreach (var v in this.valueStorage)
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

        /// <summary>
        /// Converts the elements of the current series to the specified type.
        /// </summary>
        /// <remarks>This method supports conversion to common types, including enums and <see
        /// cref="DateTime"/>.  For enums, string and integer values are supported if they match the target enum type. 
        /// For <see cref="DateTime"/>, string values are parsed using <see cref="DateTime.TryParse(string, out
        /// DateTime)"/>.</remarks>
        /// <param name="newType">The target type to which the elements of the series should be converted. This type must be a valid .NET
        /// type.</param>
        /// <param name="forceCast">A boolean value indicating whether to enforce strict type conversion.  If <see langword="true"/>, an <see
        /// cref="InvalidCastException"/> is thrown for any element that cannot be converted.  If <see
        /// langword="false"/>, elements that cannot be converted are replaced with <see cref="DBNull.Value"/>.</param>
        /// <returns>A new series with elements converted to the specified type.  Elements that cannot be converted are replaced
        /// with <see cref="DBNull.Value"/> unless <paramref name="forceCast"/> is <see langword="true"/>.</returns>
        /// <exception cref="InvalidCastException">Thrown if <paramref name="forceCast"/> is <see langword="true"/> and an element cannot be converted to
        /// <paramref name="newType"/>.</exception>
        public ISeries AsType(Type newType, bool forceCast = false)
        {
            ArgumentNullException.ThrowIfNull(newType);

            var newValues = new List<object?>(this.valueStorage.Count);
            // add new value to newValues to create new Series
            foreach (var v in this.valueStorage)
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
        public Series SortValues(bool ascending = true, bool nullsFirst = false)
        {
            switch (this.valueStorage)
            {
                case Int32ValuesStorage int32ValuesStorage:
                    {
                        // Xử lý khi storage là kiểu int
                        var nonNullVals = int32ValuesStorage.NonNullValues;
                        var ListedIndex = this.index.ToList();
                        EngineWrapper.SortingEngine.IndexValueSorter.SortByValue(nonNullVals, this.index.ToList(), ascending);
                        var ListedValues = new List<object?>();
                        if (!nullsFirst)
                        {
                            foreach (var val in nonNullVals)
                            {
                                ListedValues.Add(val);
                            }
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);

                            }
                            foreach (var val in nonNullVals)
                            {
                                ListedValues.Add(val);
                            }
                        }
                        return new Series(ListedValues, ListedIndex, this.dataType, this.seriesName);
                    }

                case Int64ValuesStorage intStorage:
                    {
                        // Xử lý khi storage là kiểu int
                        var nonNullVals = intStorage.NonNullValues;
                        var ListedIndex = this.index.ToList();
                        EngineWrapper.SortingEngine.IndexValueSorter.SortByValue(nonNullVals, this.index.ToList(), ascending);
                        var ListedValues = new List<object?>();
                        if (!nullsFirst)
                        {
                            foreach (var val in nonNullVals)
                            {
                                ListedValues.Add(val);
                            }
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);

                            }
                            foreach (var val in nonNullVals)
                            {
                                ListedValues.Add(val);
                            }
                        }
                        return new Series(ListedValues, ListedIndex, this.dataType, this.seriesName);
                    }

                case DoubleValueStorage doubleStorage:
                    {
                        // Xử lý khi storage là kiểu double
                        var nonNullVals = doubleStorage.NonNullValues;
                        var ListedIndex = this.index.ToList();
                        EngineWrapper.SortingEngine.IndexValueSorter.SortByValue(nonNullVals, this.index.ToList(), ascending);
                        var ListedValues = new List<object?>();
                        if (!nullsFirst)
                        {
                            ListedValues.AddRange(nonNullVals);
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                                ListedValues.AddRange(nonNullVals);
                            }
                        }
                        return new Series(ListedValues, ListedIndex, this.dataType, this.seriesName);
                    }

                case DateTimeStorage dateStorage:
                    {
                        // Xử lý DateTime
                        var nonNullVals = dateStorage.NonNullValues;
                        var ListedIndex = this.index.ToList();
                        EngineWrapper.SortingEngine.IndexValueSorter.SortByValue(nonNullVals, this.index.ToList(), ascending);
                        var ListedValues = new List<object?>();
                        if (!nullsFirst)
                        {
                            ListedValues.AddRange(nonNullVals);
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                                ListedValues.AddRange(nonNullVals);
                            }
                        }
                        return new Series(ListedValues, ListedIndex, this.dataType, this.seriesName);
                    }

                case StringStorage stringStorage:
                    {
                        // Xử lý khi storage là kiểu string
                        var nonNullVals = stringStorage.NonNullValues;
                        var ListedIndex = this.index.ToList();
                        EngineWrapper.SortingEngine.IndexValueSorter.SortByValue(nonNullVals, this.index.ToList(), ascending);
                        var ListedValues = new List<object?>();
                        if (!nullsFirst)
                        {
                            ListedValues.AddRange(nonNullVals);
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                                ListedValues.AddRange(nonNullVals);
                            }
                        }
                        return new Series(ListedValues, ListedIndex, this.dataType, this.seriesName);
                    }
                case DecimalStorage decimalStorage:
                    {
                        // Xử lý khi storage là kiểu decimal
                        var nonNullVals = decimalStorage.NonNullValues;
                        var ListedIndex = this.index.ToList();
                        EngineWrapper.SortingEngine.IndexValueSorter.SortByValue(nonNullVals, this.index.ToList(), ascending);
                        var ListedValues = new List<object?>();
                        if (!nullsFirst)
                        {
                            ListedValues.AddRange(nonNullVals);
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                                ListedValues.AddRange(nonNullVals);
                            }
                        }
                        return new Series(ListedValues, ListedIndex, this.dataType, this.seriesName);
                    }
                case CharStorage charStorage:
                    {
                        // Xử lý khi storage là kiểu char
                        var nonNullVals = charStorage.NonNullValues;
                        var ListedIndex = this.index.ToList();
                        EngineWrapper.SortingEngine.IndexValueSorter.SortByValue(nonNullVals, this.index.ToList(), ascending);
                        var ListedValues = new List<object?>();
                        if (!nullsFirst)
                        {
                            ListedValues.AddRange(nonNullVals);
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < this.valueStorage.NullIndices.Count(); i++)
                            {
                                ListedValues.Add(null);
                                ListedValues.AddRange(nonNullVals);
                            }
                        }
                        return new Series(ListedValues, ListedIndex, this.dataType, this.seriesName);
                    }

                default:
                    throw new NotSupportedException("Unsupported value storage: " + this.valueStorage.GetType().Name);
            }

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
            var sortedValues = new List<object?>(this.valueStorage.Count);
            foreach (var idx in sortedIndices)
            {
                sortedValues.Add(this.valueStorage[idx]);
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
            var reversedValues = new List<object?>(this.valueStorage);
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
            for (int i = 0; i < this.Count; i++)
            {
                extendedindex.Add(index[i]);
                extendedValues.Add(this.valueStorage[i]);
            }

            for (int i = 0; i < other.Count; i++)
            {
                extendedindex.Add(other.index[i]);
                extendedValues.Add(other.valueStorage[i]);
            }

            if (this.index.GetType() == typeof(RangeIndex) && other.index.GetType() == typeof(RangeIndex))
            {
                return new Series(extendedValues, null, name: this.seriesName);
            }
            return new Series(extendedValues, extendedindex, dtype: null, this.seriesName);
        }

        public dynamic Sum()
        {
            string result = string.Empty;
            switch (this.valueStorage)
            {
                case Int64ValuesStorage intStorage:
                    {
                        // Xử lý khi storage là kiểu int
                        return EngineWrapper.ComputationEngine.CalculateSum.ComputeSum(intStorage.NonNullValues);
                    }
                case DoubleValueStorage doubleStorage:
                    {
                        // Xử lý khi storage là kiểu double
                        return EngineWrapper.ComputationEngine.CalculateSum.ComputeSum(doubleStorage.NonNullValues);
                    }
                case StringStorage stringStorage:
                    {
                        // Xử lý khi storage là kiểu string
                        return string.Join("", stringStorage.NonNullValues);
                    }
                case DecimalStorage decimalStorage:
                    {
                        // Xử lý khi storage là kiểu decimal
                        return EngineWrapper.ComputationEngine.CalculateSum.ComputeSum(decimalStorage.NonNullValues).ToString();
                    }
                case CharStorage charStorage:
                    {
                        // Xử lý khi storage là kiểu char
                        return string.Join("", charStorage.NonNullValues);
                    }
                default:
                    {
                        throw new NotSupportedException($"Unsupport this type of data {this.GetType()}");
                    }

            }
        }
    }
}