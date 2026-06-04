using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProjectUploader.Dominio.Entidades;

namespace ProjectUploader.API.Servicos;

public interface IServicoToken
{
    string GerarToken(Usuario usuario);
}

public class ServicoToken : IServicoToken
{
    private readonly IConfiguration _configuracao;

    public ServicoToken(IConfiguration configuracao)
    {
        _configuracao = configuracao;
    }

    public string GerarToken(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        // Fallback secret para desenvolvimento. Em prod usar variavel de ambiente.
        var chaveSecreta = _configuracao["Jwt:Key"] ?? "ProjectUploader_Super_Secret_Key_For_JWT_Min_32_Bytes_Long_String"; 
        var key = Encoding.ASCII.GetBytes(chaveSecreta);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NomeUsuario),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
