using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace wordman.SQLite
{
    public class Example
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExampleID { get; set; }

        public int WordID { get; set; }

        public string Sentence { get; set; }

        public Word Word { get; set; }
    }
}
