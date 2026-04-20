using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.UsersManagement.DTOS
{
    public class UserManagementQueryDto
    {
        public string? SearchTerm { get; set; }

        public UserRole? Role { get; set; }

        public bool? IsActive { get; set; }

        public int? BranchId { get; set; }

        public int? HospitalId { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}