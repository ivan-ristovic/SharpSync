using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharpSync.Common
{
    public sealed class SyncConfig
    {
        [JsonProperty("7z-path")]
        public string? SevenZipPath { get; set; }

        [JsonProperty("rules")]
        public List<SyncRule>? Rules { get; set; }
    }
}
