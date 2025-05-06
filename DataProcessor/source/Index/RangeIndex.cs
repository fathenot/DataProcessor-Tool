using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Index
{
    public class RangeIndex : IIndex
    {
        private readonly int _start;
        private readonly int _stop;
        private readonly int _step;

        // Constructor
        public RangeIndex(int start, int stop, int step)
            : base(new List<object>())  // Base gọi constructor của IIndex, khởi tạo indexList trống
        {
            _start = start;
            _stop = stop;
            _step = step;

            // Chỉ cần cập nhật map cho RangeIndex
            GenerateIndex();
        }

        // Generate các giá trị index
        private void GenerateIndex()
        {
            if (_step > 0)
            {
                for (int i = _start; i < _stop; i += _step)
                {
                    indexList.Add(i);
                    if (!indexMap.ContainsKey(i))
                    {
                        indexMap[i] = new List<int>();
                    }
                    indexMap[i].Add(indexList.Count - 1);  // Đảm bảo map index đến vị trí
                }
            }
            else if (_step < 0)
            {
                for (int i = _start; i > _stop; i += _step)
                {
                    indexList.Add(i);
                    if (!indexMap.ContainsKey(i))
                    {
                        indexMap[i] = new List<int>();
                    }
                    indexMap[i].Add(indexList.Count - 1);  // Đảm bảo map index đến vị trí
                }
            }
            else
            {
                throw new ArgumentException("Step cannot be zero.");
            }
        }

        // Override phương thức slice
        public override IIndex Slice(int start, int end, int step = 1)
        {
            // Tính toán start, stop mới cho slice
            int actualStart = _start + start * _step;
            int actualStop = _start + end * _step;
            int combinedStep = _step * step;

            // Trả về một RangeIndex mới đã slice
            return new RangeIndex(actualStart, actualStop, combinedStep);
        }

        // Override phương thức để lấy vị trí của index
        public override object GetIndex(int idx)
        {
            return indexList[idx];
        }

        // Override phương thức để lấy vị trí của một key
        public override IReadOnlyList<int> GetIndexPosition(object index)
        {
            if (indexMap.ContainsKey(index))
            {
                return indexMap[(int)index];
            }
            throw new KeyNotFoundException($"Index {index} not found");
        }

        protected override void Add(object key)
        {
            throw new InvalidOperationException("Cannot add to RangeIndex. It is immutable.");
        }

        protected override void Drop(object key)
        {
            throw new InvalidOperationException("Cannot add to RangeIndex. It is immutable.");
        }
    }
}
