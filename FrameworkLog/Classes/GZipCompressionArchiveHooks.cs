using System;
using System.IO;
using System.IO.Compression;

namespace FrameworkLog.Classes;

public static class LogFileArchiver
{
    public static void CompressLogFile(string filePath, string archivePath)
    {
        if (!File.Exists(filePath)) return;

        Directory.CreateDirectory(archivePath);
        var dest = Path.Combine(archivePath, Path.GetFileName(filePath) + ".gz");

        using var original = File.OpenRead(filePath);
        using var compressed = File.Create(dest);
        using var gzip = new GZipStream(compressed, CompressionLevel.Optimal);
        original.CopyTo(gzip);
    }

    public static void ArchiveOldFiles(string sourceDir, string archiveDir, int retentionDays)
    {
        if (!Directory.Exists(sourceDir)) return;
        Directory.CreateDirectory(archiveDir);

        foreach (var file in Directory.GetFiles(sourceDir, "*.log"))
        {
            var dest = Path.Combine(archiveDir, Path.GetFileName(file) + ".gz");
            if (!File.Exists(dest))
            {
                CompressLogFile(file, archiveDir);
            }
        }

        // پاک کردن فایل‌های قدیمی
        foreach (var oldFile in Directory.GetFiles(archiveDir)
                     .Where(f => File.GetCreationTime(f) < DateTime.Now.AddDays(-retentionDays)))
        {
            File.Delete(oldFile);
        }
    }
}
