using FanucBCKNET.Models;
using FluentFTP;

namespace FanucBCKNET.Services;

public class FtpBackupService
{
    public async Task BackupRobotAsync(string destinationFolder, Robot robot, CancellationToken token = default)
    {
        Directory.CreateDirectory(destinationFolder);

        using var client = new AsyncFtpClient(robot.Ip);
        await client.Connect(token);

        var listing = await client.GetListing("/", token);
        foreach (var item in listing.Where(i => i.Type == FtpObjectType.File))
        {
            var localPath = Path.Combine(destinationFolder, item.Name);
            await client.DownloadFile(localPath, item.FullName, token: token);
        }

        await client.Disconnect(token);
    }
}
