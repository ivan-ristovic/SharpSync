using System.Collections.Generic;
using System.Text.RegularExpressions;
using CommandLine;

namespace SharpSync.Common
{
    internal abstract class SyncOptionsBase
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('c', "config", Required = false, HelpText = "Configuration file path.")]
        public string? ConfigPath { get; set; }
    }


    [Verb("add", HelpText = "Add a new sync rule.")]
    internal sealed class AddOptions : SyncOptionsBase
    {
        [Value(0, Required = true, HelpText = "Source path.")]
        public string? Source { get; set; }

        [Value(1, Required = true, HelpText = "Destination path.")]
        public string? Destination { get; set; }

        [Option('z', "zip", Required = false, HelpText = "Create archive instead of regular copy.")]
        public bool ShouldZip { get; set; }

        [Option('t', "top", Required = false, HelpText = "Top directory only.")]
        public bool TopDirectoryOnly { get; set; }

        [Option("delete-extra-files", Required = false, HelpText = "Delete files from destination path which aren't found in source path.")]
        public bool DeleteExtraFiles { get; set; }

        [Option("delete-extra-dirs", Required = false, HelpText = "Delete directories from destination path which aren't found in source path.")]
        public bool DeleteExtraDirs { get; set; }

        [Option('h', "hidden", Required = false, HelpText = "Include hidden files.")]
        public bool IncludeHidden { get; set; }

        [Option("exclude-dirs", SetName = "exclude", Required = false, HelpText = "Directories to exclude (regex).")]
        public IEnumerable<Regex>? ExcludeDirs { get; set; }

        [Option("exclude-files", SetName = "exclude", Required = false, HelpText = "Files to exclude (regex).")]
        public IEnumerable<Regex>? ExcludeFiles { get; set; }

        [Option("include-dirs", SetName = "include", Required = false, HelpText = "Directories to include (regex).")]
        public IEnumerable<Regex>? IncludeDirs { get; set; }

        [Option("include-files", SetName = "include", Required = false, HelpText = "Files to include (regex).")]
        public IEnumerable<Regex>? IncludeFiles { get; set; }

        [Option("del-exclude-dirs", SetName = "exclude", Required = false, HelpText = "Directories to exclude from deletion (regex).")]
        public IEnumerable<Regex>? DelExcludeDirs { get; set; }

        [Option("del-exclude-files", SetName = "exclude", Required = false, HelpText = "Files to exclude from deletion (regex).")]
        public IEnumerable<Regex>? DelExcludeFiles { get; set; }
    }

    [Verb("remove", HelpText = "Remove existing sync rule.")]
    internal sealed class RemoveOptions : SyncOptionsBase
    {
        [Option('a', "all", Required = false, HelpText = "Remove all sync rules.")]
        public bool All { get; set; }

        [Value(0, Required = false, HelpText = "IDs of rule(s) to remove.")]
        public IEnumerable<int>? Indexes { get; set; }

        // TODO remove by source path
    }

    [Verb("list", HelpText = "List all registered sync rules.")]
    internal sealed class ListOptions : SyncOptionsBase
    {
        // TODO filters?
        // TODO search
    }

    [Verb("sync", HelpText = "Synchronize.")]
    internal sealed class SyncOptions : SyncOptionsBase
    {

    }

    // TODO export, import, activate, deactivate
}
