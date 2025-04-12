using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.NonGenericsSeries
{
   public partial class Series
    {
        // show the series
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Series: {Name ?? "Unnamed"}"); // In tên Series
            sb.AppendLine("Index | Value");
            sb.AppendLine("--------------");
            for (int i = 0; i < values.Count; i++)
            {
                sb.AppendLine($"{index[i].ToString(),5} | {values[i]?.ToString() ?? "null"}");
            }
            return sb.ToString();
        }
    }
}
