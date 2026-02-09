using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.LoaderAndExporter
{
    public class LoadCsvException: Exception
    {
        private string message;
        public LoadCsvException(string message)
        {
            this.message = message;
        }
    }
}
