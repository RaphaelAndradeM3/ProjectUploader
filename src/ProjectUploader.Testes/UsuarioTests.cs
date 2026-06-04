using System;
using NUnit.Framework;
using ProjectUploader.Dominio.Entidades;
using ProjectUploader.Dominio.Enums;
using ProjectUploader.Dominio.Excecoes;

namespace ProjectUploader.Testes;

[TestFixture]
public class UsuarioTests
{
    [Test]
    public void DeveCriarUsuarioValido()
    {
        // Arrange & Act
        var usuario = new Usuario(Guid.NewGuid(), "Admin Local", "admin", "admin@local.com", "hash", TipoPerfil.Administrador);

        // Assert
        Assert.That(usuario.NomeUsuario, Is.EqualTo("admin"));
        Assert.That(usuario.Perfil, Is.EqualTo(TipoPerfil.Administrador));
    }

    [Test]
    public void NaoDeveCriarUsuarioSemNome()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Usuario(Guid.NewGuid(), "", "admin", "admin@local.com", "hash", TipoPerfil.Administrador)
        );
    }

    [Test]
    public void NaoDeveCriarUsuarioSemEmail()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Usuario(Guid.NewGuid(), "Admin", "admin", "", "hash", TipoPerfil.Administrador)
        );
    }
}
