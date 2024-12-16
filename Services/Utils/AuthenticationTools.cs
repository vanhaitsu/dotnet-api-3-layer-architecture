using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.Common;

namespace Services.Utils;

public static class AuthenticationTools
{
    private static readonly Random Random = new();

    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, HashType.SHA256);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword, HashType.SHA256);
    }

    public static string GenerateUsername()
    {
        return GenerateUniqueToken().Replace("/", string.Empty).Replace("+", string.Empty).Replace("-", string.Empty);
    }

    public static string GenerateDigitCode(int length)
    {
        const string chars = "0123456789";
        var code = new StringBuilder(length);
        for (var i = 0; i < length; i++) code.Append(chars[Random.Next(chars.Length)]);

        return code.ToString();
    }

    public static string GenerateUniqueToken(DateTime? expiryDateTime = null)
    {
        var validExpiryDateTime = expiryDateTime ?? DateTime.UtcNow;
        var time = BitConverter.GetBytes(validExpiryDateTime.ToBinary());
        var key = Guid.NewGuid().ToByteArray();
        var token = Convert.ToBase64String(time.Concat(key).ToArray());

        return token;
    }

    public static bool IsUniqueTokenExpired(string token)
    {
        var data = Convert.FromBase64String(token);
        var dateTime = DateTime.FromBinary(BitConverter.ToInt64(data, 0));

        return dateTime < DateTime.UtcNow.AddHours(-24);
    }

    public static JwtSecurityToken CreateJwtToken(List<Claim> authClaims, IConfiguration configuration)
    {
        var secret = configuration["JWT:Secret"];
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        var issuer = configuration["JWT:ValidIssuer"];
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        var audience = configuration["JWT:ValidAudience"];
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var token = new JwtSecurityToken(
            issuer,
            audience,
            authClaims,
            expires: DateTime.UtcNow.AddMinutes(Constant.AccessTokenValidityInMinutes),
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    public static ClaimsPrincipal? GetPrincipalFromExpiredToken(string? accessToken, IConfiguration configuration)
    {
        try
        {
            var secret = configuration["JWT:Secret"];
            ArgumentException.ThrowIfNullOrWhiteSpace(secret);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase)) throw new SecurityTokenException("Invalid token");

            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }
}