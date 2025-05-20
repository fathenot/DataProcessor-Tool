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
        public RangeIndex(int start, int stop, int step = 1)
            : base(new List<object>())  // Base gọi constructor của IIndex, khởi tạo indexList trống
        {
            _start = start;
            _stop = stop;
            _step = step;

            // Chỉ cần cập nhật map cho RangeIndex
        }

        public override int Count => (_stop - _start) / _step;
        public override IReadOnlyList<object> IndexList
        {
            get
            {
               return  this.DistinctIndices().ToList();
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
            return _start + idx * _step;
        }

        // Override phương thức để lấy vị trí của một key
        public override IReadOnlyList<int> GetIndexPosition(object index)
        {
            return new List<int>{ FirstPositionOf(index) };
        }

        public override int FirstPositionOf(object key)
        {
            return Convert.ToInt32(key) - _start;
        }

        public override bool Contains(object key)
        {
            var tmp = Convert.ToInt32(key);
            int i = 0;
            while (i < tmp)
            {
                i += _step;
            }
            return i == tmp;
        }

        public override IEnumerable<object> DistinctIndices()
        {
            List<object> tmp = new List<object>();
            for(int i = _start; i != _stop; i+= _step)
            {
                tmp.Add(Convert.ToInt32(i));
            }
            return tmp;
        }

        public override IEnumerator<object> GetEnumerator()
        {
            for (int i = _start; i != _stop; i += _step)
            {
                yield return i;
            }
        }
    }
}
