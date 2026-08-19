using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ParkingApi.Domain.Dtos.Options;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Helpers.Jwt;

public record JwtResult(string Token, string Jti, DateTime ExpiresAtUtc);

public static class TokenHelper
{
    public static JwtResult CreateJwt(User user, string roleName, JwtOptions options)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.JwtSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jti = NewToken();
        var expiresUtc = DateTime.UtcNow.AddMinutes(options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Sid, user.UserId.ToString()),
            new(ClaimTypes.Role, roleName),
            new("fullName", user.FullName),
            new(JwtRegisteredClaimNames.Jti, jti)
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresUtc,
            signingCredentials: creds
        );

        return new JwtResult(new JwtSecurityTokenHandler().WriteToken(token), jti, expiresUtc);
    }

    public static string NewToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
