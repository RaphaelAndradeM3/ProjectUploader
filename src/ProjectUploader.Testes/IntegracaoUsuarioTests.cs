using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProjectUploader.Dominio.Entidades;
using ProjectUploader.Dominio.Enums;
using ProjectUploader.Infraestrutura.Persistencia;

namespace ProjectUploader.Testes;

[TestFixture]
public class IntegracaoUsuarioTests
{
    private ContextoBancoDados _contexto;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ContextoBancoDados>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _contexto = new ContextoBancoDados(options);
    }

    [TearDown]
    public void TearDown()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();
    }

    [Test]
    public async Task DeveSalvarERecuperarUsuarioComSucesso()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuario = new Usuario(id, "Admin Teste", "admin.teste", "admin@teste.com", "hash", TipoPerfil.Administrador);
        
        // Act
        _contexto.Usuarios.Add(usuario);
        await _contexto.SaveChangesAsync();

        var usuarioSalvo = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

        // Assert
        Assert.That(usuarioSalvo, Is.Not.Null);
        Assert.That(usuarioSalvo!.NomeCompleto, Is.EqualTo("Admin Teste"));
        Assert.That(usuarioSalvo.Email, Is.EqualTo("admin@teste.com"));
        Assert.That(usuarioSalvo.Perfil, Is.EqualTo(TipoPerfil.Administrador));
    }
}
