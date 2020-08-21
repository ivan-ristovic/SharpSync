using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;
using Serilog;
using SharpSync.Common;
using SharpSync.Services;

namespace SharpSync
{
    internal static class Program
    {
        private static SyncService Service { get; } = new SyncService();


        internal static Task Main(string[] args)
        {
            return Parser.Default.ParseArguments<ListOptions, AddOptions, RemoveOptions, SyncOptions>(args)
                .MapResult(
                    (ListOptions o) => ListSyncRulesAsync(o),
                    (AddOptions o) => AddSyncRuleAsync(o),
                    (RemoveOptions o) => RemoveSyncRulesAsync(o),
                    (SyncOptions o) => SynchronizeAsync(o),
                    errs => Task.FromResult(1)
                );
        }


        private static async Task InitializeServiceAsync(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = SyncService.DefaultConfigPath;

            if (!File.Exists(path)) {
                Log.Error("Configuration file {ConfigPath} not found!", path);
                Log.Information("Do you want to create a new blank config file at given path? (y/N)");
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Y) {
                    var defConfig = new SyncConfig();
                    defConfig.ZipExePath = "insert_zip_program_path_here";
                    defConfig.Rules = new List<SyncRule> {
                        new SyncRule { Id = 1, SrcPath = "insert_src_path", DstPath = "insert_dst_path" },
                        new SyncRule { Id = 2, SrcPath = "insert_src_path", DstPath = "insert_dst_path" },
                    };
                    await Service.WriteConfigAsync(defConfig, path);
                    Log.Information("Empty config file created. Please refill it and restart the program");
                    Environment.Exit(1);
                }
            } else {
                await Service.LoadConfigAsync(path);
            }
        }

        private static async Task ListSyncRulesAsync(ListOptions o)
        {
            Setup.Logger(verbose: false);
            await InitializeServiceAsync(o.ConfigPath);

            IReadOnlyList<SyncRule> rules = Service.GetRules();
            int padWidth = "Destination".Length;
            if (rules.Any()) {
                int maxSrcWidth = Math.Max(padWidth, rules.Max(r => r.SrcPath.Length));
                int maxDstWidth = Math.Max(padWidth, rules.Max(r => r.DstPath.Length));
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

        private static async Task AddSyncRuleAsync(AddOptions o)
        {
            Setup.Logger(o.Verbose);
            Log.Information("Adding sync rule {Source} -> {Destination}", o.Source, o.Destination);
            if (o.ShouldZip)
                Log.Information("Compression requested.");

            if (string.IsNullOrWhiteSpace(o.Source) || string.IsNullOrWhiteSpace(o.Destination)) {
                Log.Fatal("Need to provide source and destination paths");
                return;
            }

            await InitializeServiceAsync(o.ConfigPath);
            await Service.AddRuleAsync(new SyncRule {
                SrcPath = o.Source,
                DstPath = o.Destination,
                ShouldZip = o.ShouldZip,
                TopDirectoryOnly = o.TopDirectoryOnly,
            });
        }

        private static async Task RemoveSyncRulesAsync(RemoveOptions o)
        {
            Setup.Logger(o.Verbose);

            if ((o.Indexes is null || !o.Indexes.Any()) && !o.All) {
                Log.Warning("No ID specified to remove. Did you want to remove all sync rules? If so, use option `-a`.");
                return;
            }
            if (o.Indexes.Any())
                Log.Information("Removing sync rule(s): {RuleIndexes}", o.Indexes);
            else
                Log.Information("Removing all sync rules");

            await InitializeServiceAsync(o.ConfigPath);
            await Service.RemoveRulesAsync(o.All ? Service.GetRules().Select(r => r.Id) : o.Indexes);
        }

        private static async Task SynchronizeAsync(SyncOptions o)
        {
            Setup.Logger(o.Verbose);
            await InitializeServiceAsync(o.ConfigPath);
            await Service.SyncAsync(o);
        }
    }
}
