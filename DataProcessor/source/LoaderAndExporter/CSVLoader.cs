using CsvHelper;
using CsvHelper.Configuration;
using DataProcessor.source.DataFrame;
using Microsoft.Win32;
using System.Globalization;
using System.Text;
using DataProcessor.source.NonGenericsSeries;

namespace DataProcessor.source.LoaderAndExporter
{
    public static class CSVLoader
    {
        private static string GenerateColumnName(int index)
        {
            return "Column_" + index.ToString();
        }
        private static char DetectDelimiter(string path, Encoding encoding)
        {
            using var reader = new StreamReader(path, encoding);

            // Read sample lines
            var sampleLines = new List<string>();
            for (int i = 0; i < 5 && !reader.EndOfStream; i++)
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                    sampleLines.Add(line);
            }

            if (sampleLines.Count == 0)
                throw new LoadCsvException("CSV file is empty");

            // Try common delimiters
            char[] candidates = OperatingSystem.IsWindows()
                ? new[] { ',', ';', '\t', '|' }
                : new[] { ',', '\t', '|' };
            foreach (var delim in candidates)
            {
                var counts = sampleLines
                    .Select(line => CountDelimiterOutsideQuotes(line, delim))
                    .ToArray();

                // Consistent count across all lines
                if (counts.Length > 0 &&
                    counts.Distinct().Count() == 1 &&
                    counts[0] > 0)
                {
                    return delim;
                }
            }

            // Fallback
            return ',';
        }

        private static int CountDelimiterOutsideQuotes(string line, char delimiter)
        {
            int count = 0;
            bool inQuotes = false;

            foreach (char c in line)
            {
                if (c == '"')
                    inQuotes = !inQuotes; // reverse the state of in quote
                else if (c == delimiter && !inQuotes)
                    count++;
            }

            return count;
        }

        private static string[] AlignRow(string[] row, int expectedNumColumns, int rowNumber, bool strict = true)
        {
            // no operation needed if perfect
            if (row.Length == expectedNumColumns)
            {
                return row.ToArray();
            }

            var aligned = new string[expectedNumColumns];

            if (row.Length > expectedNumColumns)
            {
                if (strict)
                {
                    throw new InvalidOperationException($"Row {rowNumber}: Too many columns  (expected {expectedNumColumns}, got {row.Length})");
                }
                // Truncate extra columns
                Array.Copy(row, aligned, expectedNumColumns);
            }
            else
            {
                if (strict)
                {
                    throw new InvalidOperationException(
                        $"Row {rowNumber}: Not enough columns " +
                        $"(expected {expectedNumColumns}, got {row.Length})");
                }

                // Copy available columns
                Array.Copy(row, aligned, row.Length);

                // Fill missing columns with empty string
                for (int i = row.Length; i < expectedNumColumns; i++)
                    aligned[i] = string.Empty;
            }
            return aligned;
        }


        public static DataFrame.DataFrame LoadFromCSV(string path, bool hasHeader, char? delim = null, Encoding? encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;


            // Auto-detect delimiter if not specified
            char effectiveDelimiter = delim
                ?? DetectDelimiter(path, encoding);

            // set configuration for csv reader
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = effectiveDelimiter.ToString(),
                HasHeaderRecord = true,

                // Your design: don't fail on bad data, collect it
                MissingFieldFound = null,
                BadDataFound = null,

                // Trim handling
                TrimOptions = TrimOptions.None,  // Keep raw strings!

                // Quote handling
                Mode = CsvMode.RFC4180,  // Standard CSV

                // Buffer size for performance
                BufferSize = 2048,
            };

            // read the csv file
            using var reader = new StreamReader(path, encoding);
            using var csv = new CsvReader(reader, config);

            // Step 1: Read and validate header
            ColumnRegistry columnRegistry;
            if (hasHeader)
            {
                csv.Read();
                csv.ReadHeader();

                if (csv.HeaderRecord == null || csv.HeaderRecord.Length == 0)
                    throw new LoadCsvException("Cannot read header from CSV file");

                columnRegistry = new ColumnRegistry(csv.HeaderRecord);
            }
            else {
                csv.Read();
                int columnCount = csv.Parser.Count;
                var generatedHeaders = Enumerable.Range(0, columnCount)
                    .Select(i => GenerateColumnName(i))
                    .ToArray();
                columnRegistry = new ColumnRegistry(generatedHeaders);

                // Reset to read first row as data
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                reader.DiscardBufferedData();
                csv.Read();
            }

            // Step 2: Initialize buffer to store
            var buffers = new List<List<string>>(columnRegistry.GetNumColumns());
            for (int i = 0; i < columnRegistry.GetNumColumns(); i++)
                buffers.Add(new List<string>());

            // Step 3: read rows
            int rowNumber = hasHeader ? 1 : 0;
            while (csv.Read())
            {
                rowNumber++;

                var record = csv.Parser.Record;

                if (record == null || record.Length == 0)
                    continue;  // Skip empty lines

                // Align row to expected columns
                var aligned = AlignRow(
                    record,
                    columnRegistry.GetNumColumns(),
                    rowNumber
                    );

                for (int i = 0; i < columnRegistry.GetNumColumns(); i++)
                {
                    buffers[i].Add(aligned[i]);
                }
            }

            // Step 4: Create Series<string> for each column
            var series = new List<Series>();

            for (int i = 0; i < columnRegistry.GetNumColumns(); i++)
            {
                var columnName = columnRegistry.GetColumnName(i);
                var columnData = buffers[i];

                // All Series are string type - your design!
                series.Add(new Series(
                    columnData,
                    dtype: typeof(string),
                    name: columnName));
            }

            return new DataFrame.DataFrame(
                columnRegistry.Columns.ToList(),
                series);
        }
    }
}
