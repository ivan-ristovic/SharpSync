using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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


        public string ToTableRow(int? padSrc = null, int? padDst = null)
        {
            string src = padSrc is null ? this.Source : this.Source.PadRight(padSrc.Value);
            string dst = padDst is null ? this.Destination : this.Destination.PadRight(padDst.Value);
            return $"| {this.Id.ToString().PadLeft(4, ' ')} | {src} | {dst} | {(this.ShouldZip ? "Yes ": " No ")} |";
        }
    }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
