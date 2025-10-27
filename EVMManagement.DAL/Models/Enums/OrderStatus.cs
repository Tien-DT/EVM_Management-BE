namespace EVMManagement.DAL.Models.Enums
{
    public enum OrderStatus
    {
        CONFIRMED,
        QUOTATION_RECEIVED,  // Đã nhận báo giá
        AWAITING_DEPOSIT,
        IN_PROGRESS,
        READY_FOR_HANDOVER,
        COMPLETED,
        CANCELED
    }
}
