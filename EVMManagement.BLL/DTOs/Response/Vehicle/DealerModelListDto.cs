namespace EVMManagement.BLL.DTOs.Response.Vehicle
{
    public class DealerModelListDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int VariantCount { get; init; }
    }
}
