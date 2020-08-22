using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SharpSync.Common;

namespace SharpSync.Services
{
    internal static class Utilities
    {
        public static void DeleteDirectory(DirectoryInfo dir)
        {
            FileInfo[] fis = dir.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo fi in fis) {
                if (fi.IsReadOnly)
                    fi.IsReadOnly = false;
            }

            DirectoryInfo[] dis = dir.GetDirectories("*.*", SearchOption.AllDirectories);
            foreach (DirectoryInfo di in dis) {
                if ((di.Attributes & FileAttributes.ReadOnly) > 0)
                    di.Attributes &= ~FileAttributes.ReadOnly;
            }

            if ((dir.Attributes & FileAttributes.ReadOnly) > 0)
                dir.Attributes &= ~FileAttributes.ReadOnly;

            dir.Delete(true);
        }

        public static IReadOnlyDictionary<string, FileInfo> GetFiles(DirectoryInfo di, SyncRuleOptions cfg)
        {
            return di.GetFiles()
                .Where(fi => !IsExempted(di.Name, cfg.IncludeDirs, cfg.ExcludeDirs) && (!IsHidden(fi) || (IsHidden(fi) && cfg.IncludeHidden)))
                .ToDictionary(fi => fi.Name, fi => fi)
                ;
        }

        public static IReadOnlyDictionary<string, DirectoryInfo> GetDirs(DirectoryInfo di, SyncRuleOptions cfg)
        {
            return di.GetDirectories()
                .Where(di => !IsExempted(di.Name, cfg.IncludeDirs, cfg.ExcludeDirs) && (!IsHidden(di) || (IsHidden(di) && cfg.IncludeHidden)))
                .ToDictionary(di => di.Name, di => di)
                ;
        }

        public static bool IsExempted(string name, IEnumerable<Regex>? includeRegexes, IEnumerable<Regex>? excludeRegexes)
        {
            if (excludeRegexes is { })
                return excludeRegexes.Any(r => r.IsMatch(name));
            else if (includeRegexes is { })
                return !includeRegexes.Any(r => r.IsMatch(name));
            else
                return false;
        }

        public static bool AreSynced(FileInfo src, FileInfo? dst)
        {
            return (dst is { })
                && (src.Length == dst.Length)
                && (src.LastWriteTime == dst.LastWriteTime)
                && (src.Attributes == dst.Attributes);
        }

        public static async Task CopyFileAsync(string src, string dst)
        {
            using (var srcStream = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var dstStream = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await srcStream.CopyToAsync(dstStream);
        }

        public static bool IsHidden(FileSystemInfo fi)
            => (fi.Attributes & FileAttributes.Hidden) > 0;
    }
}
