using System.Collections.Generic;
using CommandLine;

namespace SharpSync.Common
{
    [Verb("add", HelpText = "Add a new sync rule.")]
    internal sealed class AddOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Value(0, Required = true, HelpText = "Source path.")]
        public string? Source { get; set; }

        [Value(1, Required = true, HelpText = "Destination path.")]
        public string? Destination { get; set; }

        [Option('z', "zip", Required = false, HelpText = "Create archive instead of regular copy.")]
        public bool ShouldZip { get; set; }
    }

    [Verb("remove", HelpText = "Remove existing sync rule.")]
    internal sealed class RemoveOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('a', "all", Required = false, HelpText = "Remove all sync rules.")]
        public bool All { get; set; }

        [Value(0, Required = false, HelpText = "IDs of rule(s) to remove.")]
        public IEnumerable<int>? Indexes { get; set; }
    }

    [Verb("list", HelpText = "List all registered sync rules.")]
    internal sealed class ListOptions
    {
        // TODO filters?
        // TODO search
    }

    [Verb("sync", HelpText = "Synchronize.")]
    internal sealed class SyncOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
    }

    // TODO export, import, activate, deactivate
}
