using QatratHayat.Domain.Enums;

public class PromoteCitizenToStaffRequestDto
{
    public UserRole StaffRole { get; set; }

    public int? BranchId { get; set; }

    public int? HospitalId { get; set; }
}