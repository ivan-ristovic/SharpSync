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

        [Value(0, Required = true, HelpText = "Index of rule to remove.")]
        public int? Index { get; set; }
    }

    [Verb("list", HelpText = "List all registered sync rules.")]
    internal sealed class ListOptions
    {

    }
}
