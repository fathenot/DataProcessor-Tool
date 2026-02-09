using System.Text;

namespace DataProcessor.source.DataFrame
{
    public partial class DataFrame
    {
        public override string ToString() => Show(5);

        public string ShowSchema()
        {
           return schema.ToString();
        }
        public string Show(int n = 20)
        {
            var sb = new StringBuilder();

            int rowCount = frame.Count == 0 ? 0 : frame[0].Count;
            int colCount = frame.Count;

            sb.AppendLine($"DataFrame [{rowCount} rows x {colCount} columns]");
            sb.AppendLine();

            if (rowCount == 0 || colCount == 0)
            {
                sb.AppendLine("<empty DataFrame>");
                return sb.ToString();
            }

            int rowsToShow = Math.Min(n, rowCount);

            const int colWidth = 15;

            // Header
            sb.Append("| Index | ");
            foreach (var col in columnRegistry.Columns)
                sb.Append($"{col,-colWidth} |");
            sb.AppendLine();

            sb.AppendLine(new string('-', 8 + (colWidth + 3) * colCount));

            // Rows
            for (int i = 0; i < rowsToShow; i++)
            {
                sb.Append($"| {index[i],5} | ");

                foreach (var series in frame)
                {
                    var value = series.GetValueIntloc(i) ;
                    var text = value?.ToString() ?? "null";

                    if (text.Length > colWidth)
                        text = text.Substring(0, colWidth - 3) + "...";

                    sb.Append($"{text,-colWidth} |");
                }

                sb.AppendLine();
            }

            if (rowCount > rowsToShow)
                sb.AppendLine($"only showing top {rowsToShow} rows");

            return sb.ToString();
        }

    }
}
