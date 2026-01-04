namespace DataProcessor.source.NonGenericsSeries.NumericTypeSystem
{
    internal static class NumericPromotion
    {
        /// <summary>
        /// Determines the resulting <see cref="NumericKind"/> when combining two numeric kinds
        /// according to the promotion rules used in this library.
        /// </summary>
        /// <param name="a">The first <see cref="NumericKind"/> value.</param>
        /// <param name="b">The second <see cref="NumericKind"/> value.</param>
        /// <returns>
        /// The promoted <see cref="NumericKind"/> representing the type that can safely
        /// store values from both <paramref name="a"/> and <paramref name="b"/>.
        ///
        /// Promotion rules:
        /// <list type="bullet">
        /// <item>If either <paramref name="a"/> or <paramref name="b"/> is <see cref="NumericKind.Object"/>, the result is <see cref="NumericKind.Object"/>.</item>
        /// <item>When combining <see cref="NumericKind.Decimal"/> and <see cref="NumericKind.Double"/>, the result is <see cref="NumericKind.Double"/> 
        /// to handle values that exceed the range of <see cref="decimal"/>.</item>
        /// <item>Decimal dominates all other numeric types except Double and Object.</item>
        /// <item>Double dominates integer types but is dominated by Decimal unless Decimal is combined with Double (see above).</item>
        /// <item>When combining signed and unsigned integers, the result is promoted to <see cref="NumericKind.Int64"/>.</item>
        /// <item>Otherwise, the wider of the two integer types is chosen.</item>
        /// </list>
        /// </returns>
        internal static NumericKind Promote(NumericKind a, NumericKind b)
        {
            if (a == b) return a;

            if (a == NumericKind.Object || b == NumericKind.Object)
                return NumericKind.Object;
            if ((a == NumericKind.Double && b == NumericKind.Decimal) ||
                (a == NumericKind.Decimal && b == NumericKind.Double))
                return NumericKind.Double;

            if (a == NumericKind.Decimal || b == NumericKind.Decimal)
                return NumericKind.Decimal;

            if (a == NumericKind.Double || b == NumericKind.Double)
                return NumericKind.Double;

            if ((a.IsSignedInteger() && b.IsUnsignedInteger()) ||
                (a.IsUnsignedInteger() && b.IsSignedInteger()))
                return NumericKind.Int64;

            return MaxByWidth(a, b);
        }

        /// <summary>
        /// Returns the wider numeric kind based on bit width.
        /// </summary>
        /// <param name="a">The first numeric kind.</param>
        /// <param name="b">The second numeric kind.</param>
        /// <returns>The numeric kind with greater bit width.</returns>
        private static NumericKind MaxByWidth(NumericKind a, NumericKind b)
        {
            int Width(NumericKind k) => k switch
            {
                NumericKind.Int32 or NumericKind.UInt32 => 32,
                NumericKind.Int64 or NumericKind.UInt64 => 64,
                _ => 0
            };

            return Width(a) >= Width(b) ? a : b;
        }
    }
}
