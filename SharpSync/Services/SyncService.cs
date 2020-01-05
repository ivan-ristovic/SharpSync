using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlinkSyncLib;
using Serilog;
using SharpSync.Common;
using SharpSync.Database;

namespace SharpSync.Services
{
    internal static class SyncService
    {
        public static void SyncAll(IReadOnlyList<SyncRule> rules, SyncOptions opts)
        {
            if (!rules.Any())
                Log.Warning("Nothing to sync");

            foreach (SyncRule rule in rules) {
                Log.Debug("Processing rule {RuleId}", rule.Id);
                try {
                    FileAttributes srcAttrs = File.GetAttributes(rule.Source.Path);
                    FileAttributes dstAttrs = File.GetAttributes(rule.Destination.Path);

                    if (!dstAttrs.HasFlag(FileAttributes.Directory) || !srcAttrs.HasFlag(FileAttributes.Directory)) {
                        Log.Error("Paths have to point to directories:{NL}{Rule}", Environment.NewLine, rule.ToTableRow(printTopLine: true));
                        continue;
                    }

                    var sync = new Sync(rule.Source.Path, rule.Destination.Path);
                    var conf = new InputParams {
                        DeleteFromDest = opts.DeleteExtra,
                        ExcludeHidden = opts.IncludeHidden,
                    };
                    if (opts.ExcludeDirs?.Any() ?? false)
                        conf.ExcludeDirs = opts.ExcludeDirs?.ToArray();
                    if (opts.ExcludeFiles?.Any() ?? false)
                        conf.ExcludeFiles = opts.ExcludeFiles?.ToArray();
                    if (opts.IncludeDirs?.Any() ?? false)
                        conf.IncludeDirs = opts.IncludeDirs?.ToArray();
                    if (opts.IncludeFiles?.Any() ?? false)
                        conf.IncludeFiles = opts.IncludeFiles?.ToArray();
                    sync.Configuration = conf;
                    sync.Log = m => Log.Debug("SyncLib: {SyncLibLogMessage}", m);
                    sync.Start();

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
