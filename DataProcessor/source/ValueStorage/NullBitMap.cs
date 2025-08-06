namespace DataProcessor.source.ValueStorage
{
    /// <summary>
    /// Represents a bitmap to track null states for a collection of elements.
    /// Each bit in the bitmap corresponds to the null state of an element.
    /// </summary>
    internal class NullBitMap
    {
        // List of 32-bit unsigned integers, each representing 32 bits of null flags.
        private uint[] chunks;

        /// <summary>
        /// Initializes a new instance of the <see cref="NullBitMap"/> class
        /// with the capacity to track null states for the specified number of items.
        /// </summary>
        /// <param name="totalItems">The total number of elements to track.</param>
        internal NullBitMap(int totalItems)
        {
            int numChunks = (totalItems + 31) / 32;  // Calculate number of 32-bit chunks needed
            chunks = new uint[numChunks];  // Initialize chunks with zeros

            for (int i = 0; i < numChunks; i++)
            {
                chunks[i] = 0;  // Ensure all bits are initially cleared (not null)
            }

        }

        /// <summary>
        /// Sets or clears the null flag for the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <param name="isNull">If set to <c>true</c>, marks the element as null; otherwise, not null.</param>
        internal void SetNull(int index, bool isNull)
        {
            int chunkIndex = index / 32;  // Identify which chunk holds the bit for this index
            int bitIndex = index % 32;    // Identify the bit position within the chunk (0-31)
            if (isNull)
            {
                chunks[chunkIndex] |= (1U << bitIndex);  // Set the bit to 1 indicating null
            }
            else
            {
                chunks[chunkIndex] &= ~(1U << bitIndex);  // Clear the bit to 0 indicating not null
            }
        }

        /// <summary>
        /// Checks whether the element at the specified index is marked as null.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns><c>true</c> if the element is null; otherwise, <c>false</c>.</returns>
        internal bool IsNull(int index)
        {
            int chunkIndex = index / 32;  // Identify chunk containing the bit
            int bitIndex = index % 32;    // Identify bit position within chunk

            // Check if the bit at bitIndex is set (1) meaning null
            return (chunks[chunkIndex] & (1U << bitIndex)) != 0;
        }

        /// <summary>
        /// Returns a copy of the internal chunks as an array.
        /// </summary>
        /// <returns>An array of <see cref="uint"/> representing the bitmap chunks.</returns>
        internal uint[] ToArray()
        {
            return chunks;
        }

        /// <summary>
        /// Counts the total number of elements marked as null in the bitmap.
        /// </summary>
        /// <returns>The count of null elements.</returns>
        internal int CountNulls()
        {
            int count = 0;
            foreach (var chunk in chunks)
            {
                count += CountBits(chunk);
            }
            return count;
        }

        /// <summary>
        /// Counts the number of set bits (1s) in the given 32-bit unsigned integer.
        /// Uses Brian Kernighan’s algorithm for efficient bit counting.
        /// </summary>
        /// <param name="value">The value to count bits in.</param>
        /// <returns>The number of set bits.</returns>
        private int CountBits(uint value)
        {
            int count = 0;
            while (value != 0)
            {
                value &= (value - 1); // Clear the least significant bit set
                count++;
            }
            return count;
        }

        /// <summary>
        /// Creates a shallow clone of the current <see cref="NullBitMap"/>.
        /// </summary>
        /// <returns>A new <see cref="NullBitMap"/> instance with the same chunk data.</returns>
        internal NullBitMap Clone()
        {
            var clone = new NullBitMap(chunks.Length);
            for (int i = 0; i < chunks.Length; i++)
            {
                clone.chunks[i] = chunks[i]; // Copy each chunk to the new instance
            }
            return clone;
        }
    }
}