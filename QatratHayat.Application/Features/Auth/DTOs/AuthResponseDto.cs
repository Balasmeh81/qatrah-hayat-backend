using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Accounts.DTOs
{
    // This class represents the data returned to the user after successful registration or login.
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string FullNameEn { get; set; } = null!;
        public UserRole Role { get; set; } 
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public BloodType BloodType { get; set; }
        public bool IsProfileCompleted { get; set; }
        public string Token { get; set; } = null!;
    }
}