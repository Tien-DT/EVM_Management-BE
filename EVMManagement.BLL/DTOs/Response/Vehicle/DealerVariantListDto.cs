namespace EVMManagement.BLL.DTOs.Response.Vehicle
{
    public class DealerVariantListDto
    {
        public Guid Id { get; init; }
        public Guid ModelId { get; init; }
        public string ModelName { get; init; } = string.Empty;
        public int AvailableCount { get; init; }
    }
}
