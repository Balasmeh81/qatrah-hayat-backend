using QatratHayat.Domain.Enums;

public class CreateStaffFromRegistryRequestDto
{
    public string NationalId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string ConfirmPassword { get; set; } = null!;

    public MaritalStatus MaritalStatus { get; set; }

    public UserRole StaffRole { get; set; }

    public int? BranchId { get; set; }

    public int? HospitalId { get; set; }
}