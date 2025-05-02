namespace DataProcessor.source.ValueStorage
{
    internal abstract class ValueStorage
    {
        public abstract Type ValueType { get; }
        public abstract int Length { get; }
        public abstract IEnumerable<int> NullPositions { get; }
        public abstract object? GetValue(int index);
        public abstract void SetValue(int index, object? value);
        public abstract nint GetArrayAddress();
    }
}
