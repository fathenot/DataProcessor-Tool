namespace DataProcessor.source.ValueStorage
{
    internal class NullBitMap
    {
        List<uint> chunks;

        public NullBitMap(int totalItems)
        {
            chunks = new List<uint>();
            int numChunks = (totalItems + 63) / 64;  // Tính số chunk cần thiết
            for (int i = 0; i < numChunks; i++)
            {
                chunks.Add(0);  // Mỗi chunk là 64 bit (1 uint)
            }
        }

        public void SetNull(int index, bool isNull)
        {
            int chunkIndex = index / 64;  // Xác định chunk quản lý phần tử này
            int bitIndex = index % 64;    // Xác định bit trong chunk (0-63)
            if (isNull)
            {
                chunks[chunkIndex] |= (1U << bitIndex);  // Đặt bit
            }
            else
            {
                chunks[chunkIndex] &= ~(1U << bitIndex);  // Xóa bit
            }
        }

        public bool IsNull(int index)
        {
            // Tính toán chunk index và bit index
            int chunkIndex = index / 64;  // Xác định chunk
            int bitIndex = index % 64;    // Xác định vị trí bit trong chunk (0-63)

            // Dịch bit về vị trí cần kiểm tra và lấy giá trị của bit cuối cùng
            return (chunks[chunkIndex] & (1UL << bitIndex)) != 0;  // Kiểm tra bit
        }

        public uint[] ToArray()
        {
            return chunks.ToArray();
        }

        public int CountNulls()
        {
            int count = 0;
            foreach (var chunk in chunks)
            {
                count += CountBits(chunk);
            }
            return count;
        }

        private int CountBits(uint value)
        {
            // Brian Kernighan’s Algorithm
            int count = 0;
            while (value != 0)
            {
                value &= (value - 1);
                count++;
            }
            return count;
        }

        public NullBitMap Clone()
        {
            var clone = new NullBitMap(0);
            clone.chunks.AddRange(this.chunks);
            return clone;
        }
    }
}
