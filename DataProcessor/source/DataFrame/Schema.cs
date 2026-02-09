using System.Text;
namespace DataProcessor.source.DataFrame
{
    /// <summary>
    /// Represents a schema definition consisting of column names and their associated data types.
    /// </summary>
    /// <remarks>The Schema class provides a way to describe the structure of tabular data by associating each
    /// column name with its corresponding .NET type. This class is intended for internal use and is not
    /// thread-safe.</remarks>
    internal class Schema
    {
        private List<ValueTuple<string, Type>> schema; // string is for column name and type is for data type of column

        public Schema(List<(string, Type)> schema)
        {
            this.schema = schema;
        }

        public IReadOnlyList<ValueTuple<string, Type>> ShowSchema()
        {
            return this.schema;
        }

        public override string ToString()
        {
            if (schema == null || schema.Count == 0)
                return "Schema: <empty>";

            var sb = new StringBuilder();
            sb.AppendLine("Schema:");
            sb.AppendLine("--------------------------------");
            sb.AppendLine(string.Format("{0,-20} | {1}", "Column", "Type"));
            sb.AppendLine("--------------------------------");

            foreach (var (name, type) in schema)
            {
                sb.AppendLine(string.Format("{0,-20} | {1}", name, type.Name));
            }

            sb.AppendLine("--------------------------------");
            return sb.ToString();
        }

    }
}
