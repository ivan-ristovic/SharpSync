using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SharpSync.Database;
using SharpSync.Extensions;
using SharpSync.Services.Common;

namespace SharpSync.Services
{
    internal static class DatabaseService
    {
        // TODO config


        public static async Task<IReadOnlyList<SyncRule>> GetAllSyncRules()
        {
            try {
                using (var db = new DatabaseContext()) {
                    List<SyncRule> rules = await db.SyncRules
                        .Include(r => r.Source)
                        .Include(r => r.Destination)
                        .ToListAsync();
                    return rules.AsReadOnly();
                }
            } catch (Exception e) {
                Log.Error(e, "Failed to fatch sync rules from the database");
                throw e;
            }
        }

        public static async Task AddSyncRule(string src, string dst, bool zip)
        {
            if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(dst))
                return;

            string srcPath = Path.GetFullPath(src);
            string dstPath = Path.GetFullPath(dst);

            try {
                using (var db = new DatabaseContext()) {
                    SourcePath? srcP = db.SourcePaths
                        .Include(s => s.SyncRules)
                        .ThenInclude(r => r.Destination)
                        .AsEnumerable()
                        .FirstOrDefault(r => r.Path.IsSubPathOf(srcPath));
                    if (srcP is { }) {
                        Log.Warning("Rule containing {Source} is already present:", src.ToString());
                        Console.WriteLine(string.Join(Environment.NewLine, srcP.SyncRules.Select(r => r.ToTableRow(printTopLine: true))));
                        return;
                    }
                    await db.AddAsync(new SyncRule {
                        Source = new SourcePath { Path = srcPath },
                        Destination = new DestinationPath { Path = dstPath },
                        ShouldZip = zip,
                    });
                    await db.SaveChangesAsync();
                }
                Log.Information("Rule successfully added");
            } catch (Exception e) {
                Log.Error(e, "Failed to add sync rule");
                throw e;
            }
        }

        public static async Task RemoveSyncRules(IEnumerable<int>? ids)
        {
            try {
                using (var db = new DatabaseContext()) {
                    if (ids is null || !ids.Any()) {
                        db.RemoveRange(db.SyncRules);
                        await db.SaveChangesAsync();
                        Log.Information("All rules successfully removed");
                        return;
                    }

                    foreach (int id in ids) {
                        SyncRule rule = await db.SyncRules.FindAsync(id);
                        if (rule is { }) {
                            db.Remove(rule);
                            Log.Information("Rule {RuleId} successfully removed", id);
                        } else {
                            Log.Error("No rules with id {RuleId}", id);
                        }
                    }

                    await db.SaveChangesAsync();
                }
            } catch (Exception e) {
                Log.Error(e, "Failed to remove sync rule(s)");
                throw e;
            }
        }
    }
}
