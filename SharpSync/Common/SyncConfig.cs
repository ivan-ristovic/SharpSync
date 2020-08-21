using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharpSync.Common
{
    public sealed class SyncConfig
    {
        [JsonProperty("zip-path")]
        public string? ZipExePath { get; set; }

        [JsonProperty("rules")]
        public List<SyncRule>? Rules { get; set; }
    }
}
