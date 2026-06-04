using ProjectUploader.Aplicacao.Interfaces;

namespace ProjectUploader.Infraestrutura.Servicos;

public class ServicoCriptografiaHash : IServicoCriptografia
{
    public string GerarHash(string senhaTextoPlano)
    {
        // Usa o pacote BCrypt.Net-Next instalado no Passo 4
        return BCrypt.Net.BCrypt.HashPassword(senhaTextoPlano);
    }

    public bool ValidarSenha(string senhaTextoPlano, string hashArmazenado)
    {
        return BCrypt.Net.BCrypt.Verify(senhaTextoPlano, hashArmazenado);
    }
}
