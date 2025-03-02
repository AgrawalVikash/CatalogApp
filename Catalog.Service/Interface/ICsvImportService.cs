using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Service.Interface
{
    public interface ICsvImportService
    {
        Task ImportCsvAsync(string filePath);
    }
}
