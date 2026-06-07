using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Domain.Config;
using Domain.Constants;
using Domain.DTOs.Security;

namespace Application.Helpers;

public class JwtHelper
{
    public static string GenerateJwtToken(UsuarioDTO usuario, JwtConfig jwtConfig)
    {
        var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtClaimNames.IdUsuario, usuario.Id.ToString()),
                        new Claim(JwtClaimNames.Nombre, usuario.Nombre + " " + usuario.Apellido),
                        new Claim(JwtClaimNames.Email, usuario.Email),
                        new Claim(JwtClaimNames.Usuario, usuario.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtConfig.Issuer,
            audience: jwtConfig.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(jwtConfig.ExpireTime),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static IDictionary<string, string> GetTokenPayload(string token, string secret)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            // Validar el token usando los parámetros de validación
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            // Extraer los claims como un diccionario
            var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);

            return claims; // Devolver todos los claims del payload
        }
        catch
        {
            return null; // Retorna null si hay algún problema
        }
    }
}
