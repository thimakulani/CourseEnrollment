using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseEnrollment.API.Models
{
    public class RefreshTokenRecord
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey(nameof(Student))]
        public Guid UserId { get; set; }
        public string TokenHash { get; set; }
        public DateTime ExpiresUtc { get; set; }
        [StringLength(100)]
        public string CreatedByIp { get; set; }
        public bool IsActive { get; set; }
        public DateTime RevokedUtc { get; set; }
        [StringLength(100)]
        public string RevokedByIp { get; set; }
        public string ReplacedByTokenHash { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public class JwtSettings
    {
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string Key { get; set; } = default!;
        public int AccessTokenMinutes { get; set; } = 15;
        public int RefreshTokenDays { get; set; } = 14;
    }
    public sealed class TokenPair
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
