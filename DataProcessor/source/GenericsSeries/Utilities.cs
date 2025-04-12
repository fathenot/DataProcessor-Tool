using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.NonGenericsSeries;
namespace DataProcessor.source.GenericsSeries
{
    public partial class Series<DataType>
    {
        // utility methods

        public void Sort(Comparer<DataType>? comparer = null)
        {
            if (values == null) return;
            if (comparer == null)
            {
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

        public View GetView(List<object> indicies)
        {
            return new View(this, indicies);
        }

        public View GetView((object start, object end, int step) slice)
        {
            return new View(this, slice);
        }

        public GroupView GroupsByIndex()
        {
            Dictionary<object, int[]> keyValuePairs = new Dictionary<object, int[]>();
            foreach (var idx in this.indexMap.Keys)
            {
                keyValuePairs[idx] = this.indexMap[idx].ToArray();
            }
            return new GroupView(this, keyValuePairs);
        }

        public GroupView GroupByValue()
        {
            Dictionary<object, int[]> keyValuePairs = new Dictionary<object, int[]>();
            HashSet<DataType> removedDuplicate = new(values);
            foreach (var ele in removedDuplicate)
            {
                keyValuePairs[ele] = this.values.Select((value, index) => new { value, index })
                            .Where(x => ele.Equals(x))
                            .Select(x => x.index)
                            .ToList().ToArray();
            }
            return new GroupView(this, keyValuePairs);
        }

        // copy
        public Series<DataType> Clone()
        {
            if (this.values == null)
            {
                return new Series<DataType>(this.name, new List<DataType>());
            }
            return new Series<DataType>(this.name, new List<DataType>(values));
        }

        public void CopyTo(DataType[] array, int arrayIndex)
        {
            values.CopyTo((DataType[])array, arrayIndex);
        }

        public Series ConvertToNonGenerics()
        {
            List<object?> values = new List<object?>();
            values.AddRange(this.values);
            return new Series(values, this.name, this.index);
        }
    }
}
