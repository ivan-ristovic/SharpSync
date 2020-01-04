using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace SharpSync.Database
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    [Table("rules")]
    public class SyncRule
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("src"), Required]
        public int SourceId { get; set; }

        [ForeignKey("SourceId")]
        public SourcePath Source { get; set; }
        
        [Column("dst"), Required]
        public int DestinationId { get; set; }

        [ForeignKey("DestinationId")]
        public DestinationPath Destination { get; set; }

        [Column("zip")]
        public bool ShouldZip { get; set; }


        public string ToTableRow(int? padWidth = null, bool printTopLine = false)
        {
            padWidth = new[] { padWidth ?? 0, this.Source.Path.Length, this.Destination.Path.Length }.Max();
            string src = this.Source.Path.PadRight(padWidth.Value);
            string dst = this.Destination.Path.PadRight(padWidth.Value);
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
