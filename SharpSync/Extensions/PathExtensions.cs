using System.IO;
using System.Linq;

namespace SharpSync.Extensions
{
    internal static class PathExtensions
    {
        public static bool IsParentPathOf(this string src, string dst)
        {
            string[] srcParts = src.Split(Path.DirectorySeparatorChar);
            string[] dstParts = dst.Split(Path.DirectorySeparatorChar);
            return !srcParts.Zip(dstParts).SkipWhile(t => t.First == t.Second).Any();
        }
    }
}
