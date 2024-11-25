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

    public static string GenerateDigitCode(int length)
    {
        const string chars = "0123456789";
        var code = new StringBuilder(length);
        for (var i = 0; i < length; i++) code.Append(chars[Random.Next(chars.Length)]);

        return code.ToString();
    }

    public static string GenerateUniqueToken(DateTime expiryDateTime)
    {
        var time = BitConverter.GetBytes(expiryDateTime.ToBinary());
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
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!));
        var token = new JwtSecurityToken(
            audience: configuration["JWT:ValidAudience"],
            issuer: configuration["URL:Server"],
            expires: DateTime.UtcNow.AddMinutes(Constant.AccessTokenValidityInMinutes),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    public static ClaimsPrincipal? GetPrincipalFromExpiredToken(string? accessToken, IConfiguration configuration)
    {
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!)),
                ValidateLifetime = false
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