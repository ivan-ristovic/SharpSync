using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;
using SharpSync.Database;

namespace SharpSync.Services
{
    internal static class DatabaseService
    {
        // TODO config

        public static IReadOnlyList<SyncRule> GetAllSyncRules()
        {
            // TODO use full paths via FileInfo/DirectoryInfo
            // TODO check if rule is present (or contained inside another rule)
            try {
                using (var db = new DatabaseContext()) {
                    return db.SyncRules?.ToList().AsReadOnly() ?? new List<SyncRule>().AsReadOnly();
                }
            } catch (Exception e) {
                Log.Error(e, "Failed to fatch sync rules from the database!");
                throw e;
            }
        }
    }
}
