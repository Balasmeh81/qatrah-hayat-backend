namespace QatratHayat.Domain.Enums;

public enum RequestStatus
{
    PendingDoctorReview = 1,
    PendingBloodBank = 2,
    Shortage = 3,
    PartiallyAllocated = 4,
    Processing = 5,
    Fulfilled = 6,
    Cancelled = 7,
    Rejected = 8
}