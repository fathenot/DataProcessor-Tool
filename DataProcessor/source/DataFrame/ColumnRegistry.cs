using System;
using System.Collections.Generic;
using System.Linq;

namespace DataProcessor.source.DataFrame
{
    /// <summary>
    /// Provides an internal registry for managing and resolving column names and their corresponding indices within a
    /// collection.
    /// </summary>
    /// <remarks>This class is intended for internal use to efficiently map column names to their positions,
    /// including support for duplicate column names. It enables fast lookups and retrieval of column metadata by name
    /// or index.</remarks>
    internal sealed class ColumnRegistry
    {
        private readonly IReadOnlyList<string> _columns;
        private readonly Dictionary<string, List<int>> _nameToIndices; // improve efficient mapping aka columnname -> position;

        internal ColumnRegistry(IReadOnlyList<string> columns)
        {
            _columns = columns ?? throw new ArgumentNullException(nameof(columns));
            _nameToIndices = new Dictionary<string, List<int>>();
            for(int i = 0; i< _columns.Count; i++)
            {
                var name = _columns[i];
                if (!_nameToIndices.TryGetValue(name, out var indices))
                {
                    indices = new List<int>();
                    _nameToIndices[name] = indices;
                }
                indices.Add(i);
            }
        }

        internal IReadOnlyList<int> Resolve(string columnName)
        {
            return _nameToIndices.TryGetValue(columnName, out var indices)
            ? indices
            : Array.Empty<int>();  // Not found
        }

        internal string? GetColumnName(int index)
        {
            return _columns[index];
        }

        internal int GetNumColumns() => _columns.Count;

        internal bool Contains(string columnName)
        {
            return _nameToIndices.ContainsKey(columnName);
        }

        // Check for duplicates:
        internal bool HasDuplicates()
        {
            return _columns.GroupBy(c => c).Any(g => g.Count() > 1);
        }

        // Get unique column names:
        internal IEnumerable<string?> UniqueColumnNames()
        {
            return _nameToIndices.Keys;
        }

        internal int GetFirstOccurence(string columnName)
        {
            var indices = Resolve(columnName);
            if (indices.Count == 0)
                throw new ArgumentException($"Column '{columnName}' not found");
            return indices[0];
        }

        internal IEnumerable<string> UniqueColumns => this._nameToIndices.Keys;
        internal IReadOnlyList<string> Columns => _columns;
    }
}
