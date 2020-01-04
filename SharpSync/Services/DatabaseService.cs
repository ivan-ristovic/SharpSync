using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SharpSync.Database;

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

        public static async Task AddSyncRule(SyncRule r)
        {
            // TODO check if rule is present (or contained inside another rule)
            try {
                using (var db = new DatabaseContext()) {
                    //db.SyncRules.FirstOrDefaultAsync(r => r.Source)
                    await db.AddAsync(r);
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
