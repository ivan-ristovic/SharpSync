using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpSync.Database
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    [Table("destinations")]
    public sealed class DestinationPath
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("path"), Required]
        public string Path { get; set; }


        public HashSet<SyncRule> SyncRules { get; set; }
    }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
