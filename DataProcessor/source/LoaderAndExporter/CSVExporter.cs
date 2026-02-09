using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.LoaderAndExporter
{
    public class CSVExporter
    {
        /// <summary>
        /// Writes the contents of the given <see cref="DataFrame.DataFrame"/> to a CSV file.
        /// </summary>
        /// <param name="df">The data frame to export. Each entry of <c>df.Series</c> represents a column.</param>
        /// <param name="path">The file system path to write the CSV file to. If the file exists it will be overwritten.</param>
        /// <param name="delimiter">The character used to separate values in the output file. Defaults to comma (',').</param>
        /// <param name="includeHeader">If <c>true</c>, the column names from <c>df.Columns</c> are written as the first line.</param>
        /// <param name="nullValue">The string to write for null values found in the data frame. Defaults to an empty string.</param>
        /// <remarks>
        /// The method writes using UTF-8 encoding. Values are escaped using the CSV quoting rules implemented by <see cref="Escape(string)"/>.
        /// IO-related exceptions (for example, <see cref="System.IO.IOException"/> or <see cref="UnauthorizedAccessException"/>) may be thrown by the underlying file operations.
        /// </remarks>
        public static void ToCSV(DataFrame.DataFrame df, string path, char delimiter = ',', bool includeHeader = true, string nullValue = "")
        {
            using var writer = new StreamWriter(path, false, Encoding.UTF8);
            var columns = df.Columns;
            var series = df.Series;
            int rowCount = df.RowCount;

            // write header
            if (includeHeader)
            {
                writer.WriteLine(string.Join(
                    delimiter,
                    columns.Select(Escape)
                ));
            }

            // write row into file
            for (int row = 0; row < rowCount; row++)
            {
                var values = new string[series.Count];
                for (int col = 0; col < series.Count; col++)
                {
                    var value = series[col].GetValueIntloc(row);
                    values[col] = value is null ? nullValue : Escape(value.ToString()!);
                }
                writer.WriteLine(string.Join(delimiter, values));
            }
        }

        /// <summary>
        /// Escapes a single CSV field value.
        /// </summary>
        /// <param name="value">The value to escape. Must not be <c>null</c>.</param>
        /// <returns>
        /// The escaped field string. If the value contains a double quote, delimiter (comma) or newline character,
        /// the field is wrapped in double quotes and internal double quotes are doubled (as per common CSV conventions).
        /// </returns>
        /// <remarks>
        /// This method currently treats the comma character as a trigger for quoting. The caller is responsible
        /// for passing the appropriate delimiter if delimiters other than comma are used.
        /// </remarks>
        private static string Escape(string value)
        {
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
