using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            // TODO use full paths via FileInfo/DirectoryInfo
            // TODO check if rule is present (or contained inside another rule)
            try {
                using (var db = new DatabaseContext()) {
                    await db.AddAsync(r);
                    await db.SaveChangesAsync();
                }
                Log.Information("Rule successfully added");
            } catch (Exception e) {
                Log.Error(e, "Failed to add sync rule");
                throw e;
            }
        }

        public static async Task RemoveSyncRule(int id)
        {
            try {
                using (var db = new DatabaseContext()) {
                    SyncRule rule = await db.SyncRules.FindAsync(id);
                    if (rule is { }) {
                        db.Remove(rule);
                        await db.SaveChangesAsync();
                        Log.Information("Rule {RuleId} successfully removed", id);
                    } else {
                        Log.Error("No rules match given ID");
                    }
                }
            } catch (Exception e) {
                Log.Error(e, "Failed to remove sync rule");
                throw e;
            }
        }
    }
}
