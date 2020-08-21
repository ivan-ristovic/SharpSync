using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SharpSync.Common
{
    public sealed class SyncRule
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("src", Required = Required.Always)]
        public string SrcPath { get; set; } = "";

        [JsonProperty("dst", Required = Required.Always)]
        public string DstPath { get; set; } = "";

        [JsonProperty("zip")]
        public bool ShouldZip { get; set; }

        [JsonProperty("top")]
        public bool TopDirectoryOnly { get; set; }


        public string ToTableRow(int? padWidth = null, bool printTopLine = false)
        {
            padWidth = new[] { padWidth ?? 0, this.SrcPath.Length, this.DstPath.Length }.Max();
            string src = this.SrcPath.PadRight(padWidth.Value);
            string dst = this.DstPath.PadRight(padWidth.Value);
            var sb = new StringBuilder();
            if (printTopLine)
                sb.Append('+').Append('-', padWidth.Value + 9).Append('+').AppendLine();
            sb.Append("| ").Append(this.Id.ToString().PadLeft(4, ' ')).Append(" | ").Append(src).AppendLine(" |");
            sb.Append("| ").Append(this.ShouldZip ? " Yes" : "  No").Append(" | ").Append(dst).AppendLine(" |");
            sb.Append('+').Append('-', padWidth.Value + 9).Append('+');
            return sb.ToString();
        }
    }
}
