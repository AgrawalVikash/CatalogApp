namespace Catalog.Service.Models
{
    public class ProductDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string CategoryCode { get; set; }
    }
}
