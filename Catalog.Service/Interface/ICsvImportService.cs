namespace Catalog.Service.Interface
{
    public interface ICsvImportService
    {
        Task ImportCsvAsync(string filePath);
    }
}
