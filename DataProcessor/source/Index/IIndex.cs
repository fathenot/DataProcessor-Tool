namespace DataProcessor.source.Index
{
    public abstract class IIndex
    {
        protected IIndex() { }
        protected IIndex(List<object> indexList) { }

        public abstract object GetIndex(int idx);

        public abstract IReadOnlyList<int> GetIndexPosition(object index);

        public abstract int Count { get; }
        public abstract IReadOnlyList<object> IndexList { get; }

        public abstract IIndex Slice(int start, int end, int step = 1);

        public abstract bool Contains(object key);

        public abstract int FirstPositionOf(object key);

        public abstract IEnumerable<object> DistinctIndices();

        public abstract IEnumerator<object> GetEnumerator();

    }

}
