using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Serilog;
using SharpSync.Database;

namespace SharpSync.Services
{
    public static class SyncService
    {
        public static void Sync(IReadOnlyList<SyncRule> rules)
        {
            foreach (SyncRule rule in rules) {
                Log.Debug("Processing rule {RuleId}", rule.Id);
                try {
                    FileAttributes srcAttrs = File.GetAttributes(rule.Source.Path);
                    FileAttributes dstAttrs = File.GetAttributes(rule.Destination.Path);

                    if (!dstAttrs.HasFlag(FileAttributes.Directory)) {
                        Log.Error("Output path has to point to a directory:{NL}{Rule}", Environment.NewLine, rule.ToTableRow(printTopLine: true));
                        continue;
                    }

                    if (srcAttrs.HasFlag(FileAttributes.Directory)) {
                        Log.Debug("Copying directory {SourceDirectory} to {DestinationDirectory}", rule.Source.Path, rule.Destination.Path);

                        var srcInfo = new DirectoryInfo(rule.Source.Path);

                        // TODO check times
                        //DateTime dstWriteTime = Directory.GetLastWriteTime(rule.Source.Path);
                        //DateTime dstWriteTime = Directory.GetLastWriteTime(rule.Destination.Path);

                        foreach (DirectoryInfo dir in srcInfo.EnumerateDirectories("*", SearchOption.AllDirectories)) 
                            Directory.CreateDirectory(dir.FullName.Replace(rule.Source.Path, rule.Destination.Path));
                        foreach (FileInfo fi in srcInfo.EnumerateFiles("*.*", SearchOption.AllDirectories)) 
                            File.Copy(fi.FullName, fi.FullName.Replace(srcInfo.FullName, rule.Destination.Path), overwrite: true);
                    } else {
                        Log.Debug("Copying file {SourceFile} to {DestinationDirectory}", rule.Source.Path, rule.Destination.Path);
                    }
                } catch (FileNotFoundException e) {
                    Log.Error(e, "Source/Destination not found for rule:{NL}{Rule}", Environment.NewLine, rule.ToTableRow(printTopLine: true));
                } catch (IOException e) {
                    Log.Error(e, "Failed to copy rule:{NL}{Rule}", Environment.NewLine, rule.ToTableRow(printTopLine: true));
                }
            }

            Log.Information("Finished syncing");
        }
    }
}
