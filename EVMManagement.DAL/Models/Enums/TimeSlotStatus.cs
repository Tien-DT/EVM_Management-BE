namespace EVMManagement.DAL.Models.Enums
{
    public enum TimeSlotStatus
    {
        PENDING,    // Đang chờ xác nhận
        BOOKED,     // Đã xác nhận - xe bận
        COMPLETED,  // Đã hoàn thành
        CANCELED    // Đã hủy
    }
}
