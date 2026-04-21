

using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class PasswordResetOtp
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(256)]
        public string OtpHash { get; set; } = null!;

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public int FailedAttempts { get; set; }

        [Required]
        public bool IsVerified { get; set; }

        [Required]
        public bool IsUsed { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public DateTime? UsedAt { get; set; }

        [MaxLength(128)]
        public string? ResetSessionToken { get; set; }
    }
}
