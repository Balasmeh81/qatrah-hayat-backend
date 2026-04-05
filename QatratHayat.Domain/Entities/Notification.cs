using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
       
        [Required]
        public NotificationType NotificationType { get; set; }
        [Required]
        public NotificationChannel NotificationChannel { get; set; }
        [Required]
        [MaxLength(300)]
        public string TitleAr { get; set; } = null!;
        [Required]
        [MaxLength(300)]
        public string TitleEn { get; set; } = null!;
        [Required]
        [MaxLength(1000)]
        public string ContentAr { get; set; } = null!;
        [Required]
        [MaxLength(1000)]
        public string ContentEn { get; set; } = null!;
        [Required]
        public NotificationStatus NotificationStatus { get; set; }  
        [Required]
        public DateTime CreatedAt { get; set; }

        public LinkedEntityType? LinkedEntityType { get; set; }
        public int? LinkedEntityId { get; set; }
        public DateTime? SentAt { get; set; }        
        public DateTime? ReadAt { get; set; }

        // Navigation Property
        [Required]
        public int RecipientUserId { get; set; }

    }
}
