using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BlinkSyncLib;
using Serilog;
using SharpSync.Database;

namespace SharpSync.Services
{
    public static class SyncService
    {
        public static void SyncAll(IReadOnlyList<SyncRule> rules)
        {
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
                    // TODO sync.Configuration = new InputParams { };
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
