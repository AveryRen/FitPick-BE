using FitPick_EXE201.Data;
using FitPick_EXE201.Models;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using FitPick_EXE201.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FitPick_EXE201.Services
{
    public class AuthService
    {
        private readonly IAuthRepo _authRepo;
        private readonly FitPickContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(FitPickContext context, IAuthRepo authRepo, IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _authRepo = authRepo;
            _jwtSettings = jwtOptions?.Value
                ?? throw new InvalidOperationException("JWT settings not configured");
        }

        public async Task<bool> RegisterAsync(AccountRegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword || !dto.Email.Contains('@'))
                return false;

            var existingEmail = await _authRepo.GetAccountByEmailAsync(dto.Email);
            if (existingEmail != null) return false;

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var newAccount = new User
            {
                Email = dto.Email,
                Passwordhash = hashedPassword,
                Fullname = dto.Email.Split('@')[0],
                RoleId = 2,
                Createdat = DateTime.Now,
                Status = true
            };

            await _authRepo.AddAsync(newAccount);
            return true;
        }
        public string GenerateAccessToken(User acc)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, acc.Userid.ToString()),
                new Claim("id", acc.Userid.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, acc.Email),
                new Claim(ClaimTypes.Role, acc.Role?.Name ?? "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken(User acc)
        {
            var claims = new[]
            {
                new Claim("id", acc.Userid.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<AuthResultDto?> LoginAsync(AccountLoginDto dto)
        {
            var account = await _authRepo.GetAccountByEmailAsync(dto.Email);
            if (account == null) return null;

            //Kiểm tra status
            if (account.Status == false) return null;

            var isValidPassword = BCrypt.Net.BCrypt.Verify(dto.Password, account.Passwordhash);
            if (!isValidPassword) return null;

            return new AuthResultDto
            {
                AccessToken = GenerateAccessToken(account),
                RefreshToken = GenerateRefreshToken(account),
                ExpiresIn = _jwtSettings.ExpirationMinutes,
                User = account
            };
        }


        public AuthResultDto? RefreshAccessToken(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                    ValidateLifetime = true
                }, out SecurityToken validatedToken);

                var userId = principal.FindFirst("id")?.Value;
                if (userId == null) return null;

                var account = _context.Users.Find(int.Parse(userId));
                if (account == null) return null;

                return new AuthResultDto
                {
                    AccessToken = GenerateAccessToken(account),
                    RefreshToken = GenerateRefreshToken(account),
                    ExpiresIn = _jwtSettings.ExpirationMinutes
                };
            }
            catch
            {
                return null;
            }
        }

        public int GetTokenExpirationMinutes() => _jwtSettings.ExpirationMinutes;
    }
}
