using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ParkingApi.Domain.Dtos.Options;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Helpers.Jwt;

public record JwtResult(string Token, string Jti, DateTime ExpiresAtUtc);

public static class TokenHelper
{
    public static JwtResult CreateJwt(this LoginUserDto user, JwtOptions options)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.JwtSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jti = NewToken();
        var expiresUtc = DateTime.UtcNow.AddMinutes(options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserName),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, (user.IdUserRole ?? 0).ToString()),
            new(ClaimTypes.Sid, user.Id.ToString()),
            new("fullName", user.Fullname),
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

    public static JwtResult CreateJwt(this User user, string roleName, JwtOptions options)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.JwtSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jti = NewToken();
        var expiresUtc = DateTime.UtcNow.AddMinutes(options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, roleName),
            new("role_id", user.UserRoleId.ToString()),
            new(ClaimTypes.Sid, user.Id.ToString()),
            new("fullName", user.FullName),
            new("email", user.Email),
            new("mustChangePassword", user.MustChangePassword.ToString().ToLower()),
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
