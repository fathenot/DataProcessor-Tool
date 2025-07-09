using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.NonGenericsSeries
{
   public partial class Series
    {
        // this partial class contains methods used for IO operations on the series

        /// <summary>
        /// Returns a string representation of the series, including its name, index, and values.
        /// </summary>
        /// <remarks>The returned string includes the series name (or "Unnamed" if the name is null),
        /// followed by a table of indices and their corresponding values. Each value is formatted as "null" if it is
        /// not set.</remarks>
        /// <returns>A formatted string that represents the series, including its name, indices, and values.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Series: {Name ?? "Unnamed"}"); // In tên Series
            sb.AppendLine("Index | Value");
            sb.AppendLine("--------------");
            for (int i = 0; i < values.Count; i++)
            {
                sb.AppendLine($"{index.GetIndex(i).ToString(),5} | {values.GetValue(i)?.ToString() ?? "null"}");
            }
            return sb.ToString();
        }
    }
}
