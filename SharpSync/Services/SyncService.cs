using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlinkSyncLib;
using Newtonsoft.Json;
using Serilog;
using SharpSync.Common;
using SharpSync.Extensions;

namespace SharpSync.Services
{
    internal sealed class SyncService
    {
        public static readonly string DefaultConfigPath = "config.json";

        private SyncConfig Config { get; set; }
        private string ConfigPath { get; set; }


        public SyncService()
        {
            this.Config = new SyncConfig();
            this.ConfigPath = DefaultConfigPath;
        }


        public async Task<bool> LoadConfigAsync(string path)
        {
            try {
                string json = await File.ReadAllTextAsync(path);
                this.Config = JsonConvert.DeserializeObject<SyncConfig>(json);
                this.ConfigPath = path;
            } catch (Exception e) {
                Log.Fatal(e, "An exception occured while reading the configuration file: {ConfigPath}", path);
                return false;
            }
            return true;
        }

        public Task WriteConfigAsync(SyncConfig? cfg = null, string? path = null)
            => File.WriteAllTextAsync(path ?? this.ConfigPath, JsonConvert.SerializeObject(cfg ?? this.Config, Formatting.Indented));

        public Task AddRuleAsync(SyncRule rule)
        {
            this.Config.Rules ??= new List<SyncRule>();
            if (rule.Id == 0 || this.Config.Rules.Any(sr => sr.Id == rule.Id))
                rule.Id = this.Config.Rules.Max(sr => sr.Id) + 1;

            if (this.Config.Rules.Any(sr => sr.SrcPath.IsParentPathOf(rule.SrcPath) && !sr.TopDirectoryOnly)) {
                Log.Warning("This rule is already covered by another rule.");
                return Task.CompletedTask;
            }

            this.Config.Rules.Add(rule);
            return this.WriteConfigAsync();
        }

        public Task RemoveRulesAsync(IEnumerable<int>? ids)
        {
            if (ids is { }) {
                this.Config.Rules?.RemoveAll(sr => ids.Contains(sr.Id));
                return this.WriteConfigAsync();
            }
            return Task.CompletedTask;
        }

        public IReadOnlyList<SyncRule> GetRules()
            => this.Config.Rules?.AsReadOnly() ?? new List<SyncRule>().AsReadOnly();

        public async Task SyncAsync(SyncOptions _)
        {
            if (this.Config.Rules is null || !this.Config.Rules.Any()) {
                Log.Warning("Nothing to sync");
                return;
            }

            foreach (SyncRule rule in this.Config.Rules) {
                Log.Debug("Processing rule:{NL}{Src} -> {Dst}", Environment.NewLine, rule.SrcPath, rule.DstPath);
                try {
                    FileAttributes srcAttrs = File.GetAttributes(rule.SrcPath);
                    FileAttributes dstAttrs = File.GetAttributes(rule.DstPath);

                    if (!dstAttrs.HasFlag(FileAttributes.Directory) || !srcAttrs.HasFlag(FileAttributes.Directory)) {
                        Log.Error("Paths have to point to directories:{NL}{Rule}", Environment.NewLine, rule.ToTableRow(printTopLine: true));
                        continue;
                    }

                    await this.ProcessRuleAsync(rule);
                } catch (FileNotFoundException e) {
                    Log.Error(e, "Source/Destination not found for rule:{NL}{Rule}", Environment.NewLine, rule.ToTableRow(printTopLine: true));
                } catch (IOException e) {
                    Log.Error(e, "Failed to copy rule:{NL}{Rule}", Environment.NewLine, rule.ToTableRow(printTopLine: true));
                }
            }

            Log.Information("Finished syncing");
        }


        private async Task ProcessRuleAsync(SyncRule rule)
        {
            var sync = new Sync(rule.SrcPath, rule.DstPath);
            var conf = new InputParams {
                DeleteFromDest = rule.DeleteExtra,
                ExcludeHidden = rule.IncludeHidden,
            };
            if (rule.ExcludeDirs?.Any() ?? false)
                conf.ExcludeDirs = rule.ExcludeDirs?.ToArray();
            if (rule.ExcludeFiles?.Any() ?? false)
                conf.ExcludeFiles = rule.ExcludeFiles?.ToArray();
            if (rule.IncludeDirs?.Any() ?? false)
                conf.IncludeDirs = rule.IncludeDirs?.ToArray();
            if (rule.IncludeFiles?.Any() ?? false)
                conf.IncludeFiles = rule.IncludeFiles?.ToArray();
            sync.Log = m => Log.Debug("SyncLib: {SyncLibLogMessage}", m);
            sync.Start(conf);
        }
    }
}
