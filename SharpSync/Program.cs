using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Serilog;
using SharpSync.Common;
using SharpSync.Database;
using SharpSync.Services;

namespace SharpSync
{
    internal static class Program
    {
        internal static Task<int> Main(string[] args)
        {
            return Parser.Default.ParseArguments<ListOptions, AddOptions, RemoveOptions>(args)
                .MapResult(
                    (ListOptions o) => ListSyncRules(o),
                    (AddOptions o) => AddSyncRule(o),
                    (RemoveOptions o) => RemoveSyncRule(o),
                    errs => Task.FromResult(1)
                );
        }


        private static Task<int> ListSyncRules(ListOptions o)
        {
            Setup.Logger(verbose: false);
            Log.Information("Registered sync rules:");

            IReadOnlyList<SyncRule> rules = DatabaseService.GetAllSyncRules();
            if (rules.Count > 0)
                Console.WriteLine(string.Join(Environment.NewLine, rules));
            else
                Log.Information("Sync rule list is empty.");

            return Task.FromResult(0);
        }

        private static async Task<int> AddSyncRule(AddOptions o)
        {
            Setup.Logger(o.Verbose);
            Log.Information("Adding sync rule {Source} -> {Destination}", o.Source, o.Destination);
            if (o.ShouldZip)
                Log.Information("Compression requested");
            
            // TODO move to db service
            try {
                using (var db = new DatabaseContext()) {
                    await db.AddAsync(new SyncRule {
                        Source = o.Source ?? throw new ArgumentException("Missing sync source path."),
                        Destination = o.Destination ?? throw new ArgumentException("Missing sync destination path."),
                        ShouldZip = o.ShouldZip,
                    });
                    await db.SaveChangesAsync();
                }
            } catch (Exception e) {
                Log.Error(e, "Failed to add sync rule!");
                return 1;
            }

            Log.Information("Rule successfully added.");
            return 0;
        }

        private static async Task<int> RemoveSyncRule(RemoveOptions o)
        {
            Setup.Logger(o.Verbose);
            Log.Information("Removing sync rule {RuleIndex}", o.Index);

            // TODO implement in db service

            return 0;
        }
    }
}
