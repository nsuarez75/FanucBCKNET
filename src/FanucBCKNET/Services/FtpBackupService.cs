using FanucBCKNET.Models;
using FluentFTP;

namespace FanucBCKNET.Services;

public record FtpBackupStatus(double Percentage, string Message, double? SpeedBytesPerSecond);

public class FtpBackupService
{
    public async Task BackupRobotAsync(
        string destinationFolder,
        Robot robot,
        IProgress<FtpBackupStatus>? progress = null,
        CancellationToken token = default)
    {
        Directory.CreateDirectory(destinationFolder);
        progress?.Report(new FtpBackupStatus(0, "Conectando...", null));

        var port = robot.Puerto > 0 ? robot.Puerto : 21;
        using var client = new AsyncFtpClient(robot.Ip, port: port);
        await client.Connect(token);

        var archivos = (await client.GetListing("/", token))
            .Where(i => i.Type == FtpObjectType.File)
            .ToList();

        if (archivos.Count == 0)
        {
            progress?.Report(new FtpBackupStatus(100, "Sin archivos que descargar", null));
            await client.Disconnect(token);
            return;
        }

        var totalBytes = archivos.Sum(a => Math.Max(a.Size, 0));
        long transferidoAntes = 0;
        var totalArchivos = archivos.Count;
        var indice = 0;

        foreach (var archivo in archivos)
        {
            token.ThrowIfCancellationRequested();
            indice++;
            var localPath = Path.Combine(destinationFolder, archivo.Name);
            var bytesAntesDeEsteArchivo = transferidoAntes;

            var fileProgress = new Progress<FtpProgress>(p =>
            {
                var transferido = bytesAntesDeEsteArchivo + Math.Max(p.TransferredBytes, 0);
                var pct = totalBytes > 0
                    ? Math.Clamp((double)transferido / totalBytes * 100.0, 0, 100)
                    : (double)indice / totalArchivos * 100.0;
                var velocidad = double.IsNaN(p.TransferSpeed) ? (double?)null : p.TransferSpeed;
                progress?.Report(new FtpBackupStatus(pct, $"{archivo.Name} ({indice}/{totalArchivos})", velocidad));
            });

            await client.DownloadFile(localPath, archivo.FullName, progress: fileProgress, token: token);
            transferidoAntes += Math.Max(archivo.Size, 0);
        }

        await client.Disconnect(token);
        progress?.Report(new FtpBackupStatus(100, "Completado", null));
    }
}
