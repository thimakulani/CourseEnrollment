using CourseEnrollment.API.Data;
using CourseEnrollment.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CourseEnrollment.API.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public JwtTokenService(AppDbContext db, IConfiguration configuration)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        
        public async Task<TokenPair> CreateTokenPairAsync(Student user, string ipAddress = null, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(user);

            var (token, expiresUtc) = CreateAccessToken(user);
            var refreshRaw = CreateRefreshTokenRaw();
            var refreshHash = Hash(refreshRaw);

            var record = new RefreshTokenRecord
            {
                UserId = user.Id,
                TokenHash = refreshHash,
                ExpiresUtc = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress,
                CreatedUtc = DateTime.UtcNow,
                IsActive = true
            };

            _db.Set<RefreshTokenRecord>().Add(record);
            await _db.SaveChangesAsync(ct);

            return new TokenPair
            {
                AccessToken = token,
                RefreshToken = refreshRaw,
                ExpiresAtUtc = expiresUtc
            };
        }

        
        public async Task<TokenPair> RefreshAsync(string refreshToken, string ipAddress = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentNullException(nameof(refreshToken));

            var tokenHash = Hash(refreshToken);

            var record = await _db.RefreshTokenRecords
                .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && r.IsActive, ct);

            if (record == null || record.ExpiresUtc < DateTime.UtcNow)
                throw new SecurityTokenException("Invalid or expired refresh token.");

            var user = await _db.Students
                .FirstOrDefaultAsync(u => u.Id == record.UserId, ct)
                ?? throw new SecurityTokenException("Student not found.");

            
            record.IsActive = false;
            record.RevokedUtc = DateTime.UtcNow;
            record.RevokedByIp = ipAddress;

            
            var newRaw = CreateRefreshTokenRaw();
            var newHash = Hash(newRaw);

            var next = new RefreshTokenRecord
            {
                UserId = user.Id,
                TokenHash = newHash,
                ExpiresUtc = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress,
                CreatedUtc = DateTime.UtcNow,
                IsActive = true
            };

            record.ReplacedByTokenHash = newHash;

            _db.Update(record);
            await _db.AddAsync(next, ct);
            await _db.SaveChangesAsync(ct);

            var (token, expiresUtc) = CreateAccessToken(user);

            return new TokenPair
            {
                AccessToken = token,
                RefreshToken = newRaw,
                ExpiresAtUtc = expiresUtc
            };
        }

        
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return null;

            var validation = GetTokenValidationParameters(validateLifetime: false);

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principal = handler.ValidateToken(accessToken, validation, out var securityToken);

                if (securityToken is not JwtSecurityToken jwt ||
                    !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }

        
        private (string Token, DateTime ExpiresUtc) CreateAccessToken(Student user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresUtc = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresUtc,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return (jwt, expiresUtc);
        }

        
        private TokenValidationParameters GetTokenValidationParameters(bool validateLifetime)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = validateLifetime,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!)
                ),
                ClockSkew = TimeSpan.Zero
            };
        }

        
        private string CreateRefreshTokenRaw()
        {
            Span<byte> bytes = stackalloc byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Base64UrlEncode(bytes);
        }

        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }

        private static string Base64UrlEncode(ReadOnlySpan<byte> bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }

    
    public interface IJwtTokenService
    {
        Task<TokenPair> CreateTokenPairAsync(Student user, string ipAddress = null, CancellationToken ct = default);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
        Task<TokenPair> RefreshAsync(string refreshToken, string ipAddress = null, CancellationToken ct = default);
    }
}
