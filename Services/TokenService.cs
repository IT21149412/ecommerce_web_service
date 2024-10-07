/*
 * ------------------------------------------------------------------------------
 * File:       TokenService.cs
 * Description: 
 *             Provides JWT (JSON Web Token) generation services for user authentication. 
 *             It generates tokens that include user information like ID, role, and email.
 * ------------------------------------------------------------------------------
 */

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class TokenService
{
    private readonly string _secret;

    public TokenService(string secret)
    {
        _secret = secret;
    }

    public string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        Console.WriteLine($"Generated JWT: {jwt}"); // Print the JWT to console for debugging
        return jwt;
    }

}
