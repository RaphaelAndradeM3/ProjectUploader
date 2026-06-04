using System;
using NUnit.Framework;
using ProjectUploader.Aplicacao.Servicos;

namespace ProjectUploader.Testes;

[TestFixture]
public class CalculadoraETATests
{
    [Test]
    public void DeveCalcularETACorretamenteQuandoVelocidadeConstante()
    {
        // Arrange
        long bytesTotais = 1000;
        long bytesTransferidos = 500;
        double velocidadeBs = 100; // 100 bytes/sec

        // Act
        var eta = CalculadoraETA.CalcularETA(bytesTotais, bytesTransferidos, velocidadeBs);

        // Assert
        Assert.That(eta.TotalSeconds, Is.EqualTo(5));
    }

    [Test]
    public void DeveRetornarZeroQuandoUploadCompleto()
    {
        // Arrange
        long bytesTotais = 1000;
        long bytesTransferidos = 1000;
        double velocidadeBs = 100;

        // Act
        var eta = CalculadoraETA.CalcularETA(bytesTotais, bytesTransferidos, velocidadeBs);

        // Assert
        Assert.That(eta, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void DeveRetornarZeroQuandoVelocidadeZero()
    {
        // Arrange
        long bytesTotais = 1000;
        long bytesTransferidos = 500;
        double velocidadeBs = 0;

        // Act
        var eta = CalculadoraETA.CalcularETA(bytesTotais, bytesTransferidos, velocidadeBs);

        // Assert
        Assert.That(eta, Is.EqualTo(TimeSpan.Zero));
    }
}
