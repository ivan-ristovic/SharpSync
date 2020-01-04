using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        internal static Task Main(string[] args)
        {
            return Parser.Default.ParseArguments<ListOptions, AddOptions, RemoveOptions>(args)
                .MapResult(
                    (ListOptions o) => ListSyncRules(o),
                    (AddOptions o) => AddSyncRule(o),
                    (RemoveOptions o) => RemoveSyncRule(o),
                    errs => Task.FromResult(1)
                );
        }


        private static async Task ListSyncRules(ListOptions _)
        {
            Setup.Logger(verbose: false);

            IReadOnlyList<SyncRule> rules = await DatabaseService.GetAllSyncRules();
            int padWidth = "Destination".Length;
            if (rules.Any()) {
                int maxSrcWidth = Math.Max(padWidth, rules.Max(r => r.Source.Length));
                int maxDstWidth = Math.Max(padWidth, rules.Max(r => r.Destination.Length));
                padWidth = Math.Max(maxSrcWidth, maxDstWidth);
            }
            int tableWidth = padWidth + 9;

            var sb = new StringBuilder(Environment.NewLine);
            AppendBorder(sb, tableWidth);
            AppendHeader(sb, padWidth);
            AppendBorder(sb, tableWidth);
            AppendBorder(sb, tableWidth);
            if (rules.Any())
                sb.AppendJoin(Environment.NewLine, rules.Select(r => r.ToTableRow(padWidth))).AppendLine();
            Log.Information("Registered sync rules: {Rules}", sb.ToString());


            static StringBuilder AppendHeader(StringBuilder sb, int padWidth)
                => sb.Append("|   ID | ").Append("Source".PadRight(padWidth)).AppendLine(" |")
                     .Append("| Zip? | ").Append("Destination".PadRight(padWidth)).AppendLine(" |");

            static StringBuilder AppendBorder(StringBuilder sb, int width)
                => sb.Append('+').Append('-', width).Append('+').AppendLine();
        }

        private static Task AddSyncRule(AddOptions o)
        {
            Setup.Logger(o.Verbose);
            Log.Information("Adding sync rule {Source} -> {Destination}", o.Source, o.Destination);
            if (o.ShouldZip)
                Log.Information("Compression requested.");

            if (string.IsNullOrWhiteSpace(o.Source) || string.IsNullOrWhiteSpace(o.Destination)) { 
                Log.Fatal("Need to provide source and destination paths");
                return Task.CompletedTask;
            }

            return DatabaseService.AddSyncRule(o.Source, o.Destination, o.ShouldZip);
        }

        private static Task RemoveSyncRule(RemoveOptions o)
        {
            Setup.Logger(o.Verbose);
            if ((o.Indexes is null || !o.Indexes.Any()) && !o.All) {
                Log.Warning("No ID specified to remove. Did you want to remove all sync rules? If so, use option `-a`.");
                return Task.CompletedTask;
            }
            if (o.Indexes.Any())
                Log.Information("Removing sync rule(s): {RuleIndexes}", o.Indexes);
            else
                Log.Information("Removing all sync rules");

            return DatabaseService.RemoveSyncRules(o.Indexes);
        }
    }
}
