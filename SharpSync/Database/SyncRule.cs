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


        public override string ToString() 
            => $"{this.Id.ToString().PadLeft(4, ' ')}: {this.Source} -> {this.Destination} {(this.ShouldZip ? "(z)": "")}";
    }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
