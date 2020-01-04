using System;
using System.Collections.Generic;
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


        private static async Task ListSyncRules(ListOptions o)
        {
            Setup.Logger(verbose: false);

            IReadOnlyList<SyncRule> rules = await DatabaseService.GetAllSyncRules();
            int maxSrcWidth = rules.Max(r => r.Source.Length);
            int maxDstWidth = rules.Max(r => r.Destination.Length);
            int tableWidth = maxSrcWidth + maxDstWidth + 19;

            var sb = new StringBuilder(Environment.NewLine);
            AppendBorder(sb, tableWidth);
            AppendHeader(sb, maxSrcWidth, maxDstWidth);
            AppendBorder(sb, tableWidth);
            sb.AppendJoin(Environment.NewLine, rules.Select(r => r.ToTableRow(maxSrcWidth, maxDstWidth))).AppendLine();
            AppendBorder(sb, tableWidth);
            Log.Information("Registered sync rules: {Rules}", sb.ToString());


            static StringBuilder AppendHeader(StringBuilder sb, int srcWidth, int dstWidth)
                => sb.Append("|   ID | ")
                     .Append("Source".PadRight(srcWidth))
                     .Append(" | ")
                     .Append("Destination".PadRight(dstWidth))
                     .Append(" | Zip? |").AppendLine();

            static StringBuilder AppendBorder(StringBuilder sb, int width)
                => sb.Append('+').Append('-', width).Append('+').AppendLine();
        }

        private static Task AddSyncRule(AddOptions o)
        {
            Setup.Logger(o.Verbose);
            Log.Information("Adding sync rule {Source} -> {Destination}", o.Source, o.Destination);
            if (o.ShouldZip)
                Log.Information("Compression requested.");

            return DatabaseService.AddSyncRule(new SyncRule {
                Source = o.Source ?? throw new ArgumentException("Missing sync source path"),
                Destination = o.Destination ?? throw new ArgumentException("Missing sync destination path"),
                ShouldZip = o.ShouldZip,
            });
        }

        private static Task RemoveSyncRule(RemoveOptions o)
        {
            Setup.Logger(o.Verbose);
            if (o.Index is { }) {
                Log.Information("Removing sync rule {RuleIndex}", o.Index);
                return DatabaseService.RemoveSyncRule(o.Index.Value);
            } else {
                Log.Error("Rule index missing");
                return Task.CompletedTask;
            }
        }
    }
}
