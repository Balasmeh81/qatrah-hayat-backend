using System;
namespace QatratHayat.Application.Accounts.DTOs
{
    //This Class represents data coming from user SignUp.
    public class RegisterRequestDto
    {
        public string NationalId { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string FullNameEn { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }

        public string? BloodType { get; set; }
        public string? Gender { get; set; }
        public string? MaritalStatus { get; set; }

        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? JobTitle { get; set; }
        public string? Address { get; set; }

        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
}
