using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpSync.Database
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    [Table("rules")]
    public sealed class SyncRule
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("src"), Required]
        public string Source { get; set; }

        [Column("dst"), Required]
        public string Destination { get; set; }

        [Column("zip")]
        public bool ShouldZip { get; set; }

        [NotMapped]
        public string SourcePath {
            get {
                if (this.srcUri is null)
                    this.srcUri = Path.GetFullPath(this.Source);
                return this.srcUri;
            }
        }

        [NotMapped]
        public string DestinationPath {
            get {
                if (this.dstUri is null)
                    this.dstUri = Path.GetFullPath(this.Destination);
                return this.dstUri;
            }
        }


        private string? srcUri;
        private string? dstUri;


        public string ToTableRow(int? padWidth = null, bool printTopLine = false)
        {
            padWidth = new[] { padWidth ?? 0, this.Source.Length, this.Destination.Length }.Max();
            string src = this.Source.PadRight(padWidth.Value);
            string dst = this.Destination.PadRight(padWidth.Value);
            var sb = new StringBuilder();
            if (printTopLine)
                sb.Append('+').Append('-', padWidth.Value + 9).Append('+').AppendLine();
            sb.Append("| ").Append(this.Id.ToString().PadLeft(4, ' ')).Append(" | ").Append(src).AppendLine(" |");
            sb.Append("| ").Append(this.ShouldZip ? " Yes" : "  No").Append(" | ").Append(dst).AppendLine(" |");
            sb.Append('+').Append('-', padWidth.Value + 9).Append('+');
            return sb.ToString();
        }
    }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
