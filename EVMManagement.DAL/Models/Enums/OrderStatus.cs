namespace EVMManagement.DAL.Models.Enums
{
    public enum OrderStatus
    {
        AWAITING_CONFIRM,
        CONFIRMED,
        QUOTATION_RECEIVED,
        QUOTATION_ACCEPTED,
        QUOTATION_REJECTED,
        CREATED_CONTRACT,
        SIGNED_CONTRACT,
        DEALER_SIGNED_CONTRACT,
        AWAITING_DEPOSIT,
        DEPOSIT_SUCCESS,
        IN_PROGRESS,
        IN_TRANSIT,
        READY_FOR_HANDOVER,
        COMPLETED,
        CANCELED,
        REPORTED,
        REPORTED_PROCESSING,
        REPORTED_REJECTED,
        REPORTED_COMPLETED,
        PAY_SUCCESS
    }
}
