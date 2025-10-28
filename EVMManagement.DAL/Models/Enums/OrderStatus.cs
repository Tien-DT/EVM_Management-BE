namespace EVMManagement.DAL.Models.Enums
{
    public enum OrderStatus
    {
        CONFIRMED,
        QUOTATION_RECEIVED,
        QUOTATION_ACCEPTED,
        QUOTATION_REJECTED,
        CREATED_CONTRACT,
        SIGNED_CONTRACT,
        AWAITING_DEPOSIT,
        DEPOSIT_SUCCESS,
        IN_PROGRESS,
        IN_TRANSIT,
        READY_FOR_HANDOVER,
        COMPLETED,
        CANCELED
    }
}
