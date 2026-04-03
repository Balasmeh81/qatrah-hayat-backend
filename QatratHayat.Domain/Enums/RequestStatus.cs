namespace QatratHayat.Domain.Enums;

public enum RequestStatus
{
    PendingDoctorReview = 1,
    PendingBloodBank = 2,
    PartiallyAllocated = 3,
    Allocated = 4,
    Fulfilled = 5,
    Cancelled = 6,
    Rejected = 7,
    Shortage = 8
}