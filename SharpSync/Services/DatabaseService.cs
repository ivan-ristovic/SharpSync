using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SharpSync.Database;
using SharpSync.Extensions;

namespace SharpSync.Services
{
    internal static class DatabaseService
    {
        // TODO config

        public static async Task<IReadOnlyList<SyncRule>> GetAllSyncRules()
        {
            try {
                using (var db = new DatabaseContext()) {
                    List<SyncRule> rules = await db.SyncRules.ToListAsync();
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
                    SyncRule? rule = db.SyncRules.AsEnumerable().FirstOrDefault(r => r.SourcePath.IsSubPathOf(srcPath));
                    if (rule is { }) {
                        Log.Warning("Rule containing {Source} is already present:", rule.SourcePath.ToString());
                        Console.WriteLine(rule.ToTableRow(printTopLine: true));
                        return;
                    }
                    await db.AddAsync(new SyncRule {
                        Source = srcPath,
                        Destination = dstPath,
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
                            await db.SaveChangesAsync();
                            Log.Information("Rule {RuleId} successfully removed", id);
                        } else {
                            Log.Error("No rules with id {RuleId}", id);
                        }
                    }
                }
            } catch (Exception e) {
                Log.Error(e, "Failed to remove sync rule(s)");
                throw e;
            }
        }
    }
}
