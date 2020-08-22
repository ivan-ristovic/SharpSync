using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            if (this.Config.Rules.Any(sr => sr.SrcPath.IsParentPathOf(rule.SrcPath) && !sr.Options.TopDirectoryOnly)) {
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

                    if (!await this.ProcessDirectoryAsync(rule.SrcPath, rule.DstPath, rule.Options, true))
                        Log.Error("Errors occured while copying");
                } catch (FileNotFoundException e) {
                    Log.Error(e, "Source/Destination not found for rule:{NL}{Rule}", Environment.NewLine, rule.ToTableRow(printTopLine: true));
                } catch (IOException e) {
                    Log.Error(e, "Failed to copy rule:{NL}{Rule}", Environment.NewLine, rule.ToTableRow(printTopLine: true));
                }
            }

            Log.Information("Finished syncing");
        }


        private async Task<bool> ProcessDirectoryAsync(string srcPath, string dstPath, SyncRuleOptions cfg, bool top = false)
        {
            var srcDir = new DirectoryInfo(srcPath);
            var dstDir = new DirectoryInfo(dstPath);

            if (Utilities.IsExempted(srcDir.Name, cfg.IncludeDirs, cfg.ExcludeDirs))
                return true;

            // TODO zip

            if (!dstDir.Exists) {
                Log.Information("Creating directory: {0}", dstDir.FullName);
                dstDir.Create();
            }

            IReadOnlyDictionary<string, FileInfo> srcFiles = Utilities.GetFiles(srcDir, cfg);
            IReadOnlyDictionary<string, FileInfo> dstFiles = Utilities.GetFiles(dstDir, cfg);

            foreach ((string _, FileInfo srcFile) in srcFiles) {
                if (Utilities.IsExempted(srcFile.Name, cfg.IncludeFiles, cfg.ExcludeFiles))
                    continue;
                FileInfo? dstFile = dstFiles.GetValueOrDefault(srcFile.Name);
                if (!Utilities.AreSynced(srcFile, dstFile)) {
                    string dstFullPath = Path.Combine(dstPath, srcFile.Name);
                    try {
                        if (dstFile?.IsReadOnly ?? false)
                            dstFile.IsReadOnly = false;
                        Log.Information("Copying: {0} -> {1}", srcFile.FullName, Path.GetFullPath(dstFullPath));
                        if (Utilities.IsHidden(srcFile))
                            srcFile.CopyTo(dstFullPath, true);
                        else
                            await Utilities.CopyFileAsync(srcFile.FullName, dstFullPath);
                        File.SetAttributes(dstFullPath, srcFile.Attributes);
                        File.SetLastWriteTime(dstFullPath, srcFile.LastWriteTime);
                        File.SetLastWriteTimeUtc(dstFullPath, srcFile.LastWriteTimeUtc);
                    } catch (Exception ex) {
                        Log.Error(ex, "Failed to copy: {0} -> {1}", srcFile.FullName, dstFullPath);
                        return false;
                    }
                }
            }

            if (cfg.DeleteExtraFiles) {
                foreach ((string _, FileInfo dstFile) in dstFiles) {
                    if (!srcFiles.ContainsKey(dstFile.Name)) {
                        if (Utilities.IsExempted(dstFile.Name, null, cfg.DelExcludeFiles))
                            continue;
                        try {
                            Log.Information("Deleting: {0}", dstFile.FullName);
                            dstFile.IsReadOnly = false;
                            dstFile.Delete();
                        } catch (Exception ex) {
                            Log.Error(ex, "Failed to delete: {0}", dstFile.FullName);
                            return false;
                        }
                    }
                }
            }

            IReadOnlyDictionary<string, DirectoryInfo> srcSubDirs = Utilities.GetDirs(srcDir, cfg);
            IReadOnlyDictionary<string, DirectoryInfo> dstSubDirs = Utilities.GetDirs(dstDir, cfg);

            bool succ = true;
            if (!top || !cfg.TopDirectoryOnly) {
                foreach ((string _, DirectoryInfo srcSubDir) in srcSubDirs)
                    succ |= await this.ProcessDirectoryAsync(srcSubDir.FullName, Path.Combine(dstPath, srcSubDir.Name), cfg, false);
            }

            if (cfg.DeleteExtraDirs) {
                foreach ((string _, DirectoryInfo dstSubDir) in dstSubDirs) {
                    if (!srcSubDirs.ContainsKey(dstSubDir.Name)) {
                        if (Utilities.IsExempted(dstSubDir.Name, null, cfg.DelExcludeDirs))
                            continue;
                        try {
                            Log.Information("Deleting directory: {0}", dstSubDir.FullName);
                            Utilities.DeleteDirectory(dstSubDir);
                        } catch (Exception ex) {
                            Log.Error(ex, "Failed to delete directory: {0}", dstSubDir.FullName);
                            return false;
                        }
                    }
                }
            }

            return succ;
        }
    }
}
