using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.GenericsSeries;

namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series
    {
        public GroupView GroupByIndex()
        {
            Dictionary<object, int[]> groups = new Dictionary<object, int[]>();
            foreach (var Index in this.indexMap.Keys)
            {
                groups[Index] = this.indexMap[Index].ToArray();
            }
            return new GroupView(this, groups);
        }

        public GroupView GroupByValue()
        {
            Dictionary<object, int[]> groups = new Dictionary<object, int[]>();
            var removedDuplicate = new HashSet<object?>(this.values);
            foreach (var Element in removedDuplicate)
            {
                int[] indicies = this.values.Select((value, index) => new { value, index })
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
            values.CopyTo((object?[])array, arrayIndex);
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
            return new Series<DataType>(this.seriesName, newValues, this.index);
        }

        public void ResetIndex()
        {
            this.index = Enumerable.Range(0, values.Count - 1).Cast<object>().ToList();
            this.indexMap.Clear();
            foreach (var idx in index)
            {
                this.indexMap[idx] = new List<int> { Convert.ToInt32(idx) };
            }
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

            var result = new Series(newValues, this.Name, this.index)
            {
                dataType = newType // bảo toàn kiểu dữ liệu gốc tránh bị hệ thống suy luận kiểu làm sai kiểu dữ liệu
            };
            return result;
        }

        public void Sort(Comparer<object?>? comparer = null)
        {
            if (values == null) { return; }
            if (comparer == null)
            {
                if (values.Any(x => x != null && x is not IComparable))
                {
                    throw new InvalidOperationException("All non-null values must implement IComparable for sorting.");
                }
                // Sử dụng OrderBy với logic null được đưa về cuối
                values = values.OrderBy(x => x != null)
                               .ThenBy(x => x as IComparable)  // Đảm bảo sắp xếp bình thường sau khi xử lý null
                               .Concat(values.Where(x => x == null))
                               .ToList();
            }
            else
            {
                // Sử dụng OrderBy với comparer, với logic null được đưa về cuối
                values = values.OrderBy(x => x == null ? 1 : 0)
                               .ThenBy(x => x, comparer)  // Sắp xếp với comparer nếu có
                               .ToList();
            }
        }
    }
}
