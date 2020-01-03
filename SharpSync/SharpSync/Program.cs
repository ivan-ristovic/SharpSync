using System;
using System.Threading.Tasks;
using CommandLine;
using Serilog;
using SharpSync.Common;

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


        private static async Task<int> ListSyncRules(ListOptions o)
        {
            Setup.Logger(verbose: false);
            Log.Information("Registered sync rules:");
            return 0;
        }

        private static async Task<int> AddSyncRule(AddOptions o)
        {
            Setup.Logger(o.Verbose);
            Log.Information("Adding sync rule {Source} -> {Destination}", o.Source, o.Destination);
            if (o.ShouldZip)
                Log.Information("Compression requested");
            return 0;
        }

        private static async Task<int> RemoveSyncRule(RemoveOptions o)
        {
            Setup.Logger(o.Verbose);
            Log.Information("Removing sync rule {RuleIndex}", o.Index);
            return 0;
        }
    }
}
