namespace DataProcessor.source.NonGenericsSeries.NumericTypeSystem
{
    /// <summary>
    /// Represents the canonical numeric types used for type inference in DataSharp.
    /// </summary>
    internal enum NumericKind
    {
        /// <summary>
        /// Signed 32-bit integer (int, short, byte).
        /// </summary>
        Int32,

        /// <summary>
        /// Signed 64-bit integer (long).
        /// </summary>
        Int64,

        /// <summary>
        /// Unsigned 32-bit integer (uint).
        /// </summary>
        UInt32,

        /// <summary>
        /// Unsigned 64-bit integer (ulong).
        /// </summary>
        UInt64,

        /// <summary>
        /// Decimal type for high-precision numeric values.
        /// </summary>
        Decimal,

        /// <summary>
        /// Double-precision floating point.
        /// </summary>
        Double,

        /// <summary>
        /// Single-precision floating point (float).
        /// </summary>
        Float,

        /// <summary>
        /// Represents a non-numeric or mixed-type value.
        /// </summary>
        Object
    }

    /// <summary>
    /// Provides extension methods for <see cref="NumericKind"/> to check
    /// whether a numeric kind is signed or unsigned.
    /// </summary>
    internal static class NumericKindExtensions
    {
        /// <summary>
        /// Determines whether the specified <see cref="NumericKind"/> represents a signed integer type.
        /// </summary>
        /// <param name="kind">The numeric kind to check.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="kind"/> is <see cref="NumericKind.Int32"/> or <see cref="NumericKind.Int64"/>; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsSignedInteger(this NumericKind kind) =>
            kind == NumericKind.Int32 || kind == NumericKind.Int64;

        /// <summary>
        /// Determines whether the specified <see cref="NumericKind"/> represents an unsigned integer type.
        /// </summary>
        /// <param name="kind">The numeric kind to check.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="kind"/> is <see cref="NumericKind.UInt32"/> or <see cref="NumericKind.UInt64"/>; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsUnsignedInteger(this NumericKind kind) =>
            kind == NumericKind.UInt32 || kind == NumericKind.UInt64;

        /// <summary>
        /// Maps a <see cref="Type"/> to the corresponding <see cref="NumericKind"/>.
        /// </summary>
        /// <param name="type">The type to map.</param>
        /// <returns>
        /// The <see cref="NumericKind"/> representing the numeric type, 
        /// or <see cref="NumericKind.Object"/> if the type is not a recognized numeric type.
        /// </returns>
        internal static NumericKind GetNumericKind(Type type)
        {
            if (type == typeof(nint))
                return IntPtr.Size == 4 ? NumericKind.Int32 : NumericKind.Int64;

            if (type == typeof(nuint))
                return IntPtr.Size == 4 ? NumericKind.UInt32 : NumericKind.UInt64;

            return type switch
            {
                Type t when t == typeof(int) ||
                            t == typeof(short) ||
                            t == typeof(byte) ||
                            t == typeof(sbyte) ||
                            t == typeof(ushort) => NumericKind.Int32,
                
                Type t when t == typeof(long) => NumericKind.Int64,
                Type t when t == typeof(uint) => NumericKind.UInt32,
                Type t when t == typeof(ulong) => NumericKind.UInt64,
                Type t when t == typeof(float) ||
                            t == typeof(double) => NumericKind.Double,
                Type t when t == typeof(decimal) => NumericKind.Decimal,
                _ => NumericKind.Object
            };
        }

        /// <summary>
        /// Determines whether the specified <see cref="NumericKind"/> represents any integer type.
        /// </summary>
        /// <param name="kind">The numeric kind to check.</param>
        /// <returns><c>true</c> if <paramref name="kind"/> is a signed or unsigned integer; otherwise, <c>false</c>.</returns>
        internal static bool IsInteger(this NumericKind kind) =>
            kind.IsSignedInteger() || kind.IsUnsignedInteger();
    }

}
