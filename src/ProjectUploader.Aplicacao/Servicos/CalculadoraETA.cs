using System;

namespace ProjectUploader.Aplicacao.Servicos;

public static class CalculadoraETA
{
    public static TimeSpan CalcularETA(long bytesTotais, long bytesTransferidos, double bytesPorSegundo)
    {
        if (bytesTotais <= 0 || bytesTransferidos >= bytesTotais || bytesPorSegundo <= 0)
            return TimeSpan.Zero;

        long bytesRestantes = bytesTotais - bytesTransferidos;
        double segundosRestantes = bytesRestantes / bytesPorSegundo;

        return TimeSpan.FromSeconds(segundosRestantes);
    }
}
