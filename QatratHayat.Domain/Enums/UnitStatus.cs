namespace QatratHayat.Domain.Enums;

public enum UnitStatus
{
    Available = 1,

    // Reserved temporarily for a blood request.
    // Employee / BranchManager must confirm it before becoming Allocated.
    PartiallyAllocated = 2,

    // Officially allocated to a patient blood request.
    Allocated = 3,

    // Delivered / used for the patient.
    Used = 4,

    Expired = 5,

    Disposed = 6
}