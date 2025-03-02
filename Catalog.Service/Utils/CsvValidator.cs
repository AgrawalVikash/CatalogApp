using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Service.Utils
{
    public class CsvValidator
    {
        public static bool ValidateFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Log.Error("File path is empty or null.");
                return false;
            }

            if (!File.Exists(filePath))
            {
                Log.Error($"File does not exist at the given path: {filePath}");
                return false;
            }

            if (Path.GetExtension(filePath).ToLower() != ".csv")
            {
                Log.Error("Invalid file extension. Only .csv files are allowed.");
                return false;
            }

            return true;
        }
    }
}
