namespace EVMManagement.DAL.Models.Enums
{
    public enum OrderStatus
    {
        CONFIRMED,
        QUOTATION_RECEIVED,
        AWAITING_DEPOSIT,
        SIGNED_CONTRACT,
        IN_PROGRESS,
        IN_TRANSIT,
        READY_FOR_HANDOVER,
        COMPLETED,
        CANCELED
    }
}
